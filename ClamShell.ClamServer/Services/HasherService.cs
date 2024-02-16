using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Timer = System.Timers.Timer;

namespace ClamShell.ClamServer.Services
{
    public class HasherService
    {
        private const string Url = "https://raw.githubusercontent.com/mitchellkrogza/Phishing.Database/master/ALL-phishing-domains.tar.gz";
        private const double Interval = 12 * 60 * 60 * 1000;
        private HashSet<string> urlHashes = new HashSet<string>();
        private string currentHash = string.Empty;
        private Timer _timer;
        private ILogger<HasherService> _logger;

        public HasherService(ILogger<HasherService> logger)
        {
            _timer = new Timer(Interval);
            _timer.Elapsed += async (sender, e) => await UpdatePhishingDomains();
            _timer.AutoReset = true;
            _logger = logger;
        }

        public void Start()
        {
            _timer.Start();
            Task.Run(UpdatePhishingDomains);
        }

        private async Task UpdatePhishingDomains()
        {
            _logger.LogInformation("Updating phishing domains...");

            HashSet<string> newHashes = new HashSet<string>();

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(Url);
                    response.EnsureSuccessStatusCode();

                    // Read the response content into a memory stream
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var memoryStream = new MemoryStream())
                    {
                        await responseStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        // Compute the hash of the decompressed stream
                        string streamHash;
                        using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress, leaveOpen: true))
                        using (var sha256 = SHA256.Create())
                        {
                            var hashBytes = sha256.ComputeHash(gzipStream);
                            streamHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                        }

                        if (streamHash == currentHash)
                        {
                            _logger.LogInformation("Phishing domains are up to date.");
                            return;
                        }

                        currentHash = streamHash;

                        // Read the content again for processing
                        memoryStream.Position = 0;
                        using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                        using (var reader = new StreamReader(gzipStream))
                        {
                            string line;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {
                                newHashes.Add(ComputeHash(line));
                            }

                            urlHashes = newHashes;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update phishing domains: {ex.Message}", ex.Message);
            }

        }

        private string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        public bool IsPhishing(string url)
        {
            Uri uri = new Uri(url);

            string host = uri.Host;

            if (string.IsNullOrEmpty(host))
            {
                _logger.LogError("Failed to parse host from URL: {url}.. Unable to determine URL Safety", url);
                return false;
            }

            string hash = ComputeHash(host);

            return urlHashes.Contains(hash);
        }
    }
}
