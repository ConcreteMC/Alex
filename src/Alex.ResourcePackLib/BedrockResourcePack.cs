using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace Alex.ResourcePackLib
{
	public class BedrockResourcePack : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockResourcePack));

		private ConcurrentDictionary<string, Bitmap> _bitmaps = new ConcurrentDictionary<string, Bitmap>();
        public IReadOnlyDictionary<string, Bitmap> Textures => _bitmaps;
		public IReadOnlyDictionary<string, EntityDefinition> EntityDefinitions { get; private set; } = new ConcurrentDictionary<string, EntityDefinition>();

		private readonly DirectoryInfo _workingDir;

		public BedrockResourcePack(DirectoryInfo directory)
		{
			_workingDir = directory;

			Load();
		}

		public bool TryGetTexture(string name, out Bitmap texture)
		{
			return Textures.TryGetValue(NormalisePath(name), out texture);
		}

		private string NormalisePath(string path)
		{
			return path.Replace('\\', '/').ToLowerInvariant();
		}

		private void Load()
		{
            DirectoryInfo entityDefinitionsDir = null;

            Dictionary<string, FileInfo> entityGeometry = new Dictionary<string, FileInfo>();
            foreach (var dir in _workingDir.EnumerateDirectories())
            {
                if (entityDefinitionsDir == null && dir.Name.Equals("definitions"))
                {
                    foreach (var d in dir.EnumerateDirectories())
                    {
                        if (d.Name.Equals("entity"))
                        {
                            entityDefinitionsDir = d;

                            foreach (var file in d.EnumerateFiles())
                            {
                                if (!entityGeometry.TryAdd(file.Name, file))
                                {
	                                if (entityGeometry.TryGetValue(file.Name, out var current))
	                                {
		                                if (current.LastWriteTimeUtc < file.LastWriteTimeUtc)
		                                {
			                                entityGeometry[file.Name] = file;
			                                continue;
		                                }
	                                }
                                    Log.Warn($"Failed to add to entity geo dictionary (1)! {file.Name}");
                                }
                            }

                            break;
                        }
                    }
                }

                if (entityDefinitionsDir != null)
                    break;
            }


            if (entityDefinitionsDir == null || !entityDefinitionsDir.Exists)
            {
                Log.Warn("Could not find entity definitions folder!");
                return;
            }

            Dictionary<string, EntityDefinition> entityDefinitions = new Dictionary<string, EntityDefinition>();
            foreach (var def in entityDefinitionsDir.EnumerateFiles())
            {
                LoadEntityDefinition(def, entityDefinitions);
            }

            EntityDefinitions = entityDefinitions;
            Log.Info($"Processed {EntityDefinitions.Count} entity definitions");
        }

		private void LoadEntityDefinition(FileInfo entry, Dictionary<string, EntityDefinition> entityDefinitions)
		{
			using (var open = entry.OpenText())
			{
				var json = open.ReadToEnd();

				string fileName = Path.GetFileNameWithoutExtension(entry.Name);

				Dictionary<string, EntityDefinition> definitions = JsonConvert.DeserializeObject<Dictionary<string, EntityDefinition>>(json);
				foreach (var def in definitions)
				{
					def.Value.Filename = fileName;
					if (!entityDefinitions.ContainsKey(def.Key))
					{
						entityDefinitions.Add(def.Key, def.Value);
						try
						{
							if (def.Value != null && def.Value.Textures != null)
							{
								foreach (var texture in def.Value.Textures)
								{
									string texturePath = Path.Combine(_workingDir.FullName, texture.Value + ".png");
									if (File.Exists(texturePath))
									{
										Bitmap bmp = null;
										using (FileStream fs = new FileStream(texturePath, FileMode.Open))
										{
											bmp = new Bitmap(fs);
										}

										if (bmp != null)
											_bitmaps.TryAdd(texture.Value, bmp);
									}
								}
							}
						}
						catch (Exception ex)
						{
							Log.Warn($"Could not load texture! {ex}");
						}
					}
				}
			}
		}

		public class EntityDefinition
		{
			[JsonIgnore] public string Filename { get; set; } = string.Empty;

			public Dictionary<string, string> Textures;
			public Dictionary<string, string> Geometry;
		}

		public void Dispose()
		{
			//_archive?.Dispose();
		}
	}
}