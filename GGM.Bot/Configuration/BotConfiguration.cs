using System;
using Discord;

namespace GGM.Bot.Configuration
{
    public class BotConfiguration
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public LogSeverity LogLevel { get; set; }
    }
}