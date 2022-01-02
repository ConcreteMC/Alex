using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Alex.Common.Resources;
using Alex.Common.Utils;
using Alex.MoLang.Parser.Exceptions;
using Alex.ResourcePackLib.Abstraction;
using Alex.ResourcePackLib.Generic;
using Alex.ResourcePackLib.IO.Abstract;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Bedrock;
using Alex.ResourcePackLib.Json.Bedrock.Attachables;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.Particles;
using Alex.ResourcePackLib.Json.Bedrock.Sound;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Converters.Bedrock;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.ResourcePackLib.Json.Textures;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using Animation = Alex.ResourcePackLib.Json.Bedrock.Entity.Animation;
using Image = SixLabors.ImageSharp.Image;

namespace Alex.ResourcePackLib
{
    public class MCBedrockResourcePack : ResourcePack, ITextureProvider, IAnimationProvider, IRenderControllerProvider, IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MCBedrockResourcePack));

        private ConcurrentDictionary<ResourceLocation, Func<Image<Rgba32>>> _bitmaps = new ConcurrentDictionary<ResourceLocation, Func<Image<Rgba32>>>();
        public IReadOnlyDictionary<ResourceLocation, Func<Image<Rgba32>>> Textures => _bitmaps;
        public IReadOnlyDictionary<ResourceLocation, EntityDescription> EntityDefinitions { get; private set; } = new ConcurrentDictionary<ResourceLocation, EntityDescription>();
        public IReadOnlyDictionary<string, AttachableDefinition> Attachables { get; private set; } = new ConcurrentDictionary<string, AttachableDefinition>();
        public IReadOnlyDictionary<string, RenderController> RenderControllers { get; private set; } = new ConcurrentDictionary<string, RenderController>();
        public IReadOnlyDictionary<string, AnimationController> AnimationControllers { get; private set; } = new ConcurrentDictionary<string, AnimationController>();
        public IReadOnlyDictionary<string, Animation> Animations { get; private set; } = new ConcurrentDictionary<string, Animation>();

        public IReadOnlyDictionary<string, ParticleDefinition> Particles { get; private set; } =
            new ConcurrentDictionary<string, ParticleDefinition>();

        public IReadOnlyDictionary<string, JToken> GlobalUiVariables { get; private set; } =
            new Dictionary<string, JToken>();

        public SoundDefinitionFormat SoundDefinitions { get; private set; } = null;

        public SoundBindingsCollection SoundBindings { get; private set; } = null;

        private readonly IFilesystem _archive;

        public string ContentKey { get; set; } = null;

        public MCBedrockResourcePack(IFilesystem archive, ResourcePackManifest manifest, ResourcePack.LoadProgress progressReporter = null)
        {
            Info = manifest;
            _archive = archive;

            //Info = GetManifest(archive);
            Load(progressReporter);
        }

        private string NormalisePath(string path)
        {
            return path.Replace('\\', '/').ToLowerInvariant();
        }

        private const RegexOptions RegexOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
        private static readonly Regex IsEntityDefinition = new Regex(@"^entity[\\\/](?'filename'.*)\.json$", RegexOpts);
        private static readonly Regex IsEntityModel = new Regex(@"^models[\\\/]entity[\\\/](?'filename'.*)\.json$", RegexOpts);
        private static readonly Regex IsRenderController = new Regex(@"^render_controllers[\\\/](?'filename'.*)\.json$", RegexOpts);
        private static readonly Regex IsAnimationController = new Regex(@"^animation_controllers[\\\/](?'filename'.*)\.json$", RegexOpts);
        private static readonly Regex IsAnimation = new Regex(@"^animations[\\\/](?'filename'.*)\.json$", RegexOpts);
        private static readonly Regex IsSoundDefinition = new Regex(@"^sounds[\\\/]sound_definitions\.json$", RegexOpts);
        private static readonly Regex IsSoundBindings = new Regex(@"^sounds\.json$", RegexOpts);
        private static readonly Regex IsFontFile = new Regex(@"^font[\\\/](?'filename'.*)\.png$", RegexOpts);
        private static readonly Regex IsParticleFile = new Regex(@"^particles[\\\/](?'filename'.*)\.json$", RegexOpts);
        private static readonly Regex IsAttachableFile = new Regex(@"^attachables[\\\/](?'filename'.*)\.json$", RegexOpts);

        private static readonly Regex IsUiTexture = new Regex(@"^textures[\\\/]ui[\\\/](?'filename'.*)\.png", RegexOpts);
        private static readonly Regex IsUiDefinition = new Regex(@"^ui[\\\/](?'filename'.*)\.json", RegexOpts);

        private void Load(ResourcePack.LoadProgress progressReporter)
        {
            Dictionary<string, EntityDescription> entityDefinitions = new Dictionary<string, EntityDescription>();
            Dictionary<string, EntityModel> entityModels = new Dictionary<string, EntityModel>(StringComparer.Ordinal);
            Dictionary<string, RenderController> renderControllers = new Dictionary<string, RenderController>(StringComparer.Ordinal);

            Dictionary<string, AnimationController>
                animationControllers = new Dictionary<string, AnimationController>(StringComparer.Ordinal);

            Dictionary<string, ParticleDefinition> particleDefinitions = new Dictionary<string, ParticleDefinition>(StringComparer.Ordinal);

            Dictionary<string, AttachableDefinition> attachableDefinitions =
                new Dictionary<string, AttachableDefinition>(StringComparer.Ordinal);
            Dictionary<string, Animation>
                animations = new Dictionary<string, Animation>(StringComparer.Ordinal);

            TryAddBitmap("textures/ui/title");

            if (TryLoadMobModels(entityModels))
            {
                //Log.Info($"Loaded mobs.json: {entityModels.Count}");
            }

            var entries = _archive.Entries.ToArray();
            var total = entries.Length;
            int count = 0;

            foreach (var entry in entries)
            {
                try
                {
                    count++;
                    progressReporter?.Invoke((int)(((double)count / (double)total) * 100D), entry.FullName);

                    if (entry.Name.Equals("_global_variables.json"))
                    {
                        try
                        {
                            using (var stream = entry.OpenEncoded(ContentKey))
                            {
                                var json = Encoding.UTF8.GetString(stream.ReadToSpan(entry.Length));

                                Dictionary<string, JToken> globalVariables =
                                    MCJsonConvert.DeserializeObject<Dictionary<string, JToken>>(json);

                                GlobalUiVariables = new ReadOnlyDictionary<string, JToken>(globalVariables);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warn(ex, $"Could not load global variables.");
                        }

                        continue;
                    }

                    if (IsSoundBindings.IsMatch(entry.FullName))
                    {
                        ProcessSoundBindings(entry);
                        continue;
                    }

                    if (IsUiTexture.IsMatch(entry.FullName))
                    {
                        ProcessTexture(entry);
                        continue;
                    }

                    if (IsEntityDefinition.IsMatch(entry.FullName))
                    {
                        LoadEntityDefinition(entry, entityDefinitions);

                        continue;
                    }

                    if (IsEntityModel.IsMatch(entry.FullName))
                    {
                        LoadEntityModel(entry, entityModels);

                        continue;
                    }

                    if (IsSoundDefinition.IsMatch(entry.FullName))
                    {
                        ProcessSounds(progressReporter, entry);

                        continue;
                    }

                    if (IsFontFile.IsMatch(entry.FullName))
                    {
                        ProcessFontFile(progressReporter, entry);

                        continue;
                    }

                    if (IsRenderController.IsMatch(entry.FullName))
                    {
                        ProcessRenderController(entry, renderControllers);

                        continue;
                    }

                    if (IsAnimationController.IsMatch(entry.FullName))
                    {
                        ProcessAnimationController(entry, animationControllers);

                        continue;
                    }

                    if (IsAnimation.IsMatch(entry.FullName))
                    {
                        ProcessAnimation(entry, animations);

                        continue;
                    }

                    if (IsParticleFile.IsMatch(entry.FullName))
                    {
                        ProcessParticle(entry, particleDefinitions);

                        continue;
                    }

                    if (IsAttachableFile.IsMatch(entry.FullName))
                    {
                        ProcessAttachable(entry, attachableDefinitions);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn(ex, $"Could not process file in resource pack: '{entry.FullName}' continuing anyways...");
                }
            }

            TryAddBitmap("textures/entity/chest/double_normal");
            TryAddBitmap("textures/entity/enchanting_table_book");
            TryAddBitmap("textures/entity/beacon_beam");
            TryAddBitmap("textures/entity/banner/banner");
            TryAddBitmap("textures/entity/banner/banner_base");
            TryAddBitmap("textures/entity/banner/banner_stripe_bottom");
            TryAddBitmap("textures/entity/banner/banner_stripe_top");
            TryAddBitmap("textures/entity/banner/banner_stripe_left");
            TryAddBitmap("textures/entity/banner/banner_stripe_right");
            TryAddBitmap("textures/entity/banner/banner_stripe_center");
            TryAddBitmap("textures/entity/banner/banner_stripe_middle");
            TryAddBitmap("textures/entity/banner/banner_stripe_downright");
            TryAddBitmap("textures/entity/banner/banner_stripe_downleft");
            TryAddBitmap("textures/entity/banner/banner_small_stripes");
            TryAddBitmap("textures/entity/banner/banner_cross");
            TryAddBitmap("textures/entity/banner/banner_straight_cross");
            TryAddBitmap("textures/entity/banner/banner_diagonal_left");
            TryAddBitmap("textures/entity/banner/banner_diagonal_up_right");
            TryAddBitmap("textures/entity/banner/banner_diagonal_up_left");
            TryAddBitmap("textures/entity/banner/banner_diagonal_right");
            TryAddBitmap("textures/entity/banner/banner_half_vertical");
            TryAddBitmap("textures/entity/banner/banner_half_vertical_right");
            TryAddBitmap("textures/entity/banner/banner_half_horizontal");
            TryAddBitmap("textures/entity/banner/banner_half_horizontal_bottom");
            TryAddBitmap("textures/entity/banner/banner_square_bottom_left");
            TryAddBitmap("textures/entity/banner/banner_square_bottom_right");
            TryAddBitmap("textures/entity/banner/banner_square_top_left");
            TryAddBitmap("textures/entity/banner/banner_square_top_right");
            TryAddBitmap("textures/entity/banner/banner_triangle_bottom");
            TryAddBitmap("textures/entity/banner/banner_triangle_top");
            TryAddBitmap("textures/entity/banner/banner_triangles_bottom");
            TryAddBitmap("textures/entity/banner/banner_triangles_top");
            TryAddBitmap("textures/entity/banner/banner_circle");
            TryAddBitmap("textures/entity/banner/banner_rhombus");
            TryAddBitmap("textures/entity/banner/banner_border");
            TryAddBitmap("textures/entity/banner/banner_curly_border");
            TryAddBitmap("textures/entity/banner/banner_bricks");
            TryAddBitmap("textures/entity/banner/banner_gradient");
            TryAddBitmap("textures/entity/banner/banner_gradient_up");
            TryAddBitmap("textures/entity/banner/banner_creeper");
            TryAddBitmap("textures/entity/banner/banner_skull");
            TryAddBitmap("textures/entity/banner/banner_flower");
            TryAddBitmap("textures/entity/banner/banner_mojang");
            TryAddBitmap("textures/entity/banner/banner_pattern_illager");
            TryAddBitmap("textures/entity/banner/banner_piglin");

            ProcessEntityDefinitions(entityDefinitions);

            EntityModels = entityModels;
            EntityDefinitions = entityDefinitions.ToDictionary(x => new ResourceLocation(x.Key), v => v.Value);
            RenderControllers = renderControllers;
            AnimationControllers = animationControllers;
            Animations = animations;
            Particles = particleDefinitions;
            Attachables = attachableDefinitions;
        }

        private void ProcessEntityDefinitions(IDictionary<string, EntityDescription> definitions)
        {
            foreach (var def in definitions.Values)
            {
                try
                {
                    foreach (var texture in def.Textures)
                    {
                        if (_bitmaps.ContainsKey(texture.Value))
                        {
                            //Log.Warn($"Duplicate bitmap: {texture.Value}");
                            continue;
                        }

                        TryAddBitmap(texture.Value);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn($"Could not load texture! {ex}");
                }
            }
        }

        private void ProcessSoundBindings(IFile entry)
        {
            try
            {
                string json = entry.ReadAsEncodedString(ContentKey);
                SoundBindings = MCJsonConvert.DeserializeObject<SoundBindingsCollection>(json);
            }
            catch (Exception ex)
            {
                Log.Warn(ex, $"Failed to load soundbindings from file \"{entry.FullName}\"");
            }
        }

        private void ProcessTexture(IFile entry)
        {
        }

        private void ProcessAttachable(IFile entry, Dictionary<string, AttachableDefinition> attachableDefinitions)
        {
            try
            {
                string json = entry.ReadAsEncodedString(ContentKey);

                var versionedResource = MCJsonConvert.DeserializeObject<VersionedResource<AttachableDefinition>>(
                    json, new VersionedResourceConverter<AttachableDefinition>("minecraft:attachable", true, (def) => def.Description.Identifier));

                foreach (var controller in versionedResource.Values)
                {
                    if (attachableDefinitions.TryAdd(controller.Key, controller.Value))
                    {
                        if (controller.Value?.Description?.Textures != null)
                        {
                            foreach (var kv in controller.Value.Description.Textures)
                            {
                                var texture = kv.Value;

                                if (texture != null && !_bitmaps.ContainsKey(texture))
                                {
                                    TryAddBitmap(texture);
                                }
                            }
                        }
                    }

                    ;
                }
            }
            catch (MoLangParserException ex)
            {
                Log.Warn(ex, $"Failed to load attachable from file \"{entry.FullName}\"");
            }
        }

        private void ProcessParticle(IFile entry, Dictionary<string, ParticleDefinition> particleDefinitions)
        {
            try
            {
                string json = entry.ReadAsEncodedString(ContentKey);

                var versionedResource = MCJsonConvert.DeserializeObject<VersionedResource<ParticleDefinition>>(
                    json, new VersionedResourceConverter<ParticleDefinition>("particle_effect", true, (def) => def.Description.Identifier));

                foreach (var controller in versionedResource.Values)
                {
                    if (particleDefinitions.TryAdd(controller.Key, controller.Value))
                    {
                        string texture = controller.Value?.Description?.BasicRenderParameters?.Texture;

                        if (texture != null && !_bitmaps.ContainsKey(controller.Value.Description.BasicRenderParameters.Texture))
                        {
                            TryAddBitmap(texture);
                        }
                    }

                    ;
                }
            }
            catch (MoLangParserException ex)
            {
                Log.Warn(ex, $"Failed to load particle from file \"{entry.FullName}\"");
            }
        }

        private void ProcessAnimation(IFile entry, Dictionary<string, Animation> animations)
        {
            try
            {
                string json = entry.ReadAsEncodedString(ContentKey);

                var versionedResource = MCJsonConvert.DeserializeObject<VersionedResource<Animation>>(
                    json, new VersionedResourceConverter<Animation>("animations"));

                foreach (var controller in versionedResource.Values)
                {
                    if (controller.Value.AnimationTimeUpdate == null)
                        controller.Value.AnimationTimeUpdate = Animation.DefaultTimeUpdate;

                    animations.TryAdd(controller.Key, controller.Value);
                }
            }
            catch (MoLangParserException ex)
            {
                Log.Warn(ex, $"Failed to load animation from file \"{entry.FullName}\"");
            }
        }

        private void ProcessAnimationController(IFile entry, Dictionary<string, AnimationController> animationControllers)
        {
            string json = entry.ReadAsEncodedString(ContentKey);

            var versionedResource = MCJsonConvert.DeserializeObject<VersionedResource<AnimationController>>(
                json, new VersionedResourceConverter<AnimationController>("animation_controllers"));

            foreach (var controller in versionedResource.Values)
            {
                animationControllers.TryAdd(controller.Key, controller.Value);
            }
        }

        private void ProcessRenderController(IFile entry, Dictionary<string, RenderController> renderControllers)
        {
            string json = entry.ReadAsEncodedString(ContentKey);

            var versionedResource = MCJsonConvert.DeserializeObject<VersionedResource<RenderController>>(
                json, new VersionedResourceConverter<RenderController>("render_controllers"));

            foreach (var controller in versionedResource.Values)
            {
                renderControllers.TryAdd(controller.Key, controller.Value);
            }
        }

        public IReadOnlyDictionary<string, EntityModel> EntityModels { get; private set; }

        public static Dictionary<string, EntityModel> ProcessEntityModels(Dictionary<string, EntityModel> models, Func<string, EntityModel> lookup = null)
        {
            Dictionary<string, EntityModel> final = new Dictionary<string, EntityModel>(StringComparer.InvariantCultureIgnoreCase);
            Queue<KeyValuePair<string, EntityModel>> workQueue = new Queue<KeyValuePair<string, EntityModel>>();

            foreach (var model in models.OrderBy(x => x.Key.Count(k => k == ':')))
            {
                workQueue.Enqueue(model);
            }

            //var item = workQueue.First;

            //while (item.Next != null)
            while (workQueue.TryDequeue(out var item))
            {
                try
                {
                    var model = item.Value;

                    EntityModel result = null;

                    if (!item.Key.Contains(":"))
                    {
                        final.TryAdd(item.Key, model);
                        continue;
                    }

                    var split = item.Key.Split(':').Reverse().ToArray();
                    //string sb            = "";
                    bool wasInterupted = false;

                    for (int i = 0; i < split.Length; i++)
                    {
                        //	if (i == 0)
                        //		break;
                        EntityModel requiredEntityModel;
                        if (i == split.Length - 1) //Last item
                        {
                            requiredEntityModel = model;
                        }
                        else
                        {
                            var key = split[i];

                            if (!final.TryGetValue(key, out requiredEntityModel))
                            {
                                if (lookup != null)
                                {
                                    requiredEntityModel = lookup.Invoke(key);

                                    if (requiredEntityModel == null)
                                    {
                                        wasInterupted = true;
                                        workQueue.Enqueue(item);

                                        break;
                                    }
                                }
                                else
                                {
                                    //Log.Warn($"Could not find entity model: {key} of {item.Key}");
                                    wasInterupted = true;
                                    workQueue.Enqueue(item);

                                    break;
                                }
                            }
                        }

                        if (i == 0)
                        {
                            result = requiredEntityModel.Clone();
                        }
                        else
                        {
                            result += requiredEntityModel;
                        }

                        /*if (i > 0)
                        {
                            result += requiredEntityModel;
                        }
                        else
                        {
                            result = requiredEntityModel;
                        }*/

                        //final.TryAdd(split[i], result);
                    }

                    //models.TryAdd(sb, result);
                    if (!wasInterupted && result != null)
                    {
                        result.Description = model.Description.Clone();

                        if (!final.TryAdd(split[^1], result))
                        {
                            Log.Warn($"Failed to add {split[0]}");
                        }

                        //final[split[0]] = result;
                    }
                }
                finally
                {
                    //item = item.Next;
                    //item.Previous = null;
                    //	workQueue.Remove(item.Previous);
                }
            }

            return final;
        }

        private void LoadEntityModel(IFile entry, Dictionary<string, EntityModel> models)
        {
            try
            {
                string json = entry.ReadAsEncodedString(ContentKey);

                LoadEntityModel(json, models);
            }
            catch (Exception ex)
            {
                Log.Warn(ex, $"Failed to load entity model from file: {entry.FullName}");
            }
        }

        public static void LoadEntityModel(string json, Dictionary<string, EntityModel> models)
        {
            var d = MCJsonConvert.DeserializeObject<MobsModelDefinition>(json);

            //if (decoded == null)
            if (d != null)
            {
                foreach (var item in d)
                {
                    if (item.Value.Description?.Identifier == null)
                    {
                        Log.Warn($"Missing identifier for {item.Key}");

                        //return;
                    }

                    if (!models.TryAdd(item.Key, item.Value))
                    {
                        if (models.TryGetValue(item.Key, out var existingValue))
                        {
                            if (item.Value.FormatVersion > existingValue.FormatVersion)
                            {
                                models[item.Key] = item.Value;
                                return;
                            }
                        }

                        Log.Warn($"Duplicate geometry model: {item.Key}");
                    }
                }
            }
        }

        private bool TryLoadMobModels(Dictionary<string, EntityModel> models)
        {
            var mobsJson = _archive.GetEntry("models/mobs.json");

            if (mobsJson == null)
                return false;

            LoadEntityModel(mobsJson, models);

            return true;
        }

        private void ProcessSounds(ResourcePack.LoadProgress progress, IFile entry)
        {
            string json = entry.ReadAsEncodedString(ContentKey);

            SoundDefinitions = SoundDefinitionFormat.FromJson(json);
        }

        private void LoadEntityDefinition(IFile entry,
            Dictionary<string, EntityDescription> entityDefinitions)
        {
            string json = entry.ReadAsEncodedString(ContentKey);
            List<EntityDescription> definitions = new List<EntityDescription>();

            JObject obj = JObject.Parse(json, new JsonLoadSettings());

            FormatVersion format = FormatVersion.Unknown;

            if (obj.TryGetValue("format_version", out var ftv))
            {
                if (ftv.Type == JTokenType.String)
                {
                    format = FormatVersionHelpers.FromString(ftv.Value<string>());
                }
            }

            foreach (var e in obj)
            {
                if (e.Key == "format_version") continue;

                if (e.Key == "minecraft:client_entity" && e.Value != null)
                {
                    EntityDescription desc = null;
                    var clientEntity = (JObject)e.Value;

                    if (!clientEntity.TryGetValue("description", StringComparison.InvariantCultureIgnoreCase, out var descriptionToken) || descriptionToken.Type != JTokenType.Object)
                        continue;

                    desc = descriptionToken.ToObject<EntityDescription>(MCJsonConvert.Serializer);
                    if (desc == null)
                        continue;

                    desc.FormatVersion = format;

                    string engineText = desc.MinEngineVersion;
                    if (!string.IsNullOrWhiteSpace(engineText))
                    {
                        desc.MinEngine = FormatVersionHelpers.FromString(engineText);
                    }
                    else if (((JObject)descriptionToken).TryGetValue(
                                 "min_engine_version", StringComparison.InvariantCultureIgnoreCase, out var minEngineVersion))
                    {
                        desc.MinEngine = FormatVersionHelpers.FromString(minEngineVersion.Value<string>());
                    }

                    if (desc.MinEngine == FormatVersion.V1_8_0)
                        continue;

                    definitions.Add(desc);
                }
            }

            foreach (var items in definitions.GroupBy(x => x.Identifier).ToArray())
            {
                var values = items.ToArray();

                foreach (var def in values.OrderByDescending(x => x.MinEngine.GetValueOrDefault(FormatVersion.Unknown)))
                {
                    if (def.Textures == null) continue;

                    EntityDescription existing = null;
                    entityDefinitions.TryGetValue(def.Identifier, out existing);

                    if (existing == null
                        || ((existing.MinEngine.HasValue && def.MinEngine.HasValue
                                                         && def.MinEngine > existing.MinEngine)
                            || (existing.MinEngine == null && def.MinEngine != null
                                                           && def.MinEngine != FormatVersion.Unknown)))
                    {
                        entityDefinitions[def.Identifier] = def;
                    }
                }
            }
        }

        private void ProcessFontFile(ResourcePack.LoadProgress progress, IFile entry)
        {
            //TODO: Process server sent custom font glyph files (glyph_E1 starts at E100 in unicode, so \uE100)
            return;
            var image = TryLoad(entry.FullName);

            if (image != null)
            {
            }
        }

        private bool TryAddBitmap(string path)
        {
            if (_bitmaps.ContainsKey(path))
                return false;

            if (_archive.GetEntry(path + ".tga") != null)
            {
                return _bitmaps.TryAdd(path, new Func<Image<Rgba32>>(
                    () => { return TryLoad(path + ".tga"); }));
            }
            else if (_archive.GetEntry(path + ".png") != null)
            {
                return _bitmaps.TryAdd(path, new Func<Image<Rgba32>>(
                    () => { return TryLoad(path + ".png"); }));
            }

            return false;
        }

        private Image<Rgba32> TryLoad(string file)
        {
            var entry = _archive.GetEntry(file);
            if (entry != null)
            {
                Image<Rgba32> bmp = null;
                using (var fs = entry.OpenEncoded(ContentKey))
                {
                    if (file.EndsWith(".tga"))
                    {
                        bmp = Image.Load(fs, new TgaDecoder()).CloneAs<Rgba32>();
                    }
                    else
                    {
                        bmp = Image.Load<Rgba32>(fs);
                    }

                    //	bmp = new Image(fs);
                }

                return bmp;
            }

            return null;
        }

        public bool TryGetBitmap(ResourceLocation textureName, out Image<Rgba32> bitmap)
        {
            if (Textures.TryGetValue(new ResourceLocation(textureName.Namespace, NormalisePath(textureName.Path)), out var t))
            {
                bitmap = t();

                return bitmap != null;
            }

            bitmap = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetTextureMeta(ResourceLocation textureName, out TextureMeta meta)
        {
            meta = null;
            return false;
        }

        public void Dispose()
        {
            _archive?.Dispose();
        }

        /// <inheritdoc />
        public bool TryGetAnimationController(string key, out AnimationController animationController)
        {
            return AnimationControllers.TryGetValue(key, out animationController);
        }

        /// <inheritdoc />
        public bool TryGetAnimation(string key, out Animation animation)
        {
            return Animations.TryGetValue(key, out animation);
        }

        /// <inheritdoc />
        public bool TryGetRenderController(string key, out RenderController renderController)
        {
            return RenderControllers.TryGetValue(key, out renderController);
        }
    }
}