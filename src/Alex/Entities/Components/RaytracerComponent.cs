using System.Collections.Generic;
using System.Linq;
using Alex.Blocks;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Utils.Vectors;
using Alex.Common.World;
using Alex.Interfaces;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Components
{
	public class RaytracerComponent : EntityComponent, ITicked
	{
		public bool HasValue { get; private set; } = false;

		public Vector3 CursorPosition { get; set; } = Vector3.Zero;
		public Block TracedBlock { get; private set; } = null;
		public BlockCoordinates ResultingCoordinates { get; private set; }
		public BlockFace Face { get; private set; } = BlockFace.None;
		public BlockCoordinates AdjacentBlockCoordinates { get; private set; }

		public BoundingBox[] RaytraceBoundingBoxes => _boundingBoxes.ToArray();
		private List<BoundingBox> _boundingBoxes = new List<BoundingBox>();

		/// <inheritdoc />
		public RaytracerComponent(Entity entity) : base(entity, "RayTracer") { }

		/// <inheritdoc />
		public void OnTick()
		{
			UpdateBlockRayTracer();
		}

		private void UpdateBlockRayTracer()
		{
			var camPos = Entity.Level.Camera.Position;
			var lookVector = Entity.Level.Camera.Direction;

			//List<BoundingBox> boundingBoxes = new List<BoundingBox>();
			// var               ray           = new Ray(camPos, lookVector * 8f);

			BlockState previous = null;
			BlockCoordinates previousPosition = BlockCoordinates.Zero;
			Vector3 previousTargetPoint = Vector3.Zero;

			for (float x = (float)(Entity.Width * Entity.Scale); x < 8f; x += 0.01f)
			{
				Vector3 targetPoint = camPos + (lookVector * x);
				var flooredBlock = Vector3.Floor(targetPoint);

				BlockCoordinates currentPosition = flooredBlock;
				var block = Entity.Level.GetBlockState(currentPosition);

				if (CheckHitBlock(block, targetPoint, currentPosition, out var boundingBoxes))
				{
					_boundingBoxes.Clear();
					_boundingBoxes.AddRange(boundingBoxes);

					ResultingCoordinates = currentPosition;
					TracedBlock = block.Block;

					// _raytraced = targetPoint;
					HasValue = true;

					if (previous != null)
					{
						var flooredAdj = Vector3.Floor(previousTargetPoint);

						var cursorPosition = new Vector3(
							previousTargetPoint.X - flooredAdj.X, previousTargetPoint.Y - flooredAdj.Y,
							previousTargetPoint.Z - flooredAdj.Z);

						var difference = flooredAdj - flooredBlock;
						var adj = new Vector3(difference.X, difference.Y, difference.Z);
						adj.Normalize();

						var blockFace = adj.GetBlockFace();

						if (blockFace == BlockFace.None)
						{
							if (lookVector.Y > 0)
							{
								blockFace = BlockFace.Down;
							}
							else if (lookVector.Y < 0)
							{
								blockFace = BlockFace.Up;
							}
						}

						AdjacentBlockCoordinates = previousPosition;
						Face = blockFace;
						CursorPosition = cursorPosition;
					}
					else
					{
						CursorPosition = Vector3.Zero;
						Face = BlockFace.None;
						AdjacentBlockCoordinates = BlockCoordinates.Zero;
					}

					return;
				}

				previous = block;
				previousPosition = currentPosition;
				previousTargetPoint = targetPoint;
			}

			TracedBlock = null;
			HasValue = false;
			_boundingBoxes.Clear();
		}


		private bool CheckHitBlock(BlockState state,
			Vector3 targetPoint,
			BlockCoordinates blockCoordinates,
			out BoundingBox[] hits)
		{
			hits = new BoundingBox[0];

			if (state != null && state.Block.HasHitbox)
			{
				//boundingBoxes.Clear();

				var boundingBoxes = state.Block.GetBoundingBoxes(blockCoordinates).ToArray();

				foreach (var bbox in boundingBoxes)
				{
					if (bbox.Contains(targetPoint) == ContainmentType.Contains)
					{
						hits = boundingBoxes;

						return true;
					}
				}
			}

			return false;
		}
	}
}