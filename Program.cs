using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordBotCSharp
{
    public class DiscordBot
    {

        private readonly DiscordSocketClient _client;
        static Task Main(string[] args)
        {
            return new DiscordBot().MainAsync(); /*.GetAwaiter().GetResult(); */
        }


        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        private DiscordBot()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);

            _client.Ready += ReadyAsync;
            //_client.MessageReceived += MessageReceivedAsync;
            //_client.InteractionCreated += InteractionCreatedAsync;

            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,

                CaseSensitiveCommands = false,
            });

            _client.Log += Log;
            _client.Log += LogAsync;
            //_services = ConfigureServices();
        }

        public async Task MainAsync()
        {
            var token = "MTE3NDc5ODgzNDU5ODE2NjU1OQ.GVb_yf.v832NSKLRfauUDHRBCpJXf4OOY_e07CBM-BkBQ";
            await InitCommands();

            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private async Task InitCommands()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

            int pos = 0;

            if (msg.HasCharPrefix('!', ref pos))
            {
                var context = new SocketCommandContext(_client, msg);

                var result = await _commands.ExecuteAsync(context, pos, _services);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");
            return Task.CompletedTask;
        }
        
    }
}