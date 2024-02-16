using ClamShell.MessageBus.Models.Payloads;
using ClamShell.MessageBus.Models;
using Discord.WebSocket;

namespace ClamShell.Bot.Services.Interfaces
{
    public interface IMessageHandler
    {
        Task HandleMessageAsync(SocketMessage message);
    }
}
