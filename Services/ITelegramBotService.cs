using BtcMiner.Entity;

namespace BtcMiner.Services
{
    public interface ITelegramBotService
    {
        bool CheckChannelMember(User user);
    }
}