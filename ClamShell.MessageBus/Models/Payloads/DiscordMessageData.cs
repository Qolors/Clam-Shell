using ClamShell.MessageBus.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClamShell.MessageBus.Models.Payloads
{
    public class DiscordMessageData
    {
        public string ChannelName { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public Status Status { get; set; }
        public ScanType ScanType { get; set; }
    }
}
