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

            var repoRoot = Environment.CurrentDirectory;

            var csFilePath = Path.Combine(repoRoot, "MusicBeeInterface.cs");
            var vbFilePath = Path.Combine(repoRoot, "MusicBeeInterface.vb");

            using var httpClient = new HttpClient();
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

            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
            var hashHex = Convert.ToHexString(hashBytes).ToLowerInvariant();

            Console.WriteLine($"HASH={hashHex}");
        }
    }
}
