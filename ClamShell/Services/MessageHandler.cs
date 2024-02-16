using ClamShell.Bot.Helpers;
using ClamShell.Bot.Services.Interfaces;
using ClamShell.MessageBus;
using ClamShell.MessageBus.Models;
using ClamShell.MessageBus.Models.Enums;
using ClamShell.MessageBus.Models.Payloads;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace ClamShell.Bot.Services
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IScanService _scanService;
        private readonly Consumer<DiscordMessageData> _consumer;
        private readonly DiscordSocketClient _client;
        private readonly bool _addReactions;

        public MessageHandler(IScanService scanService, Consumer<DiscordMessageData> consumer, DiscordSocketClient client, IConfiguration configuration)
        {
            _scanService = scanService;
            _consumer = consumer;
            _client = client;
            _addReactions = bool.Parse(configuration["Settings:USE_REACTIONS"]);

            _consumer.MessageReceived += HandleClamNotificationAsync;
        }
        public async Task HandleMessageAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage message) { return; }

            if (message.Author.Id == _client.CurrentUser.Id || message.Author.IsBot) { return; }

            string? extractedUrl = UrlExtractor.ExtractUrl(message.Content);

            if (extractedUrl is not null)
            {
                byte[] urlBytes = Encoding.UTF8.GetBytes(extractedUrl);
                _scanService.ScanUrl(urlBytes, message.Channel.Id, message.Id, message.Channel.Name);
            }

            if (message.Attachments.Count == 0)
            {
                return;
            }

            foreach (IAttachment attachment in message.Attachments)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        byte[] attachmentContent = await httpClient.GetByteArrayAsync(attachment.Url);

                        _scanService.ScanFile(attachmentContent, message.Channel.Id, message.Id, message.Channel.Name);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Error downloading file");
                }

            }
        }

        private async void HandleClamNotificationAsync(object? sender, TransferModel<DiscordMessageData> e)
        {
            if (e is null) { Console.WriteLine("Received null message"); return; }

            var channel = _client.GetChannel(e.Data.ChannelId) as IMessageChannel;

            IMessage? msg = await channel.GetMessageAsync(e.Data.MessageId);

            SocketUserMessage? message = msg as SocketUserMessage;

            if (e.Data.Status == Status.Clean && _addReactions)
            {
                await msg.AddReactionAsync(new Emoji("✅"));
            }
            else if (e.Data.Status == Status.Infected)
            {
                try
                {
                    await message.ReplyAsync(embed: EmbedHelper.Build(msg.Content, msg.Author.GlobalName, e.Data.ScanType));

                    await msg.DeleteAsync();
                }
                catch (Discord.Net.HttpException)
                {
                    Console.WriteLine("Message not found - Assuming Deletion Previously");
                }
                catch (Exception)
                {
                    Console.WriteLine("Error replying to message");
                }

            }
            else if (e.Data.Status == Status.Error && _addReactions)
            {
                await msg.AddReactionAsync(new Emoji("❓"));
            }
        }
    }
}
