﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Services;
using Alex.Common.Utils;
using NLog;

namespace Alex.Utils.Assets
{
    public class MCJavaAssetsUtil
    {
        private const string VersionManifestUri =
            "https://launchermeta.mojang.com/mc/game/version_manifest.json?_t={ts}";

        private const string AssetResourceUri = "https://resources.download.minecraft.net/{hash_sub2}/{hash}";

        private static readonly string CurrentVersionStorageKey = Path.Combine("assets", "version.txt");

        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MCJavaAssetsUtil));

        private readonly IStorageSystem _storage;

        private VersionManifest _manifest;

        public MCJavaAssetsUtil(IStorageSystem storage)
        {
            _storage = storage;
        }


        private async Task<VersionManifest> GetManifestAsync()
        {
            if (_manifest != null)
                return _manifest;

            using var httpClient = new HttpClient();

            try
            {
                var versionManifestJson = await httpClient.GetStringAsync(
                    VersionManifestUri.Replace("{ts}", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));

                var versionManifest = VersionManifest.FromJson(versionManifestJson);
                _manifest = versionManifest;

                return _manifest;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch latest MCJava version manifest.");
                throw;
            }
        }

        private string GetMinecraftPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "versions");
            }
            
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".minecraft", "versions");
        }
        
        private bool CheckLocal(string targetRelease, out string path)
        {
            path = null;

            var mcPath = GetMinecraftPath();
            mcPath = Path.Combine(mcPath, targetRelease);

            if (Directory.Exists(mcPath))
            {
                mcPath = Path.Combine(mcPath, $"{targetRelease}.jar");

                if (File.Exists(mcPath))
                {
                    path = mcPath;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetStoredVersion(out string storedVersion)
        {
            if (_storage.TryReadString(CurrentVersionStorageKey, out var currentVersion))
            {
                storedVersion = currentVersion;
                return true;
            }

            storedVersion = null;
            return false;
        }
        
        public async Task<string> EnsureTargetReleaseAsync(string targetRelease, IProgressReceiver progressReceiver, string assetDirectory)
        {
            var targetVersion = targetRelease; //manifest.Latest.Release;

       //     string assetDirectory    = Path.Combine("assets", "java");
            string assetsZipSavePath = Path.Combine("assets", $"java-{targetVersion}.zip");

            /*if (TryGetStoredVersion(out string currentVersion))
            {
                if (currentVersion == targetVersion)
                {
                    if (_storage.Exists(assetsZipSavePath))
                    {
                        Log.Debug("MCJava Assets Up to date!");
                        return assetsZipSavePath;
                    }
                }
            }*/

            /*if (CheckLocal(targetRelease, out var jarPath))
            {
                _storage.TryWriteString(CurrentVersionStorageKey, targetRelease);
                return jarPath;
                // using(FileStream)
            }
            else*/
            {
                progressReceiver?.UpdateProgress(0, "Downloading assets...");

                var manifest = await GetManifestAsync();

                // not latest, update
                Log.Info($"Downloading MCJava {targetVersion} Assets.");

                var version = manifest.Versions.FirstOrDefault(
                    v => string.Equals(v.Id, targetVersion, StringComparison.InvariantCultureIgnoreCase));

                if (version == null)
                {
                    Log.Error("Version not found in versions? wut?");

                    return assetsZipSavePath;
                }

                LauncherMeta launcherMeta;
                AssetIndex   assetIndex;

                //var dirpath = Path.Combine("assets", $"java-{targetVersion}_cache");

                if (!_storage.TryGetDirectory(assetDirectory, out var dir))
                {
                    if (_storage.TryCreateDirectory(assetDirectory))
                    {
                        if (!_storage.TryGetDirectory(assetDirectory, out dir))
                            return assetsZipSavePath;
                    }
                }

                // fetch version's json thing
                using (var httpClient = new HttpClient())
                {
                    var launcherMetaJson = await httpClient.GetStringAsync(version.Url);
                    launcherMeta = LauncherMeta.FromJson(launcherMetaJson);

                    // download client, prob usefil?
                    //var clientJar = await httpClient.GetByteArrayAsync(launcherMeta.Downloads.Client.Url);


                   // using (var clientMs = new MemoryStream(clientJar))
                   
                   using (var clientMs = await httpClient.GetStreamAsync(launcherMeta.Downloads.Client.Url))
                   {
                       using (ZipArchive clientJarZip = new ZipArchive(clientMs, ZipArchiveMode.Read))
                       {
                           foreach (var entry in clientJarZip.Entries)
                           {
                               if (!entry.FullName.StartsWith("assets") && entry.FullName != "pack.mcmeta") continue;

                               var localpath = Path.Combine(dir.FullName, entry.FullName);

                               if (!_storage.TryGetDirectory(Path.GetDirectoryName(localpath), out _))
                               {
                                   _storage.TryCreateDirectory(Path.GetDirectoryName(localpath));
                                   // Directory.CreateDirectory(Path.GetDirectoryName(localpath));
                               }

                               entry.ExtractToFile(localpath, true);

                               Log.Debug($"Extracted Asset '{entry.Name}' (Size: {entry.Length})");
                           }
                       }
                   }

                   // now we only care about asset index soooo... grab that
                    var assetIndexJson = await httpClient.GetStringAsync(launcherMeta.AssetIndex.Url);
                    _storage.TryWriteString("assetIndex", assetIndexJson, Encoding.UTF8);
                    
                    assetIndex = AssetIndex.FromJson(assetIndexJson);

                    int target = assetIndex.Objects.Count;

                    int done = 0;

                    foreach (var assetIndexObject in assetIndex.Objects)
                    {
                        // Skip ogg files
                        if (assetIndexObject.Key.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        var assetUrl = AssetResourceUri
                           .Replace("{hash_sub2}", assetIndexObject.Value.Hash.Substring(0, 2)).Replace(
                                "{hash}", assetIndexObject.Value.Hash);

                        progressReceiver?.UpdateProgress(done, target, "Downloading assets...", assetIndexObject.Key);

                        try
                        {
                            var fileBytes = await httpClient.GetByteArrayAsync(assetUrl);

                            var filename = Path.Combine("assets", assetIndexObject.Key);

                            var localpath = Path.Combine(dir.FullName, filename);

                            if (!_storage.TryGetDirectory(Path.GetDirectoryName(localpath), out _))
                            {
                                _storage.TryCreateDirectory(Path.GetDirectoryName(localpath));
                                // Directory.CreateDirectory(Path.GetDirectoryName(localpath));
                            }

                            //  File.WriteAllBytes(localpath, fileBytes);

                            _storage.TryWriteBytes(localpath, fileBytes);

                            Log.Debug(
                                $"Downloaded asset '{assetIndexObject.Key}' (Hash: {assetIndexObject.Value.Hash})");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(
                                ex,
                                $"Failed to download asset '{assetIndexObject.Key}' (Hash: {assetIndexObject.Value.Hash})");

                            continue;
                        }

                        done++;
                    }
                }

                //  make new zip m8
                /*using (var ms = new MemoryStream())
                {
                    using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in dir.EnumerateFiles("*", SearchOption.AllDirectories))
                        {
                            // Zip file entries MUST use '/' as directory separator 
                            zip.CreateEntryFromFile(file.FullName, Path.GetRelativePath(dir.FullName, file.FullName).Replace(Path.DirectorySeparatorChar, '/'));
                        }
                    }

                    // saving zip m8
                    ms.Seek(0, SeekOrigin.Begin);
                    var allBytes = ms.ToArray();

                    _storage.TryWriteBytes(assetsZipSavePath, allBytes);
                    Log.Info($"Written Archive to '{assetsZipSavePath}' (Size: {allBytes.Length})");
                }*/

               // Thread.Sleep(500);
                //dir.Delete(true);
            }
            
            _storage.TryWriteString(CurrentVersionStorageKey, targetVersion);
            
            return assetsZipSavePath;
        }
    }
}