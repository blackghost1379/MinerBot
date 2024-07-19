using BtcMiner.Entity;
using BtcMiner.Models;

namespace BtcMiner.Services
{
    public interface IAuthenticationService
    {
        AuthResponse? Authenticate(AuthRequest model);
        IEnumerable<User> GetAllUsers();
        User? GetById(int id);
        AuthResponse GetTasks(User user);
        AuthResponse GetReferals(User user);
        AuthResponse Claim(User user);
        AuthResponse GetMe(User? user);
        AuthResponse GetServerTime();
        AuthResponse GetRemainClaimTime(User? user);
    }
}
