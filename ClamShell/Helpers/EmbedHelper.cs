using ClamShell.MessageBus.Models.Enums;
using Discord;

namespace ClamShell.Bot.Helpers
{
    public static class EmbedHelper
    {
        public static Embed Build(string content, string author, ScanType scanType)
        {

            string description = scanType switch
            {
                ScanType.File => "The file you uploaded has been detected as a virus and has been removed.",
                ScanType.URL => "The URL you shared has been reported as a Phishing site and has been removed.",
                _ => "The content you uploaded has been detected as a virus and has been removed."
            };

            string title = scanType switch
            {
                ScanType.File => "Virus Detected",
                ScanType.URL => "Phishing Site Detected",
                _ => "Virus Detected"
            };

            string userSummary = scanType switch
            {
                ScanType.File => "User that uploaded this file",
                ScanType.URL => "User that shared this URL",
                _ => "User that uploaded this content"
            };

            return new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithFields(new EmbedFieldBuilder[] {
                    new EmbedFieldBuilder()
                        .WithName("Original Message")
                        .WithValue(string.IsNullOrEmpty(content) ? "_No message was with attachment.._" : "\"" + content + "\"")
                        .WithIsInline(true),
                    new EmbedFieldBuilder()
                        .WithName(userSummary)
                        .WithValue(author)
                        .WithIsInline(false)
                })
                .WithColor(Color.DarkRed)
                .Build();
        }
    }
}
