using System;
using System.Drawing.Imaging;
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
    private BlockColor _color;
    private EntityDescription _entityDescription = null;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BannerBlockEntity));
    private ModelBone RootBone { get; set; }

    private byte _rotation = 0;
    private float _yRotation = 0f;

    public PatternLayer[] Patterns { get; set; } = Array.Empty<PatternLayer>();
    private Image<Rgba32> _canvasTexture;
    
    public byte BannerRotation
    {
        get { return _rotation; }
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

    private void UpdateCanvasTexture()
    {
        _canvasTexture ??= new Image<Rgba32>(64, 64, Color.Black);
        _canvasTexture.Mutate(cxt => { cxt.Clear(Color.Black); });

        ApplyCanvasLayer(new PatternLayer() { Pattern = BannerPattern.Base, Color = BannerColor.FromId((int)_color) });

        if (!(Patterns == null || Patterns.Length == 0))
        {
            for (int i = 0; i < Patterns.Length; i++)
            {
                var layer = Patterns[i];
                if (layer == null) continue;

                ApplyCanvasLayer(layer);
            }
        }

        //if (ModelRenderer != null)
        //{
        Texture = TextureUtils.BitmapToTexture2D(this, Alex.Instance.GraphicsDevice, _canvasTexture.Clone());
        //}
    }

    private void ApplyCanvasLayer(PatternLayer layer)
    {
        var texture = ResolvePatternMask(layer.Pattern);

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
    
    
    private void ApplyCanvasLayerTheShittyWay(PatternLayer layer)
    {
        var texture = ResolvePatternMask(layer.Pattern);

        if (texture == null)
        {
            Log.Warn($"Could not resolve pattern mask/texture for {layer.Pattern}");
            return;
        }

        //var color = Color.FromRgba(layer.Color.Color.R, layer.Color.Color.G, layer.Color.Color.B, layer.Color.Color.A);
        var color = layer.Color.Color;
        
        texture.Mutate(cxt =>
        {
            cxt.SetGraphicsOptions(opt =>
            {
                opt.ColorBlendingMode = PixelColorBlendingMode.Normal;
                opt.AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver;
            });
            cxt .BackgroundColor(Color.Transparent)
                //.AdaptiveThreshold(Color.White, Color.Black, 0.5f)
                //.DetectEdges()
                //.Fill(new RecolorBrush(Color.White, color, 0.5f))
                //.Quantize()
                //.Dither()
                .ProcessPixelRowsAsVector4(row =>
                {
                    var totalW = 0;
                    var abovehalfW = 0;
                    
                    for (int i = 0; i < row.Length; i++)
                    {
                        totalW++;
                        var c = row[i];
                        if (c.W <= 0.5f)
                        {
                            abovehalfW++;
                            //row[i].X = 0f;
                            //row[i].Y = 0f;
                            //row[i].Z = 0f;
                            //row[i].W = 0f;
                        }
                        else
                        {
                            var w = row[i].W;
                            var c2 = color.ToVector4();
                            row[i].X = c2.X;
                            row[i].Y = c2.Y;
                            row[i].Z = c2.Z;
                            //row[i].W = c2.W;
                        }
                    }

                    Log.Warn($"Big wieener counter: {abovehalfW}/{totalW}");
                })
                ;
            var str = $"hello {color.ToHexString()}";
        });


        _canvasTexture.Mutate(cxt =>
        {
            cxt.SetGraphicsOptions(opt =>
            {
                opt.ColorBlendingMode = PixelColorBlendingMode.Normal;
                opt.AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver;
                opt.BlendPercentage = 0.5f;
            });
                cxt.DrawImage(texture,
                    new Point(0, 0),
                    PixelColorBlendingMode.Normal,
                    PixelAlphaCompositionMode.SrcOver,
                    1f);
            
        });
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
            var img = new Image<Rgba32>(patternMask.Width, patternMask.Height, new Rgba32(1f, 1f, 1f, 0f));
            img.Mutate(cxt =>
            {
                cxt.DrawImage(patternMask, new Point(0, 0), 1f);
            });
            return patternMask.Clone(cxt =>
            {
                cxt.Crop(new Rectangle(0, 0, 64, 64));
            });
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
            _color = wallBanner.Color;

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
            _color = standingBanner.Color;

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
                    newPatterns[i] = new PatternLayer();

                    if (patternCompound.TryGet<NbtInt>("Color", out var color))
                    {
                        newPatterns[i].Color = BannerColor.FromId(color.IntValue);
                    }

                    if (patternCompound.TryGet<NbtString>("Pattern", out var pattern))
                    {
                        if (EnumHelper.TryParseUsingEnumMember<BannerPattern>(pattern.StringValue, out var bannerPattern))
                        {
                            newPatterns[i].Pattern = bannerPattern;
                        }
                    }
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

    public class PatternLayer
    {
        public BannerColor Color { get; set; }
        public BannerPattern Pattern { get; set; }
    }
}