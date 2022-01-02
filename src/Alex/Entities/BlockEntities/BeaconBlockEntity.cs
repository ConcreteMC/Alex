using System;
using System.Linq;
using Alex.Common.Utils.Vectors;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework;
using MiNET.Effects;

namespace Alex.Entities.BlockEntities;

public class BeaconBlockEntity : BlockEntity
{
    private static readonly EffectType[] _validPrimaryEffects = { EffectType.None, EffectType.Speed, EffectType.Haste, EffectType.Resistance, EffectType.JumpBoost, EffectType.Strength };
    private static readonly EffectType[] _validSecondaryEffects = { EffectType.None, EffectType.Regeneration };

    private EffectType _primaryEffect = EffectType.None;
    private EffectType _secondaryEffect = EffectType.None;

    public EffectType PrimaryEffect
    {
        get => _primaryEffect;
        set
        {
            if (!_validPrimaryEffects.Contains(value))
                throw new InvalidOperationException();

            _primaryEffect = value;
        }
    }

    public EffectType SecondaryEffect
    {
        get => _secondaryEffect;
        set
        {
            if (!(_validSecondaryEffects.Contains(value) || value == _primaryEffect))
                throw new InvalidOperationException();

            _secondaryEffect = value;
        }
    }

    public int Levels { get; set; } = 0;

    public BeaconBlockEntity(World level, BlockCoordinates coordinates) : base(level)
    {
        Type = "minecraft:beacon_beam";

        Width = 1f;
        Height = 128f;

        Offset = new Vector3(0.5f, 0f, 0.5f);

        X = coordinates.X;
        Y = coordinates.Y;
        Z = coordinates.Z;

        HideNameTag = true;
        IsAlwaysShowName = false;
        AnimationController.Enabled = true;
    }

    protected override void OnModelUpdated()
    {
        base.OnModelUpdated();

        UpdateHeight();
    }

    private void UpdateHeight()
    {
        return;
        if (ModelRenderer?.Model == null) return;

        ModelRenderer.Model.Root.Scale = new Vector3(1f, Level.GetChunkColumn(X, Z).WorldSettings.TotalHeight - Y, 1f);
    }

    protected override void ReadFrom(NbtCompound compound)
    {
        UpdateHeight();
        
        if (compound == null)
            return;

        if (compound.TryGet<NbtInt>("Primary", out var primary))
        {
            if (primary.Value >= 0)
            {
                PrimaryEffect = (EffectType)primary.Value;
            }
        }

        if (compound.TryGet<NbtInt>("Secondary", out var secondary))
        {
            if (secondary.Value >= 0)
            {
                SecondaryEffect = (EffectType)secondary.Value;
            }
        }

        if (compound.TryGet<NbtInt>("Levels", out var levels))
        {
            if (levels.Value >= 0)
            {
                Levels = levels.Value;
            }
        }
    }
    
    public override void SetData(BlockEntityActionType action, NbtCompound compound)
    {
        if (action == BlockEntityActionType.SetBeaconData || action == BlockEntityActionType._Init)
        {
            ReadFrom(compound);
        }
        else
        {
            base.SetData(action, compound);
        }
    }
}