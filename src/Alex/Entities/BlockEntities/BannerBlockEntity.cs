using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using Alex.Blocks.Minecraft;
using Alex.Common.Blocks;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.Worlds;
using DiscordRPC.Helper;
using fNbt;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;
using NLog.Fluent;
using RocketUI.Utilities.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Binarization;
using SixLabors.ImageSharp.Processing.Processors.Filters;
using Color = SixLabors.ImageSharp.Color;
using ColorMatrix = SixLabors.ImageSharp.ColorMatrix;
using ModelBone = Alex.Graphics.Models.ModelBone;
using Point = SixLabors.ImageSharp.Point;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Microsoft.Xna.Framework;
using NLog;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace Alex.Entities.BlockEntities;

public class BannerBlockEntity : BlockEntity
{
    private BlockColor _bannerColor;
    private EntityDescription _entityDescription = null;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BannerBlockEntity));
    private ModelBone RootBone { get; set; }

    private byte _rotation = 0;
    private float _yRotation = 0f;

    public IReadOnlyCollection<PatternLayer> Patterns
    {
        get => _patterns;
        private set
        {
            if (_patterns?.SequenceEqual(value) ?? false)
                return;

            _patterns = value;
            _isTextureDirty = true;
        }
    }

    private Image<Rgba32> _canvasTexture;

    public BlockColor BannerColor
    {
        get => _bannerColor;
        set
        {
            if (_bannerColor == value)
                return;

            _bannerColor = value;
            _isTextureDirty = true;
        }
    }

    public byte BannerRotation
    {
        get => _rotation;
        set
        {
            _rotation = Math.Clamp(value, (byte)0, (byte)15);

            _yRotation = _rotation * -22.5f;
            if (RootBone != null)
            {
                var headRotation = RootBone.Rotation;
                headRotation.Y = _yRotation;
                RootBone.Rotation = headRotation;
            }
            //HeadBone.Rotation = headRotation;
        }
    }

    public BannerBlockEntity(World level, BlockCoordinates coordinates) : base(level)
    {
//        _color = color;
        Type = "minecraft:standing_banner";

        Width = 1f;
        Height = 2f;

        Offset = new Vector3(0.5f, 0f, 0.5f);

        X = coordinates.X;
        Y = coordinates.Y;
        Z = coordinates.Z;

        HideNameTag = true;
        IsAlwaysShowName = false;
        AnimationController.Enabled = true;

        Patterns = Array.Empty<PatternLayer>();
        BannerColor = BlockColor.White;
    }

    /// <inheritdoc />
    protected override void UpdateModelParts()
    {
        base.UpdateModelParts();

        if (ModelRenderer != null && ModelRenderer.GetBone("root", out var bone))
        {
            var rot = bone.Rotation;
            rot.Y = _yRotation;
            bone.Rotation = rot;
            RootBone = bone;
        }
    }

    private bool _isTextureDirty;
    private IReadOnlyCollection<PatternLayer> _patterns;

    private void UpdateCanvasTexture()
    {
        if (_isTextureDirty)
        {
            _canvasTexture ??= new Image<Rgba32>(64, 64, Color.Black);
            _canvasTexture.Mutate(cxt => { cxt.Clear(Color.Black); });

            ApplyCanvasLayer(new PatternLayer(Common.Utils.BannerColor.FromId((int)BannerColor), BannerPattern.Base));

            if (Patterns?.Count > 0)
            {
                foreach (var layer in Patterns)
                {
                    if (layer == null) continue;

                    ApplyCanvasLayer(layer);
                }
            }

            _isTextureDirty = false;
        }

        //if (ModelRenderer != null)
        //{
        var newTexure = TextureUtils.BitmapToTexture2D(this, Alex.Instance.GraphicsDevice, _canvasTexture.Clone());
        if (Texture != null)
        {
            var oldTexture = Texture;
            Texture = newTexure;
            oldTexture?.Dispose();
        }
    }

    private void ApplyCanvasLayer(PatternLayer layer)
    {
        using var texture = ResolvePatternMask(layer.Pattern);

        if (texture == null)
        {
            Log.Warn($"Could not resolve pattern mask/texture for {layer.Pattern}");
            return;
        }

        //var color = Color.FromRgba(layer.Color.Color.R, layer.Color.Color.G, layer.Color.Color.B, layer.Color.Color.A);
        var color = layer.Color.Color;

        for (int x = 0; x < texture.Width; x++)
        for (int y = 0; y < texture.Height; y++)
        {
            var c = texture[x, y];
            if (c.A > 128)
            {
                var d = _canvasTexture[x, y];

                d.R = color.R;
                d.G = color.G;
                d.B = color.B;
                d.A = c.A;
                _canvasTexture[x, y] = d;
            }
        }
    }

    private Image<Rgba32> ResolvePatternMask(BannerPattern pattern)
    {
        if (_entityDescription?.Textures == null)
        {
            Log.Warn($"Unable to resolve pattern mask due to null entity description!");
            return null;
        }

        var key = pattern.ToString().ToLowerSnakeCase();

        if (_entityDescription.Textures.TryGetValue(key, out var patternMaskPath))
        {
            Image<Rgba32> patternMask;
            if (Alex.Instance.Resources.TryGetBedrockBitmap(patternMaskPath, out patternMask))
            {
            }
            else if (Alex.Instance.Resources.TryGetBitmap(patternMaskPath, out patternMask))
            {
            }
            else
            {
                Log.Warn($"Could not resolve texture for pattern {pattern} (Path='{patternMaskPath}')");
                throw new InvalidOperationException();
            }

            if (patternMask == null) return null;
            //
            // var cloned = patternMask.Clone(cxt =>
            // {
            //     cxt.Crop(new Rectangle(0, 0, 64, 64));
            // });

//            patternMask.Dispose();
            return patternMask;
        }
        else
        {
            Log.Warn($"Entity definition does not contain an entry for '{key}'");
        }

        return null;
    }

    protected override bool BlockChanged(Block oldBlock, Block newBlock)
    {
        var success = false;
        if (newBlock is WallBanner wallBanner)
        {
            Type = "minecraft:wall_banner";
            BannerColor = wallBanner.Color;

            if (newBlock.BlockState.TryGetValue("facing", out var facing))
            {
                if (Enum.TryParse<BlockFace>(facing, true, out var face))
                {
                    switch (face)
                    {
                        case BlockFace.West:
                            BannerRotation = 4;
                            break;

                        case BlockFace.East:
                            BannerRotation = 12;
                            break;

                        case BlockFace.North:
                            BannerRotation = 8;
                            break;

                        case BlockFace.South:
                            BannerRotation = 0;
                            break;
                    }
                }
            }

            success = true;
        }

        if (newBlock is StandingBanner standingBanner)
        {
            Type = "minecraft:standing_banner";
            BannerColor = standingBanner.Color;

            if (newBlock.BlockState.TryGetValue("rotation", out var r))
            {
                if (byte.TryParse(r, out var rot))
                {
                    BannerRotation = (byte)rot; // // ((rot + 3) % 15);
                }
            }

            success = true;
        }

        if (_entityDescription == null)
        {
            if (Alex.Instance.Resources.TryGetEntityDefinition(
                    Type, out var entityDescription, out var source))
            {
                _entityDescription = entityDescription;
                AnimationController?.UpdateEntityDefinition(source, source, entityDescription);
            }
        }

        UpdateCanvasTexture();

        return success;
    }

    protected override void ReadFrom(NbtCompound compound)
    {
        base.ReadFrom(compound);
        if (compound != null)
        {
            if (compound.TryGet<NbtList>("Patterns", out var patterns))
            {
                var newPatterns = new PatternLayer[patterns.Count];
                for (int i = 0; i < patterns.Count; i++)
                {
                    var patternCompound = patterns.Get<NbtCompound>(i);
                    BannerColor lColor = Common.Utils.BannerColor.White;
                    BannerPattern lPattern = BannerPattern.Base;

                    if (patternCompound.TryGet<NbtInt>("Color", out var color))
                    {
                        lColor = Common.Utils.BannerColor.FromId(color.IntValue);
                    }

                    if (patternCompound.TryGet<NbtString>("Pattern", out var pattern))
                    {
                        if (EnumHelper.TryParseUsingEnumMember<BannerPattern>(pattern.StringValue, out var bannerPattern))
                        {
                            lPattern = bannerPattern;
                        }
                    }

                    newPatterns[i] = new PatternLayer(lColor, lPattern);
                }

                Patterns = newPatterns;

                UpdateCanvasTexture();
            }
        }
    }

    public override void SetData(BlockEntityActionType action, NbtCompound compound)
    {
        if (action == BlockEntityActionType.SetBannerProperties || action == BlockEntityActionType._Init)
        {
            ReadFrom(compound);
        }
        else
        {
            base.SetData(action, compound);
        }
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        _canvasTexture?.Dispose();
        _canvasTexture = null;
    }

    public class PatternLayer
    {
        public BannerColor Color { get; }
        public BannerPattern Pattern { get; }

        public PatternLayer(BannerColor color, BannerPattern pattern)
        {
            Color = color;
            Pattern = pattern;
        }
    }
}