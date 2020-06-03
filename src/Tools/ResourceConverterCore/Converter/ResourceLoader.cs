using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace ResourceConverterCore.Converter
{
    public class ResourceLoader
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private readonly DirectoryInfo _source;

        private DirectoryInfo _entityDirectory, _definitionDirectory, _modelEntityDirectory;
        private Dictionary<string, EntityModel>          _processedModels = new Dictionary<string, EntityModel>();
        public  IReadOnlyDictionary<string, EntityModel> EntityModels => _processedModels;
        private FileInfo _mobsFile;

        private Dictionary<string, FileInfo> _entityGeometry = new Dictionary<string, FileInfo>();

        private Dictionary<string, EntityDefinition> _entityDefinitions = new Dictionary<string, EntityDefinition>();

        public FileInfo MobsFile
        {
            get => _mobsFile;
        }

        public IReadOnlyDictionary<string, FileInfo> EntityGeometry
        {
            get => _entityGeometry;
        }

        public ResourceLoader(DirectoryInfo source)
        {
            _source = source;

        }

        public void Load()
        {
            LoadResources();

            ProcessEntityDefinitions();
        }

        private void LoadResources()
        {
            foreach (var dir in _source.EnumerateDirectories())
            {
                if (_entityDirectory == null && dir.Name.Equals("entity"))
                {
                    LoadEntityResources(dir);
                }
                else if (_definitionDirectory == null && dir.Name.Equals("definitions"))
                {
                    LoadDefinitionResources(dir);
                }
                else if (dir.Name.Equals("models"))
                {
                    LoadModelEntityResources(dir);
                }
            }
        }

        private void LoadEntityResources(DirectoryInfo directory)
        {
            if (_entityDirectory != null)
            {
                Log.Warn("Entity directory has already been loaded.");
                return;
            }

            _entityDirectory = directory;

            foreach (var file in directory.EnumerateFiles())
            {
                if (!_entityGeometry.TryAdd(file.Name, file))
                {
                    if (_entityGeometry.TryGetValue(file.Name, out var current))
                    {
                        if (current.LastWriteTimeUtc < file.LastWriteTimeUtc)
                        {
                            _entityGeometry[file.Name] = file;
                            continue;
                        }
                    }

                    Log.Warn($"Failed to add to entity geo dictionary (0)! {file.Name}");
                }
            }

        }
        private void LoadDefinitionResources(DirectoryInfo directory)
        {
            if (_definitionDirectory != null)
            {
                Log.Warn("Entity Definition directory has already been loaded.");
                return;
            }

            _definitionDirectory = directory;

            foreach (var d in directory.EnumerateDirectories())
            {
                if (d.Name.Equals("entity"))
                {
                    _definitionDirectory = d;

                    foreach (var file in d.EnumerateFiles())
                    {
                        if (!_entityGeometry.TryAdd(file.Name, file))
                        {
                            if (_entityGeometry.TryGetValue(file.Name, out var current))
                            {
                                if (current.LastWriteTimeUtc < file.LastWriteTimeUtc)
                                {
                                    _entityGeometry[file.Name] = file;
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
        
        private void LoadModelEntityResources(DirectoryInfo directory)
        {
            if (_modelEntityDirectory == null)
            {
                foreach (var d in directory.EnumerateDirectories())
                {
                    if (d.Name.Equals("entity"))
                    {
                        _modelEntityDirectory = d;

                        foreach (var file in d.EnumerateFiles())
                        {
                            if (!_entityGeometry.TryAdd(file.Name, file))
                            {
                                if (_entityGeometry.TryGetValue(file.Name, out var current))
                                {
                                    if (current.LastWriteTimeUtc < file.LastWriteTimeUtc)
                                    {
                                        _entityGeometry[file.Name] = file;
                                        continue;
                                    }
                                }
                                Log.Warn($"Failed to add to entity geo dictionary (2)! {file.Name}");
                            }
                        }

                        break;
                    }
                }
            }

            if (_mobsFile == null)
            {
                foreach (var file in directory.EnumerateFiles())
                {
                    if (file.Name.Equals("mobs.json"))
                    {
                        _mobsFile = file;
                        break;
                    }
                }
            }
        }


        private void ProcessEntityDefinitions()
        {
            foreach (var file in _definitionDirectory.EnumerateFiles())
            {
                LoadEntityDefinition(file);
            }

            
            var res = new Dictionary<string, EntityModel>();
            GetEntries(_mobsFile, res);

            int missed1 = LoadMobs(res);

            res.Clear();

            foreach (var file in _entityGeometry.Values)
            {
                GetEntries(file, res);
            }

            int missed2 = LoadMobs(res);

            if (missed1 > 0 || missed2 > 0)
            {
                Log.Warn($"Failed to process {missed1 + missed2} entity models");
            }

            Log.Info($"Processed {EntityModels.Count} entity models!");
            Log.Info($"Processed {_entityDefinitions.Count} entity definitions");
        }

        private void LoadEntityDefinition(FileInfo definition)
        {
            using (var open = definition.OpenText())
            {
                var json = open.ReadToEnd();

                string fileName = Path.GetFileNameWithoutExtension(definition.Name);

                Dictionary<string, EntityDefinition> definitions = JsonConvert.DeserializeObject<Dictionary<string, EntityDefinition>>(json);
                foreach (var def in definitions)
                {
                    def.Value.Filename = fileName;
                    if (!_entityDefinitions.ContainsKey(def.Key))
                    {
                        _entityDefinitions.Add(def.Key, def.Value);
                        //try
                        //{
                        //    if (def.Value != null && def.Value.Textures != null)
                        //    {
                        //        foreach (var texture in def.Value.Textures)
                        //        {
                        //            string texturePath = Path.Combine(_workingDir.FullName, texture.Value + ".png");
                        //            if (File.Exists(texturePath))
                        //            {
                        //                Bitmap bmp = null;
                        //                using (FileStream fs = new FileStream(texturePath, FileMode.Open))
                        //                {
                        //                    bmp = new Bitmap(fs);
                        //                }

                        //                if (bmp != null)
                        //                    _bitmaps.TryAdd(texture.Value, bmp);
                        //            }
                        //        }
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    Log.Warn($"Could not load texture! {ex}");
                        //}
                    }
                }
            }
        }
        
        private void GetEntries(FileInfo file, Dictionary<string, EntityModel> entries)
        {
            using (var open = file.OpenText())
            {
                var     json = open.ReadToEnd();
                JObject obj  = JObject.Parse(json, new JsonLoadSettings());

                foreach (var e in obj)
                {
                    if (e.Key == "format_version") continue;
                    //if (e.Key == "minecraft:client_entity") continue;
                    //if (e.Key.Contains("zombie")) Console.WriteLine(e.Key);
                    entries.TryAdd(e.Key, e.Value.ToObject<EntityModel>(new JsonSerializer()
                    {
                        Converters = { new Vector3Converter(), new Vector2Converter() }
                    }));
                }
            }
        }
        
        private int LoadMobs(Dictionary<string, EntityModel> entries)
        {
            int c = 0;

            List<string> laterStages = new List<string>();
            Dictionary<string, EntityModel> orderedDict = new Dictionary<string, EntityModel>();
            Dictionary<string, EntityModel> failedToProcess = new Dictionary<string, EntityModel>();

            foreach (var (key, value) in entries)
            {
                if (!key.Contains(":"))
                {
                    if (!orderedDict.TryAdd(key, value))
                    {
                        Log.Warn($"Failed to add to dictionary! {key}");
                    }
                }
                else
                {
                    if (!laterStages.Contains(key))
                        laterStages.Add(key);
                }
            }

            orderedDict = OrderByLength(orderedDict);


            foreach (var late in laterStages.ToArray())
            {
                var split = late.Split(':');
                string parent = split[1];
                string kid = split[0];

                if (orderedDict.TryGetValue(parent, out EntityModel _))
                {
                    if (orderedDict.TryAdd(late, entries[late]))
                    {
                        laterStages.Remove(late);
                    }
                    else
                    {
                        Log.Warn($"Could not add to ordered dictionary!");
                    }
                }
                else
                {
               //     Log.Warn($"Unresolved entity: {late}");
                }
            }

            orderedDict = OrderByChild(orderedDict);

            foreach (var (key, value) in orderedDict)
            {
                value.Name = key;

                if (_processedModels.ContainsKey(key))
                    continue;

                ProcessEntityModel(value, entries, failedToProcess, false);
            }

            var retryCopy = new Dictionary<string, EntityModel>(failedToProcess.ToArray());

            int fix = 0;
            foreach (var e in retryCopy)
            {
                if (ProcessEntityModel(e.Value, entries, failedToProcess, true))
                {
                    fix++;
                }
            }

            c = retryCopy.Count;

            return failedToProcess.Count - fix;
        }
        private bool ProcessEntityModel(EntityModel model, Dictionary<string, EntityModel> models,
            Dictionary<string, EntityModel> failedToProcess, bool isRetry = false)
        {
               string modelName = model.Name;
            if (model.Name.Contains(":")) //This model inherits from another model.
            {
                string[] split = model.Name.Split(':');
                string parent = split[1];

                if (!_processedModels.TryGetValue(parent, out var parentModel))
                {
                    if (!isRetry)
                    {
                        failedToProcess.TryAdd(modelName, model);

                        Log.Warn($"No parent model for {modelName}");
                    }

                    return false;
                }

                modelName = split[0];

                if (model.Bones == null)
                {
                    model.Bones = new EntityModelBone[0];
                }

                if (parentModel == null)
                {
                    Log.Warn($"Pass 1 fail... {modelName}");
                    return false;
                }

                if (parentModel.Bones == null || parentModel.Bones.Length == 0)
                {
                    Log.Warn($"Parent models contains no bones! {modelName}");
                    return false;
                }

                Dictionary<string, EntityModelBone> parentBones =
                    parentModel.Bones.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name))
                        .ToDictionary(x => x.Name, e => e);

                Dictionary<string, EntityModelBone> bones =
                    model.Bones.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name))
                        .ToDictionary(x => x.Name, e => e);

                foreach (var bone in parentBones)
                {
                    var parentBone = bone.Value;
                    if (bones.TryGetValue(bone.Key, out EntityModelBone val))
                    {
                        if (!val.Reset)
                        {
                            if (val.Cubes != null)
                            {
                                val.Cubes = val.Cubes.Concat(parentBone.Cubes).ToArray();
                            }
                            else
                            {
                                val.Cubes = parentBone.Cubes;
                            }

                            //val.Cubes.Concat(parentBone.Cubes);
                        }


                        bones[bone.Key] = val;
                    }
                    else
                    {
                        bones.Add(bone.Key, parentBone);
                    }
                }

                model.Bones = bones.Values.ToArray();
            }

            return _processedModels.TryAdd(modelName, model);
        }
        private Dictionary<string, TValue> OrderByLength<TValue>(Dictionary<string, TValue> dictionary)
        {
            return dictionary.OrderBy(obj => obj.Key.Length).ToDictionary(obj => obj.Key, obj => obj.Value);
        }

        private Dictionary<string, TValue> OrderByChild<TValue>(Dictionary<string, TValue> dictionary)
        {
            return dictionary.OrderBy(obj => obj.Key.Contains(":")).ToDictionary(obj => obj.Key, obj => obj.Value);
        }
    }
}
