using ClamShell.Bot.Services;
using ClamShell.Bot.Services.Interfaces;
using ClamShell.MessageBus;
using ClamShell.MessageBus.Models.Payloads;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace ClamShell.Bot
{
    public class Program
    {
        private static IServiceProvider _services;
        public static async Task Main(string[] args)
        {
            _services = ConfigureServices();

            var client = _services.GetRequiredService<DiscordSocketClient>();
            var messageHandler = _services.GetRequiredService<IMessageHandler>();

            client.Log += LogAsync;
            client.Ready += OnReadyAsync;
            client.MessageReceived += messageHandler.HandleMessageAsync;

            await client.LoginAsync(TokenType.Bot, _services.GetRequiredService<IConfiguration>()["Settings:BOT_TOKEN"]);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,
                    MessageCacheSize = 50,
                    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
                }))
                .AddSingleton<IScanService, ScanService>()
                .AddSingleton<IMessageHandler, MessageHandler>()
                .AddSingleton(new Consumer<DiscordMessageData>("scan_result"))
                .AddSingleton<IConfiguration>(GetConfiguration())
                .BuildServiceProvider();
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private static Task OnReadyAsync()
        {
            Console.WriteLine("Clam Shell Running..");

            return Task.CompletedTask;
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}
