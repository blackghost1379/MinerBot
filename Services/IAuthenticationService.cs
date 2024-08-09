using BtcMiner.Entity;
using BtcMiner.Models;

namespace BtcMiner.Services
{
    public interface IAuthenticationService
    {
        AuthResponse? Authenticate(AuthRequest model);

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
        /// Change Task Status to Claim
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        AuthResponse DoClaimTask(User? user, int taskId);
        AuthResponse DoCompleteTask(User? user, CheckTaskRequest request);
    }
}
