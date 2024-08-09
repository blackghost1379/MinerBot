using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using BtcMiner.Entity;
using BtcMiner.Helpers;
using BtcMiner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BtcMiner.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppSettings _appSettings;
        private readonly MinerDb _minerDb;

        private readonly ITelegramBotService _botService;

        public AuthenticationService(
            IOptions<AppSettings> appSettings,
            MinerDb minerDb,
            ITelegramBotService botService
        )
        {
            _appSettings = appSettings.Value;
            _minerDb = minerDb;
            _botService = botService;
        }

        public AuthResponse? Authenticate(AuthRequest model)
        {
            var u = _minerDb.Users.FirstOrDefault(x => x.TelegramId == model.UserId);

            if (u == null)
            {
                // register user
                u = new User
                {
                    TelegramId = model.UserId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Username = model.Username,
                    LanguageCode = model.LanguageCode,
                    AllowWritePm = model.AllowWritePm,
                    IsPremium = model.IsPremium,
                    ProfilePicUrl = model.ProfilePicUrl,
                    ClaimCoins = _appSettings.DefaultClaimCoinsPerHour,
                    ClaimHour = _appSettings.DefaultClaimHour,
                    ClaimRefferal = _appSettings.DefaultInviteCoinsPerUser
                };
                _minerDb.Users.Add(u);
                _minerDb.SaveChanges();
            }

            // check refferer for write refferal
            if (!model.ReffererId.IsNullOrEmpty() && model.ReffererId != u.TelegramId)
            {
                var refferal = AddRefferal(_minerDb, model);
            }

            // Generate Token for users
            var token = GenerateJwtToken(u);
            var checkResp = CheckCanClaim(u);

            return new AuthResponse
            {
                Message = "Login Success",
                Data = new
                {
                    Token = token,
                    Info = new
                    {
                        Time = DateTime.Now.ToString(),
                        BtcBlance = u.BtcBalance,
                        Balance = u.Balance,
                        ClaimCoins = u.ClaimCoins,
                        ClaimHour = u.ClaimHour,
                        ClaimRefferal = u.ClaimRefferal,
                        ClaimRemainTime = new
                        {
                            Min = checkResp.RemainTime.Minutes,
                            Hour = checkResp.RemainTime.Hours,
                            Sec = checkResp.RemainTime.Seconds,
                            State = checkResp.State
                        }
                    }
                }
            };
        }


        public User? GetById(int id)
        {
            return _minerDb.Users.FirstOrDefault(x => x.Id == id);
        }

        public AuthResponse GetTasks(User user)
        {
            return new AuthResponse
            {
                Message = "Ok",
                Data = _minerDb
                    .Tasks.Select(t => new
                    {
                        IsComplete = _minerDb
                            .UserTasks.Where(ut => ut.UserId == user.Id && ut.TaskId == t.Id).FirstOrDefault() != null ? _minerDb
                            .UserTasks.Where(ut => ut.UserId == user.Id && ut.TaskId == t.Id).FirstOrDefault()!.Status : BtcMiner.Helpers.TaskStatus.START,
                        Task = t
                    })
                    .ToList(),
            };
        }

        public AuthResponse GetReferals(User user)
        {
            return new AuthResponse
            {
                Message = "Ok",
                Data = _minerDb
                    .Referals.Where(x => x.UserId == user.Id)
                    .Select(refral => new
                    {
                        FirstName = refral.FirstName,
                        LastName = refral.LastName,
                        ProfilePicUrl = refral.ProfilePicUrl,
                        Coin = refral.Coin
                    })
                    .ToList(),
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // makes the Id of user to be the claim identity
                Subject = new ClaimsIdentity(
                    new[]
                    {
                        new Claim("id", user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.TelegramId.ToString()),
                        new Claim("tg_id", user.TelegramId.ToString())
                    }
                ),
                // set the token expiry to a day
                Expires = DateTime.UtcNow.AddDays(1),
                // setting the signing credentials
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature
                )
            };
            // create the token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private Referal? AddRefferal(MinerDb minerDb, AuthRequest model)
        {
            var refferer = minerDb.Users.FirstOrDefault(x => x.TelegramId == model.ReffererId);

            if (refferer == null)
            {
                return null;
            }

            var r = new Referal
            {
                UserId = refferer.Id,
                TelegramId = model.UserId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ProfilePicUrl = model.ProfilePicUrl,
                Coin = refferer.ClaimRefferal
            };
            minerDb.Referals.Add(r);
            minerDb.SaveChanges();
            var inviteReward = refferer.Balance + refferer.ClaimRefferal;
            _minerDb
                .Users.Where(u => u.Id == refferer.Id)
                .ExecuteUpdate(u => u.SetProperty(p => p.Balance, inviteReward));

            return r;
        }

        public AuthResponse Claim(User user)
        {
            var checkResponse = CheckCanClaim(user);
            if (checkResponse.State == ClaimStatus.CANT)
            {
                return new AuthResponse
                {
                    Data = new
                    {
                        Info = new
                        {
                            Time = DateTime.Now.ToString(),
                            BtcBlance = user.BtcBalance,
                            Balance = user.Balance,
                        ClaimCoins = user.ClaimCoins,
                        ClaimHour = user.ClaimHour,
                        ClaimRefferal = user.ClaimRefferal,
                            ClaimRemainTime = new
                            {
                                Min = checkResponse.RemainTime.Minutes,
                                Hour = checkResponse.RemainTime.Hours,
                                Sec = checkResponse.RemainTime.Seconds,
                                State = checkResponse.State
                            }
                        },
                    },
                    Message = "Ok",
                    StatusCode = StatusCodes.Status207MultiStatus
                };
            }

            var t = new Transaction { UserId = user.Id, Type = TransactionType.Claim };

            // add user balance
            var newBalance = user.Balance + user.ClaimCoins;
            _minerDb
                .Users.Where(u => u.Id == user.Id)
                .ExecuteUpdate(u => u.SetProperty(p => p.Balance, newBalance));

            _minerDb.Transactions.Add(t);
            _minerDb.SaveChanges();

            // send  hour to reset
            return new AuthResponse
            {
                Message = "Ok",
                Data = new
                {
                    Info = new
                    {
                        Time = DateTime.Now.ToString(),
                        BtcBlance = user.BtcBalance,
                        Balance = newBalance,
                        ClaimCoins = user.ClaimCoins,
                        ClaimHour = user.ClaimHour,
                        ClaimRefferal = user.ClaimRefferal,
                        ClaimRemainTime = new
                        {
                            Min = 0,
                            Hour = 0,
                            Sec = 0,
                            State = checkResponse.State
                        }
                    },
                },
                StatusCode = StatusCodes.Status200OK
            };
        }

        public AuthResponse GetMe(User? user)
        {
            return new AuthResponse { Message = "Ok", Data = user!, };
        }

        public AuthResponse GetServerTime()
        {
            return new AuthResponse
            {
                Message = "Ok",
                StatusCode = StatusCodes.Status200OK,
                Data = DateTime.Now
            };
        }

        public AuthResponse GetRemainClaimTime(User? user)
        {
            var checkResponse = CheckCanClaim(user!);

            return new AuthResponse
            {
                Data = new
                {
                    Min = checkResponse.RemainTime.Minutes,
                    Hour = checkResponse.RemainTime.Hours,
                    Sec = checkResponse.RemainTime.Seconds,
                    State = checkResponse.State
                },
                Message = "Ok",
                StatusCode = checkResponse.State
                    ? StatusCodes.Status200OK
                    : StatusCodes.Status207MultiStatus
            };
        }

        private ClaimStatus CheckCanClaim(User user)
        {
            var LastTransaction = _minerDb
                .Transactions.OrderBy(x => x.Created)
                .LastOrDefault(x => x.UserId == user.Id && x.Type == TransactionType.Claim);

            if (LastTransaction == null)
            {
                // first step
                var t = new Transaction { UserId = user.Id, Type = TransactionType.Claim };
                _minerDb.Transactions.Add(t);
                _minerDb.SaveChanges();

                var remaineTime = DateTime.Now - t.Created;

                return new ClaimStatus { State = ClaimStatus.CAN, RemainTime = remaineTime };
            }

            var remianTime = DateTime.Now - LastTransaction.Created;

            var appTime = new TimeSpan(
                user.ClaimHour,
                _appSettings.ClaimTimeInMin,
                _appSettings.ClaimTimeInSecond
            );

            if (remianTime > appTime)
            {
                // user can Claim
                return new ClaimStatus
                {
                    State = ClaimStatus.CAN,
                    RemainTime = appTime
                };
            }
            else
            {
                //  user cant Claim
                var time = appTime - remianTime;
                return new ClaimStatus { State = ClaimStatus.CANT, RemainTime = remianTime };
            }
        }

        public AuthResponse DoClaimTask(User? user, int taskId)
        {
            // get task rom db 
            var userTask = _minerDb.UserTasks.Where(ut => ut.UserId == user!.Id && ut.TaskId == taskId).FirstOrDefault();

            if (userTask == null)
            {
                // 
                var ut = new UserTask
                {
                    UserId = user!.Id,
                    TaskId = taskId,
                    Status = BtcMiner.Helpers.TaskStatus.CLAIM
                };
                _minerDb.UserTasks.Add(ut);
                _minerDb.SaveChanges();
            }
            else
            {
                _minerDb
                .UserTasks.Where(u => u.Id == user!.Id && u.TaskId == taskId)
                .ExecuteUpdate(u => u.SetProperty(p => p.Status, BtcMiner.Helpers.TaskStatus.CLAIM));
            }

            return new AuthResponse
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "OK",
                Data = new { }
            };
        }

        public AuthResponse DoCompleteTask(User? user, CheckTaskRequest request)
        {
            var userTask = _minerDb.UserTasks.Where(ut => ut.UserId == user!.Id && ut.TaskId == request.TaskId && ut.Status == BtcMiner.Helpers.TaskStatus.CLAIM).Include(ut => ut.Task).FirstOrDefault();

            if (userTask == null)
            {
                return new AuthResponse
                {
                    StatusCode = StatusCodes.Status406NotAcceptable,
                    Message = "Faild Task Not Found",
                    Data = new { }
                };
            }

            if (userTask.Task!.Type == TaskTypes.CHECK_YOUTUBE_CODE)
            {
                // check task code
                var checkResp = userTask.Task.Value == request.TaskData;

                if (checkResp)
                {
                    // add balance
                    _minerDb
                        .Users.Where(u => u.Id == user!.Id)
                        .ExecuteUpdate(u =>
                            u.SetProperty(p => p.Balance, user!.Balance + userTask.Task!.Balance)
                        );
                }

                _minerDb
                .UserTasks.Where(u => u.Id == user!.Id && u.TaskId == request.TaskId)
                .ExecuteUpdate(u => u.SetProperty(p => p.Status, BtcMiner.Helpers.TaskStatus.DONE));

                return new AuthResponse
                {
                    Message = "Ok",
                    StatusCode = checkResp
                        ? StatusCodes.Status200OK
                        : StatusCodes.Status406NotAcceptable,
                    Data = new { result = checkResp }
                };
            }
            else if (userTask.Task!.Type == TaskTypes.JOIN_TELEGRAM)
            {
                // check join channel
                var memberCheckResp = _botService.CheckChannelMember(user!, userTask.Task!.Value!);

                if (memberCheckResp)
                {
                    // add balance
                    _minerDb
                        .Users.Where(u => u.Id == user!.Id)
                        .ExecuteUpdate(u =>
                            u.SetProperty(p => p.Balance, user!.Balance + userTask.Task!.Balance)
                        );

                    _minerDb
                .UserTasks.Where(u => u.Id == user!.Id && u.TaskId == request.TaskId)
                .ExecuteUpdate(u => u.SetProperty(p => p.Status, BtcMiner.Helpers.TaskStatus.DONE));

                    return new AuthResponse
                    {
                        Message = "Ok",
                        StatusCode = StatusCodes.Status200OK,
                        Data = new { }
                    };
                }
                else
                {
                    return new AuthResponse
                    {
                        Message = "Fail",
                        StatusCode = StatusCodes.Status406NotAcceptable,
                        Data = new { }
                    };
                }
            }
            else if (userTask.Task!.Type == TaskTypes.INVITE)
            {
                // check refferals
                var taskInviteCount = int.Parse(userTask.Task!.Value!);
                var inviteCount = _minerDb.Referals.Where(r => r.UserId == user!.Id).Count();
                var resp = inviteCount >= taskInviteCount;

                if (resp)
                {

                    _minerDb
                        .Users.Where(u => u.Id == user!.Id)
                        .ExecuteUpdate(u =>
                            u.SetProperty(p => p.Balance, user!.Balance + userTask.Task!.Balance)
                        );
                    _minerDb
                .UserTasks.Where(u => u.Id == user!.Id && u.TaskId == request.TaskId)
                .ExecuteUpdate(u => u.SetProperty(p => p.Status, BtcMiner.Helpers.TaskStatus.DONE));
                }

                return new AuthResponse
                {
                    Message = "Ok",
                    StatusCode = resp
                        ? StatusCodes.Status200OK
                        : StatusCodes.Status406NotAcceptable,
                    Data = new { result = resp }
                };
            }else if (userTask.Task!.Type == TaskTypes.BOOST_CHANNEL)
            {
                return new AuthResponse
                {
                    Message = "Not Implement",
                    StatusCode = StatusCodes.Status200OK,
                    Data = new { }
                };
            }
            else
            {

                _minerDb
                    .Users.Where(u => u.Id == user!.Id)
                    .ExecuteUpdate(u =>
                        u.SetProperty(p => p.Balance, user!.Balance + userTask.Task!.Balance)
                    );

                _minerDb
            .UserTasks.Where(u => u.Id == user!.Id && u.TaskId == request.TaskId)
            .ExecuteUpdate(u => u.SetProperty(p => p.Status, BtcMiner.Helpers.TaskStatus.DONE));

                return new AuthResponse
                {
                    Message = "Ok",
                    StatusCode = StatusCodes.Status200OK,
                    Data = new { }
                };
            }



        }
    }
}
