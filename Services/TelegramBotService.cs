using System.Net;
using BtcMiner.Entity;
using BtcMiner.Helpers;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BtcMiner.Services
{
    class TelegramBotService : ITelegramBotService
    {
        private readonly AppSettings _appSettings;

        private TelegramBotClient _client;

        public TelegramBotService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _client = new TelegramBotClient(_appSettings.BotToken);
        }

        bool ITelegramBotService.CheckChannelMember(BtcMiner.Entity.User user, string channelName)
        {
            try
            {
                ChatMember member = _client
                    .GetChatMemberAsync(
                        new ChatId(channelName),
                        long.Parse(user.TelegramId)
                    )
                    .Result;

                return
                    member.Status == ChatMemberStatus.Creator
                    || member.Status == ChatMemberStatus.Administrator
                    || member.Status == ChatMemberStatus.Member
                    ? true
                    : false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
