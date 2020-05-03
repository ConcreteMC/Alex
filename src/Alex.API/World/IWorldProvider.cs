using System;
using System.Threading.Tasks;
using Alex.API.Blocks.State;
using Alex.API.Data;
using Alex.API.Entities;
using Alex.API.Gui.Elements;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.World
{
	/*public interface IWorldReceiver
	{
		IEntity GetPlayerEntity();

		IChunkColumn GetChunkColumn(int x, int z);
		
		void SpawnEntity(long entityId, IEntity entity);
		void DespawnEntity(long entityId);

		void UpdatePlayerPosition(PlayerLocation location);
		void UpdateEntityPosition(long entityId, PlayerLocation position, bool relative = false, bool updateLook = false, bool updatePitch = false);
		void UpdateEntityLook(long entityId, float yaw, float pitch, bool onGround);
        bool TryGetEntity(long entityId, out IEntity entity);
		
		void SetTime(long worldTime);
		void SetRain(bool raining);

		void SetBlockState(BlockCoordinates coordinates, IBlockState blockState);
		void SetBlockState(BlockCoordinates coordinates, IBlockState blockState, int storage);
		
		void AddPlayerListItem(PlayerListItem item);
		void RemovePlayerListItem(UUID item);
	};*/
}
