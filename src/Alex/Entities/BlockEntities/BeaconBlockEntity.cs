using System;
using System.Linq;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Common.Utils.Vectors;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using fNbt;
using Microsoft.Xna.Framework;
using MiNET.Effects;

namespace Alex.Entities.BlockEntities;

public class BeaconBlockEntity : BlockEntity
{
	private static readonly EffectType[] _validPrimaryEffects =
	{
		EffectType.None, EffectType.Speed, EffectType.Haste, EffectType.Resistance, EffectType.JumpBoost,
		EffectType.Strength
	};

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
		if (!_canActivate)
		{
			IsInvisible = true;

			return;
		}


		var model = ModelRenderer?.Model;

		if (model == null) return;

		//model.Root.ScaleOverTime(new Vector3(1f, (float)Height, 1f), 500, true);
		//model.Root.Scale = new Vector3(1f, Level.GetChunkColumn(X, Z).WorldSettings.TotalHeight - Y, 1f);
	}

	protected override void ReadFrom(NbtCompound compound)
	{
		UpdateHeight();

		if (compound == null)
			return;

		if (compound.TryGet<NbtInt>("Primary", out var primary) || compound.TryGet<NbtInt>("primary", out primary))
		{
			if (primary.Value >= 0)
			{
				PrimaryEffect = (EffectType)primary.Value;
			}
		}

		if (compound.TryGet<NbtInt>("Secondary", out var secondary)
		    || compound.TryGet<NbtInt>("secondary", out primary))
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

	/// <inheritdoc />
	protected override bool BlockChanged(Block oldBlock, Block newBlock)
	{
		var level = Level;

		if (level != null)
		{
			level.ChunkManager.OnChunkUpdate += OnChunkUpdate;
		}

		return base.BlockChanged(oldBlock, newBlock);
	}

	private void OnChunkUpdate(object sender, ChunkUpdatedEventArgs e)
	{
		if (e.Position == new ChunkCoordinates(KnownPosition))
		{
			_nextUpdate = Age + (Math.Max(20 * 5, Math.Min(Age - _lastUpdate, 0)));
		}
	}

	private long _nextUpdate = 0;
	private long _lastUpdate = 0;
	private bool _canActivate = false;

	/// <inheritdoc />
	public override void OnTick()
	{
		base.OnTick();

		var now = Age;

		if (now >= _nextUpdate)
		{
			_lastUpdate = now;
			Levels = GetPyramidLevels(Level);
			_canActivate = CanActivate(Level);

			if (Level.ChunkManager.TryGetChunk(new ChunkCoordinates(new BlockCoordinates(X, Y, Z)), out var chunk))
			{
				Height = chunk.WorldSettings.TotalHeight - Y;
			}

			if (!_canActivate && !IsInvisible)
			{
				IsInvisible = true;
			}
			else if (_canActivate && IsInvisible)
			{
				IsInvisible = false;
			}
			//_nextUpdate = 
		}
	}

	private bool CanActivate(IBlockAccess level)
	{
		var height = level.GetHeight(new BlockCoordinates(X, Y, Z));

		return height <= Y;
	}

	private int GetPyramidLevels(IBlockAccess level)
	{
		for (int i = 1; i < 5; i++)
		{
			for (int x = -i; x < i + 1; x++)
			{
				for (int z = -i; z < i + 1; z++)
				{
					var block = level.GetBlockState(new BlockCoordinates(X + x, Y + -i, Z + z))?.Block;

					if (block is DiamondBlock || block is IronBlock || block is GoldBlock || block is EmeraldBlock)
						continue;

					return i - 1;
				}
			}
		}

		return 4;
	}

	/// <inheritdoc />
	protected override void OnDispose()
	{
		var manager = Level?.ChunkManager;

		if (manager != null)
		{
			manager.OnChunkUpdate -= OnChunkUpdate;
		}

		base.OnDispose();
	}
}