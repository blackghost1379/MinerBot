using BtcMiner.Entity;
using BtcMiner.Models;

namespace BtcMiner.Services
{
    public interface IAuthenticationService
    {
        AuthResponse? Authenticate(AuthRequest model);
        IEnumerable<User> GetAllUsers();
        User? GetById(int id);
        /// <summary>
        /// Returns all completed tasks
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        AuthResponse GetTasks(User user);
        AuthResponse GetReferals(User user);
        AuthResponse Claim(User user);
        AuthResponse GetMe(User? user);
        AuthResponse GetServerTime();
        AuthResponse GetRemainClaimTime(User? user);
        /// <summary>
        /// Checks tasks and completes them if they are unfinished
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        AuthResponse CheckTask(User? user, CheckTaskRequest request);
        AuthResponse DoTask(User? user);
        /// <summary>
        /// Return Remain Task Of User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        AuthResponse ListTasks(User? user);
    }
}
