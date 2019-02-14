using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GGM.Bot.Configuration;

namespace GGM.Bot.Command
{
    // 추후 상용화 할때는 Performance를 위해 StringBuilder 를 사용하는 것을 고려해보아야함.
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Color EMBED_COLOR = new Color(114, 137, 218);

        private readonly BotConfiguration _configuration;
        private readonly CommandService _commandService;

        public HelpModule(BotConfiguration configuration, CommandService commandService)
        {
            _configuration = configuration;
            _commandService = commandService;
        }

        [Command("help")]
        [Summary("명령어 도움말")]
        public async Task HelpAsync()
        {
            var builder = new EmbedBuilder {Color = EMBED_COLOR};

            foreach (var module in _commandService.Modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"{_configuration.Prefix}{cmd.Aliases.First()} - {cmd.Summary ?? "NONE"}\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("help")]
        [Summary("명령어 상세 도움말")]
        public async Task HelpAsync(string command)
        {
            var result = _commandService.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Not exist command.");
                return;
            }

            var builder = new EmbedBuilder {Color = EMBED_COLOR};

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;
                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters:\n{string.Join('\n', cmd.Parameters.Select(p => $"- {p.Name}: {p.Summary}"))}\nSummary: {cmd.Summary}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}