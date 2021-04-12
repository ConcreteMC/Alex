using System.Linq;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
using Alex.Blocks.Minecraft;
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
		
		internal override PlayerLocation RenderLocation
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
		}
		
		private PhysicsComponent PhysicsComponent { get; }
		/// <inheritdoc />
		public LivingEntity(World level) : base(
			level)
		{
			EntityComponents.Push(PhysicsComponent = new PhysicsComponent(this));
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

		public long Age { get; set; } = 0;
		/// <inheritdoc />
		public override void OnTick()
		{
			Age++;
			
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
				ModelRenderer.EntityColor = Color.White.ToVector3();
			}
			
			base.OnTick();
			
			if (NoAi || _waitingOnChunk) return;
			//	IsMoving = Velocity.LengthSquared() > 0f;

			var knownPos  = new BlockCoordinates(new Vector3(KnownPosition.X, KnownPosition.Y, KnownPosition.Z));
			var knownDown = KnownPosition.GetCoordinates3D();

			//	if (Alex.ServerType == ServerType.Bedrock)
			{
				knownDown = knownDown.BlockDown();
			}

			var blockBelowFeet = Level?.GetBlockStates(knownDown.X, knownDown.Y, knownDown.Z);
			var feetBlock      = Level?.GetBlockStates(knownPos.X, knownPos.Y, knownPos.Z).ToArray();
			var headBlockState = Level?.GetBlockState(KnownPosition.GetCoordinates3D() + new BlockCoordinates(0, 1, 0));

			if (headBlockState != null)
			{
				var headBlock = headBlockState.Block;

				if (headBlock.Solid)
				{
					HeadInBlock = true;
				}
				else
				{
					HeadInBlock = false;
				}

				if (headBlock.BlockMaterial == Material.Water || headBlock.IsWater)
				{
					HeadInWater = true;
				}
				else
				{
					HeadInWater = false;
				}

				if (headBlock.BlockMaterial == Material.Lava || headBlock is Lava || headBlock is FlowingLava)
				{
					HeadInLava = true;
				}
				else
				{
					HeadInLava = false;
				}
			}

			if (blockBelowFeet != null)
			{
				if (blockBelowFeet.Any(b => b.State.Block.BlockMaterial == Material.Water || b.State.Block.IsWater))
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

			if (feetBlock != null)
			{
				if (feetBlock.Any(b => b.State.Block.BlockMaterial == Material.Water || b.State.Block.IsWater))
				{
					FeetInWater = true;
				}
				else
				{
					FeetInWater = false;
				}

				if (feetBlock.Any(b => b.State.Block.BlockMaterial == Material.Lava))
				{
					FeetInLava = true;
				}
				else
				{
					FeetInLava = false;
				}
			}

			IsInWater = FeetInWater || HeadInWater;
			IsInLava = FeetInLava || HeadInLava;
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
