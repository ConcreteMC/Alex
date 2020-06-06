using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Alex.API.Services;

namespace Alex.Utils
{
   /* public class MCBedrockAssetUtils
    {
        private const string DownloadURL = "https://aka.ms/resourcepacktemplate";
        private static readonly string CurrentBedrockVersionStorageKey = Path.Combine("assets", "bedrock", "version.txt");
        
        private static readonly Regex ExtractVersionFromFilename = new Regex(@"Vanilla_.*Pack(?<version>[\d\.]+).zip");
        
        private IStorageSystem Storage { get; }
        public MCBedrockAssetUtils(IStorageSystem storage)
        {
            Storage = storage;
        }

        public async Task<ZipArchive> CheckAndDownloadResources()
        {
            using var httpClient = new HttpClient();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, DownloadURL);
                var zipDownloadHeaders = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                var fileName = zipDownloadHeaders.Content.Headers.ContentDisposition.FileName;
                var versionMatch = ExtractVersionFromFilename.Match(fileName);
                if (versionMatch.Success)
                {
                    var version = versionMatch.Groups["version"].Value;

                    if (version != currentVersion)
                    {
                        var content = await zipDownloadHeaders.Content.ReadAsByteArrayAsync();
                        // save locally
                        Storage.tryw(CurrentBedrockVersionStorageKey, version);

                        // return
                    }
                    
                }
            }
        }
    }*/
}