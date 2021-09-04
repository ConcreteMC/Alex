using System;
using System.Linq;
using Alex.Blocks.Materials;
using Alex.Blocks.Minecraft;
using Alex.Common.Items;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Entities.Components;
using Alex.Items;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities
{
	public class LivingEntity : Entity
	{
		public bool IsLeftHanded { get; set; } = false;
		private bool _hasPhysics = true;
		internal bool HasPhysics
		{
			get => _hasPhysics;
			set
			{
				PhysicsComponent.Enabled = value;
				_hasPhysics = value;
				
				if (!value)
					Velocity = Vector3.Zero;
			}
		}
		
		/*internal override PlayerLocation RenderLocation
		{
			get
			{
				return HasPhysics ? base.RenderLocation : KnownPosition;
			}
			set
			{
				if (HasPhysics)
				{
					base.RenderLocation = value;
				}
				else
				{
					KnownPosition = value;
				}
			}
		}*/
		
		protected PhysicsComponent PhysicsComponent { get; }
		/// <inheritdoc />
		public LivingEntity(World level) : base(
			level)
		{
			_hasPhysics = false;
			EntityComponents.Push(PhysicsComponent = new PhysicsComponent(this)
			{
				Enabled = false
			});
		}
		
		public Item GetItemInHand(bool mainHand)
		{
			return mainHand ? Inventory.MainHand : Inventory.OffHand;
		}
		
		//TODO: Handle hand animations
		
		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 7 && entry is MetadataByte data)
			{
				bool handActive = (data.Value & 0x01) != 0;

				if (handActive)
				{
					bool offHandActive = (data.Value & 0x02) != 0;
					var item = GetItemInHand(!offHandActive);

					if (item != null)
					{
						if (item is ItemEdible) //Food or drink
						{
							IsEating = true;
						}
						else if (item.ItemType == ItemType.Sword || item.ItemType == ItemType.Shield)
						{
							IsBlocking = true;
						} 
						else if (!(item is ItemAir) && item.Count > 0)
						{
							IsUsingItem = true;
						}
					}
				}
				else
				{
					IsBlocking = false;
					IsEating = false;
					IsUsingItem = false;
				}
			}
			else if (entry.Index == 8 && entry is MetadataFloat flt)
			{
				HealthManager.Health = flt.Value;
			}
		}
		
		private bool _waitingOnChunk = true;
		public bool HasChunk => !_waitingOnChunk;

		
		/// <inheritdoc />
		public override void OnTick()
		{
			if (_waitingOnChunk)
			{
				if (Level.GetChunk(KnownPosition.GetCoordinates3D(), true) != null)
				{
					_waitingOnChunk = false;
				}
			}
			
			if (_isHit && Age > _hitAnimationEnd)
			{
				_isHit = false;
				
				if (ModelRenderer != null)
					ModelRenderer.EntityColor = Color.White.ToVector3();
			}
			
			base.OnTick();
			
			if (NoAi || _waitingOnChunk) return;
			//	IsMoving = Velocity.LengthSquared() > 0f;

			var renderPosition = RenderLocation;
			var knownDown = renderPosition.GetCoordinates3D();

			//	if (Alex.ServerType == ServerType.Bedrock)
			{
				knownDown = knownDown.BlockDown();
			}

			var entityBoundingBox = GetBoundingBox(renderPosition);
			var blockBelowFeet = Level?.GetBlockStates(knownDown.X, knownDown.Y, knownDown.Z);

			if (blockBelowFeet != null)
			{
				if (blockBelowFeet.Any(b => b.State.Block.BlockMaterial == Material.Water))
				{
					AboveWater = true;
				}
				else
				{
					AboveWater = false;
				}
			}
			else
			{
				AboveWater = false;
			}

			bool feetInWater = false;
			bool feetInLava = false;
			bool headInWater = false;
			bool headInLava = false;
			foreach (var corner in entityBoundingBox.GetCorners())
			{
				if (Math.Abs(corner.Y - entityBoundingBox.Min.Y) < 0.001f) //Check feet.
				{
					var blockcoords = new BlockCoordinates(
						new PlayerLocation(corner.X, (float) (corner.Y + (Height * 0.5f)), corner.Z));

					var feetBlock = Level?.GetBlockState(blockcoords.X, blockcoords.Y, blockcoords.Z);

					if (feetBlock != null)
					{
						if (!feetInWater)
						{
							if (feetBlock.Block.BlockMaterial == Material.Water)
							{
								feetInWater = true;
							}
							else
							{
								feetInWater = false;
							}
						}

						if (!feetInLava)
						{
							if (feetBlock.Block.BlockMaterial == Material.Lava)
							{
								feetInLava = true;
							}
							else
							{
								feetInLava = false;
							}
						}
					}
				}
				else //Check head.
				{
					var blockcoords = new BlockCoordinates(
						new PlayerLocation(corner.X, Math.Floor(corner.Y), corner.Z));

					var headBlock = Level?.GetBlockState(blockcoords.X, blockcoords.Y, blockcoords.Z);

					if (headBlock != null)
					{
						if (!headInWater)
						{
							if (headBlock.Block.BlockMaterial == Material.Water)
							{
								headInWater = true;
							}
							else
							{
								headInWater = false;
							}
						}

						if (!headInLava)
						{
							if (headBlock.Block.BlockMaterial == Material.Lava)
							{
								headInLava = true;
							}
							else
							{
								headInLava = false;
							}
						}
					}
				}
			}

			FeetInWater = feetInWater;
			FeetInLava = feetInLava;

			HeadInWater = headInWater;
			HeadInLava = headInLava;
			
			IsInWater = FeetInWater || HeadInWater;
			IsInLava = FeetInLava || HeadInLava;

			if (IsSwimming && (!feetInWater || !headInWater))
				IsSwimming = false;
		}
		
		private long _hitAnimationEnd = 0;
		private bool _isHit = false;
		/// <inheritdoc />
		public override void EntityHurt()
		{
			base.EntityHurt();
			if (ModelRenderer == null)
				return;

			_isHit = true;
			_hitAnimationEnd = Age + 5;
		
			ModelRenderer.EntityColor = Color.Red.ToVector3();
		}

		/// <inheritdoc />
		public override void HandleEntityStatus(byte status)
		{
			if (status == 2) // Plays the hurt animation and hurt sound 
			{
				EntityHurt();
				return;
			}
			else if (status == 3) // Plays the death sound and death animation 
			{
				EntityDied();
				return;
			}
			base.HandleEntityStatus(status);
		}
	}
}
