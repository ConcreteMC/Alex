using System;
using Alex.Blocks.Minecraft;
using Alex.Common.Blocks;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Graphics.Models;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework;

namespace Alex.Entities.BlockEntities;

public class BannerBlockEntity : BlockEntity
{
    private ModelBone RootBone { get; set; }

    private byte _rotation = 0;
    private float _yRotation = 0f;

    public PatternLayer[] Patterns { get; set; } = Array.Empty<PatternLayer>();
    
    public byte Rotation
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
            //rot.Y = _yRotation;
            //bone.Rotation = rot;

            RootBone = bone;
        }
    }

    protected override bool BlockChanged(Block oldBlock, Block newBlock)
    {
        if (newBlock is WallBanner)
        {
            Type = "minecraft:wall_banner";
            
            if (newBlock.BlockState.TryGetValue("facing", out var facing))
            {
                if (Enum.TryParse<BlockFace>(facing, true, out var face))
                {
                    switch (face)
                    {
                        case BlockFace.West:
                            Rotation = 4;
                            break;

                        case BlockFace.East:
                            Rotation = 12;
                            break;

                        case BlockFace.North:
                            Rotation = 8;
                            break;

                        case BlockFace.South:
                            Rotation = 0;
                            break;
                    }
                }
            }

            return true;
        }

        if (newBlock is StandingBanner)
        {
            Type = "minecraft:standing_banner";
            
            if (newBlock.BlockState.TryGetValue("rotation", out var r))
            {
                if (byte.TryParse(r, out var rot))
                {
                    Rotation = (byte)rot; // // ((rot + 3) % 15);
                }
            }

            return true;
        }

        return false;
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
                        newPatterns[i].Color = DyeColor.FromId(color.IntValue);
                    }

                    if (patternCompound.TryGet<NbtString>("Pattern", out var pattern))
                    {
                        if (Enum.TryParse<BannerPattern>(pattern.StringValue, true, out var bannerPattern))
                        {
                            newPatterns[i].Pattern = bannerPattern;
                        }
                    }
                }

                Patterns = newPatterns;
            }
        }
    }

    public override void SetData(BlockEntityActionType action, NbtCompound compound)
    {
        if (action == BlockEntityActionType.SetBannerProperties)
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
        public DyeColor Color { get; set; }
        public BannerPattern Pattern { get; set; }
    }
}