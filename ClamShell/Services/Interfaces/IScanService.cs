namespace ClamShell.Bot.Services.Interfaces
{
    public interface IScanService
    {
        void ScanUrl(byte[] url, ulong channelId, ulong messageId, string channelName);
        void ScanFile(byte[] file, ulong channelId, ulong messageId, string channelName);
    }
}
