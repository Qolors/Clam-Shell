using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClamShell.Bot.Helpers
{
    public static class UrlExtractor
    {
        public static string? ExtractUrl(string message)
        {
            string[] words = message.Split(' ');

            foreach (var word in words)
            {
                if (Uri.TryCreate(word, UriKind.Absolute, out var uriResult) && 
                    (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    return word;
                }
            }

            return null;
        }
    }
}
