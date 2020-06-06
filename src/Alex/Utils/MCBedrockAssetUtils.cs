using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Alex.API.Services;
using NLog;

namespace Alex.Utils
{
    public class MCBedrockAssetUtils
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MCBedrockAssetUtils));

        private const string DownloadURL = "https://aka.ms/resourcepacktemplate";
        private static readonly string CurrentBedrockVersionStorageKey = Path.Combine("assets", "bedrock", "version.txt");
        
        private static readonly Regex ExtractVersionFromFilename = new Regex(@"Vanilla_.*Pack(?<version>[\d\.]+).zip");
        
        private IStorageSystem Storage { get; }
        public MCBedrockAssetUtils(IStorageSystem storage)
        {
            Storage = storage;
        }

        public async Task<string> CheckAndDownloadResources()
        {
            using var httpClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false,
            });

            try
            {
                string assetsZipSavePath = String.Empty;

                if (Storage.TryReadString(CurrentBedrockVersionStorageKey, out var currentVersion))
                {
                    assetsZipSavePath = Path.Combine("assets", $"bedrock-{currentVersion}.zip");
                }

                var request = new HttpRequestMessage(HttpMethod.Get, DownloadURL);
                var zipDownloadHeaders = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                var fileName = zipDownloadHeaders.Content.Headers.ContentDisposition.FileName ??
                               zipDownloadHeaders.RequestMessage.RequestUri.LocalPath;
                var versionMatch = ExtractVersionFromFilename.Match(fileName);
                if (versionMatch.Success)
                {
                    var latestVersion = versionMatch.Groups["version"].Value;

                    if (latestVersion != currentVersion ||
                        (!string.IsNullOrWhiteSpace(assetsZipSavePath) && !Storage.Exists(assetsZipSavePath)))
                    {
                        var content = await zipDownloadHeaders.Content.ReadAsByteArrayAsync();
                        // save locally
                        Storage.TryWriteString(CurrentBedrockVersionStorageKey, latestVersion);

                        Storage.TryWriteBytes(assetsZipSavePath, content);

                    }

                }
                
                return assetsZipSavePath;
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Failed to fetch latest bedrock assets pack.");
                throw;
            }
        }
    }
}