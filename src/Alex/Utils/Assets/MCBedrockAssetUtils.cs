using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.InGame;
using NLog;

namespace Alex.Utils.Assets
{
    public class MCBedrockAssetUtils
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MCBedrockAssetUtils));

        private const string DownloadURL = "https://aka.ms/resourcepacktemplate";
        private static readonly string CurrentBedrockVersionStorageKey = Path.Combine("assets", "bedrock-version.txt");
        
        private static readonly Regex ExtractVersionFromFilename = new Regex(@"Vanilla_.*Pack_(?<version>[\d\.]+).zip", RegexOptions.Compiled);
        
        private IStorageSystem Storage { get; }
        public MCBedrockAssetUtils(IStorageSystem storage)
        {
            Storage = storage;
        }

        public bool TryGetStoredAssetVersion(out string version)
        {
            if (Storage.TryReadString(CurrentBedrockVersionStorageKey, out var currentVersion))
            {
                version = currentVersion;

                return true;
            }

            version = null;
            return false;
        }

        public bool CheckUpdate(IProgressReceiver progressReceiver, out string path)
        {
            path = String.Empty;
            progressReceiver?.UpdateProgress(0, "Checking for resource updates...");
            
            using var httpClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true,
            });

            try
            {
              //  string assetsZipSavePath = String.Empty;

                string currentVersion;
                if (TryGetStoredAssetVersion(out currentVersion))
                {
                    path = Path.Combine("assets", $"bedrock-{currentVersion}.zip");

                    if (!Storage.Exists(path))
                        currentVersion = null;
                }

                try
                {
                    var zipDownloadHeaders = httpClient.Send(
                        new HttpRequestMessage(HttpMethod.Get, DownloadURL), HttpCompletionOption.ResponseHeadersRead);

                    var fileName = zipDownloadHeaders.Content?.Headers?.ContentDisposition?.FileName
                                   ?? zipDownloadHeaders.RequestMessage?.RequestUri?.LocalPath;

                    var versionMatch = ExtractVersionFromFilename.Match(fileName);

                    if (versionMatch.Success)
                    {
                        var latestVersion = versionMatch.Groups["version"].Value;

                        if (latestVersion != currentVersion
                            || (!string.IsNullOrWhiteSpace(path) && !Storage.Exists(path)))
                        {
                            if (Storage.Exists(path))
                                Storage.Delete(path);
                            
                            progressReceiver?.UpdateProgress(
                                0, "Downloading latest bedrock assets...", "This could take a while...");

                            /* zipDownloadHeaders = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                                 zipDownloadHeaders.Headers.Location), HttpCompletionOption.ResponseContentRead);
                             var content = await zipDownloadHeaders.Content.ReadAsByteArrayAsync();*/
                            Stopwatch sw = new Stopwatch();
                            WebClient wc = new WebClient();

                            wc.DownloadProgressChanged += (sender, args) =>
                            {
                                var downloadSpeed =
                                    $"Download speed: {FormattingUtils.GetBytesReadable((long) (Convert.ToDouble(args.BytesReceived) / sw.Elapsed.TotalSeconds), 2)}/s";

                                progressReceiver?.UpdateProgress(
                                    args.ProgressPercentage, $"Downloading latest bedrock assets...", downloadSpeed);
                            };

                            bool   complete = false;
                            string archivePath = string.Empty;
                            wc.DownloadDataCompleted += (sender, args) =>
                            {
                                var content = args.Result;
                                
                                archivePath = Path.Combine("assets", $"bedrock-{latestVersion}.zip");

                                // save locally
                                
                                Storage.TryWriteString(CurrentBedrockVersionStorageKey, latestVersion);
                                
                                Storage.TryWriteBytes(archivePath, content);

                                complete = true;
                            };
                            
                            wc.DownloadDataAsync(zipDownloadHeaders.RequestMessage.RequestUri);
                            sw.Restart();
                            
                            SpinWait.SpinUntil(() => complete);
                            
                            path = archivePath;
                            
                            return true;
                        }
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to download bedrock assets...");

                    return false;
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Failed to fetch latest bedrock assets pack.");
                throw;
            }
        }
    }
}