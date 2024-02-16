using ClamShell.Bot.Services.Interfaces;
using ClamShell.MessageBus;
using ClamShell.MessageBus.Models;
using ClamShell.MessageBus.Models.Enums;
using ClamShell.MessageBus.Models.Payloads;
using System.Text;
using System;

namespace ClamShell.Bot.Services
{
    public class ScanService : IScanService
    {
        public void ScanFile(byte[] fileData, ulong channelId, ulong messageId, string channelName)
        {
            using var publisher = new Publisher<ScanRequestData>("scan_request");

            TransferModel<ScanRequestData> transferModel = new TransferModel<ScanRequestData>
            {
                Data = new ScanRequestData
                {
                    Data = fileData,
                    ScanType = ScanType.File,
                    ChannelId = channelId,
                    MessageId = messageId,
                    ChannelName = channelName
                }
            };

            publisher.Publish(transferModel);
        }

        public void ScanUrl(byte[] requestUrl, ulong channelId, ulong messageId, string channelName)
        {
            using var publisher = new Publisher<ScanRequestData>("scan_request");

            TransferModel<ScanRequestData> transferModel = new TransferModel<ScanRequestData>
            {
                Data = new ScanRequestData
                {
                    Data = requestUrl,
                    ScanType = ScanType.URL,
                    ChannelId = channelId,
                    MessageId = messageId,
                    ChannelName = channelName
                }
            };

            publisher.Publish(transferModel);
        }
    }
}
