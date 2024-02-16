using ClamShell.MessageBus;
using ClamShell.MessageBus.Models;
using ClamShell.MessageBus.Models.Payloads;
using ClamShell.MessageBus.Models.Enums;
using nClam;
using System.Text.Json;
using System.Text;
using ClamShell.ClamServer.Services;
using Microsoft.Extensions.Options;

namespace ClamShell.ClamServer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Consumer<ScanRequestData> _consumer;
        private readonly HasherService _hasherService;
        private readonly ClamClient _clam;
        private readonly Publisher<DiscordMessageData> _publisher;
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;
        private readonly bool _useLogs;

        public Worker(ILogger<Worker> logger, Consumer<ScanRequestData> consumer, ClamClient clam, HasherService hasherService, IConfiguration options)
        {
            _logger = logger;
            _consumer = consumer;
            _hasherService = hasherService;
            _clam = clam;
            _publisher = new Publisher<DiscordMessageData>("scan_result");
            _httpClient = new HttpClient();
            _webhookUrl = options["Settings:WEBHOOK_URL"];
            _useLogs = bool.Parse(options["Settings:USE_LOGS"]);

            _hasherService.Start();
            _consumer.MessageReceived += Consumer_MessageReceived;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Clam Server Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken);
            }
        }

        private async void Consumer_MessageReceived(object sender, TransferModel<ScanRequestData> e)
        {
            _logger.LogInformation("Received message from Message Queue at: {time}", DateTimeOffset.Now);

            Status status = Status.Error;
            bool isUrl = e.Data.ScanType == ScanType.URL;

            if (e.Data.ScanType == ScanType.URL)
            {
                status = _hasherService.IsPhishing(Encoding.UTF8.GetString(e.Data.Data)) ? 
                    Status.Infected : 
                    Status.Clean;
            }
            else if (e.Data.ScanType == ScanType.File)
            {
                var result = await _clam.SendAndScanFileAsync(e.Data.Data);

                if (result is null)
                {
                    _logger.LogError("Error scanning file..");
                    return;
                }

                status = result.Result switch
                {
                    ClamScanResults.Clean => Status.Clean,
                    ClamScanResults.VirusDetected => Status.Infected,
                    _ => Status.Error
                };
            }

            _logger.LogInformation("Sending processed request to Message Queue");

            Task.Run(() => _publisher.Publish(new TransferModel<DiscordMessageData>
            {
                Data = new DiscordMessageData
                {
                    ChannelId = e.Data.ChannelId,
                    MessageId = e.Data.MessageId,
                    Status = status,
                    ChannelName = e.Data.ChannelName,
                    ScanType = e.Data.ScanType
                }
            }));

            if (_useLogs)
                Task.Run(() => PublishToLogs(status, e.Data.ChannelName, isUrl));
        }

        private async Task PublishToLogs(Status status, string channelName, bool isUrl)
        {
            string processType = isUrl ? "URL" : "File";
            object data = status switch
            {
                Status.Clean => new { content = $"**{DateTime.UtcNow}**: Processed a `{processType}` in `{channelName}` with result `CLEAN`" },
                Status.Infected => new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = "Virus Detected",
                            description = $"Clam Shell has detected a `{processType}` with malicious intent resulted `INFECTED`. It was deleted in `{channelName}` at {DateTime.UtcNow}",
                            color = 16711680
                        }
                    }
                },
                _ => new { content = $"**{DateTime.UtcNow}**: ClamShell has encountered an error processing a `{processType}` in `{channelName}`" }
            };

            var payloadJson = JsonSerializer.Serialize(data);
            var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            await _httpClient.PostAsync(_webhookUrl, content);
        }
    }
}
