using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
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

        public AuthenticationService(IOptions<AppSettings> appSettings, MinerDb minerDb)
        {
            _appSettings = appSettings.Value;
            _minerDb = minerDb;
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
                };
                _minerDb.Users.Add(u);
                _minerDb.SaveChanges();
            }

            // check refferer for write refferal
            if (!model.ReffererId.IsNullOrEmpty())
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

        public IEnumerable<User> GetAllUsers()
        {
            throw new NotImplementedException();
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
                    .UserTasks.Where(x => x.UserId == user.Id)
                    .Include(x => x.Task)
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
                        ProfilePicUrl = refral.ProfilePicUrl
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
                ProfilePicUrl = model.ProfilePicUrl
            };
            minerDb.Referals.Add(r);
            minerDb.SaveChanges();

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
                        Min = checkResponse.RemainTime.Minutes,
                        Hour = checkResponse.RemainTime.Hours,
                        Sec = checkResponse.RemainTime.Seconds,
                        State = checkResponse.State
                    },
                    Message = "Ok",
                    StatusCode = StatusCodes.Status207MultiStatus
                };
            }

            var t = new Transaction { UserId = user.Id, Type = TransactionType.Claim };

            // add user btc balance
            var newBalance = user.BtcBalance + _appSettings.AddBalance;
            _minerDb
                .Users.Where(u => u.Id == user.Id)
                .ExecuteUpdate(u =>
                    u.SetProperty(p => p.BtcBalance, user.BtcBalance + _appSettings.AddBalance)
                );

            _minerDb.Transactions.Add(t);
            _minerDb.SaveChanges();

            // send  hour to reset
            return new AuthResponse
            {
                Message = "Ok",
                Data = new
                {
                    Min = 0,
                    Hour = _appSettings.ClaimTimeInHour,
                    Sec = 0,
                    Balance = user.BtcBalance + _appSettings.AddBalance
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
                return new ClaimStatus
                {
                    State = ClaimStatus.CAN,
                    RemainTime = new TimeSpan(_appSettings.ClaimTimeInHour, 0, 0)
                };
            }

            var remianTime = DateTime.Now - LastTransaction.Created;

            if (remianTime.Hours > _appSettings.ClaimTimeInHour)
            {
                // user can Claim
                return new ClaimStatus
                {
                    State = ClaimStatus.CAN,
                    RemainTime = new TimeSpan(_appSettings.ClaimTimeInHour, 0, 0)
                };
            }
            else
            {
                //  user cant Claim
                var time = new TimeSpan(_appSettings.ClaimTimeInHour, 0, 0) - remianTime;
                return new ClaimStatus { State = ClaimStatus.CANT, RemainTime = time };
            }
        }

        public AuthResponse CheckTask(User? user, CheckTaskRequest request)
        {
            var userTask = _minerDb
                .UserTasks.Where(ut => ut.TaskId == request.taskId && ut.UserId == user!.Id)
                .Include(ut => ut.Task)
                .FirstOrDefault();

            if (userTask!.Task!.Type == TaskTypes.CHECK_CODE)
            {
                // check task code
                var checkResp = userTask.Task.Value == request.TaskData;
                return new AuthResponse
                {
                    Message = "Ok",
                    StatusCode = checkResp
                        ? StatusCodes.Status406NotAcceptable
                        : StatusCodes.Status200OK,
                    Data = new { result = checkResp }
                };
            }
            else if (userTask.Task.Type == TaskTypes.JOIN && userTask.Task.Name == "Telegram")
            {
                // check join channel
            }

            return new AuthResponse
            {
                Message = "Ok",
                StatusCode =
                    userTask == null ? StatusCodes.Status406NotAcceptable : StatusCodes.Status200OK,
                Data = new { result = userTask == null ? false : true }
            };
        }

        public AuthResponse DoTask(User? user)
        {
            throw new NotImplementedException();
        }

        public AuthResponse ListTasks(User? user)
        {
            return new AuthResponse
            {
                Message = "OK",
                Data = _minerDb
                    .Tasks.Where(t =>
                        _minerDb.UserTasks.FirstOrDefault(ut => ut.TaskId == t.Id) == null
                            ? true
                            : false
                    )
                    .ToList()
            };
        }
    }
}
