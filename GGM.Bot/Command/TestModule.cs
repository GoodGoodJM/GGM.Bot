using System.Threading.Tasks;
using Discord.Commands;

namespace GGM.Bot.Command
{
    [Group("test")]
    public class TestModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public Task PingPong() => ReplyAsync("pong");

        [Command("echo")]
        public Task Echo([Remainder] [Summary("The message to echo")] string message) => ReplyAsync(message);

    }
}