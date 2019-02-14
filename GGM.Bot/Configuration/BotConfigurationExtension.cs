using Discord.Commands;
using Discord.WebSocket;

namespace GGM.Bot.Configuration
{

    static class BotConfigurationExtension
    {
        public static DiscordSocketConfig ToDiscordSocketConfig(this BotConfiguration self) => new DiscordSocketConfig
        {
            LogLevel = self.LogLevel,
        };

        public static CommandServiceConfig ToCommandServiceConfig(this BotConfiguration self) => new CommandServiceConfig
        {
           LogLevel = self.LogLevel,
            DefaultRunMode = RunMode.Async
        };
    }
}