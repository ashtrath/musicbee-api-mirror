using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace ApiChecker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var csUrl = "https://getmusicbee.com/download/plugins/MusicBeeInterface.cs";
            var vbUrl = "https://getmusicbee.com/download/plugins/MusicBeeInterface.vb";

            var repoRoot = args.Length > 0 ? args[0] : Environment.CurrentDirectory;

            var csFilePath = Path.Combine(repoRoot, "MusicBeeInterface.cs");
            var vbFilePath = Path.Combine(repoRoot, "MusicBeeInterface.vb");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            var csContent = await httpClient.GetStringAsync(csUrl);
            var vbContent = await httpClient.GetStringAsync(vbUrl);

            await File.WriteAllTextAsync(csFilePath, csContent);
            await File.WriteAllTextAsync(vbFilePath, vbContent);

            var csTree = CSharpSyntaxTree.ParseText(csContent);
            var vbTree = VisualBasicSyntaxTree.ParseText(vbContent);

            var csTokens = csTree.GetRoot().DescendantTokens().Select(t => t.Text);
            var vbTokens = vbTree.GetRoot().DescendantTokens().Select(t => t.Text);

            var csNormalized = string.Join("", csTokens);
            var vbNormalized = string.Join("", vbTokens);

            var combined = csNormalized + vbNormalized;

            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
            var hashHex = Convert.ToHexString(hashBytes).ToLowerInvariant();

            Console.WriteLine($"HASH={hashHex}");
        }
    }
}
