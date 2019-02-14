using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GGM.Bot.Configuration;
using Microsoft.Extensions.Logging;

namespace GGM.Bot
{
    public class Application
    {
        private readonly ILogger _logger;
        private readonly BotConfiguration _configuration;
        private readonly TaskCompletionSource<bool> _taskCompletionSource;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _provider;
        private bool _isRunning;

        public Application(
            ILogger<Application> logger,
            BotConfiguration configuration,
            CommandService commandService,
            IServiceProvider provider
        )
        {
            _logger = logger;
            _configuration = configuration;
            _taskCompletionSource = new TaskCompletionSource<bool>();
            var discordSocketConfig = _configuration.ToDiscordSocketConfig();
            _client = new DiscordSocketClient(discordSocketConfig);
            _client.Log += Log;
            _client.MessageReceived += HandleMessage;
            _commandService = commandService;
            _provider = provider;
        }

        public async Task Start()
        {
            if (_isRunning)
                return;

            await _commandService.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _provider
            );
            await _client.LoginAsync(TokenType.Bot, _configuration.Token);
            await _client.StartAsync();
            _isRunning = true;
        }

        public async Task Stop()
        {
            await _client.StopAsync();
            await _client.LogoutAsync();
            _isRunning = false;
        }

        public void Terminate() => _taskCompletionSource.SetResult(true);

        public Task WhenTerminated() => _taskCompletionSource.Task;

        private Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(message.ToString());
                    break;
                case LogSeverity.Error:
                    _logger.LogError(message.ToString());
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(message.ToString());
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(message.ToString());
                    break;
                case LogSeverity.Verbose:
                    _logger.LogInformation(message.ToString());
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(message.ToString());
                    break;
                default:
                    _logger.LogInformation(message.ToString());
                    break;
            }

            return Task.CompletedTask;
        }

        private async Task HandleMessage(SocketMessage message)
        {
            if (!(message is SocketUserMessage userMessage))
                return;

            int argumentPosition = 0;
            if (!userMessage.HasStringPrefix(_configuration.Prefix, ref argumentPosition))
                return;

            var context = new SocketCommandContext(_client, userMessage);
            var result = await _commandService.ExecuteAsync(
                context: context,
                argPos: argumentPosition,
                services: _provider
            );

#if DEBUG
            if (!result.IsSuccess)
            {
                _logger.LogDebug(result.ErrorReason);
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
#endif
        }
    }
}