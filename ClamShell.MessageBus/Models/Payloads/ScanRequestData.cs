using ClamShell.MessageBus.Models.Enums;

namespace ClamShell.MessageBus.Models.Payloads
{
    public class ScanRequestData
    {
        public ScanType ScanType { get; set; }
        public byte[] Data { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public string ChannelName { get; set; }
    }
}
