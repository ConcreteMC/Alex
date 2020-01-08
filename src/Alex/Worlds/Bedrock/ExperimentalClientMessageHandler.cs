using Alex.API.Network.Bedrock;
using MiNET.Net;

namespace Alex.Worlds.Bedrock
{
    public class ExperimentalClientMessageHandler : IMcpeClientMessageHandler
    {
        private IBedrockNetworkProvider Client { get; }
        public ExperimentalClientMessageHandler(IBedrockNetworkProvider networkProvider)
        {
            Client = networkProvider;
        }
        
        public void HandleMcpePlayStatus(McpePlayStatus message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeServerToClientHandshake(McpeServerToClientHandshake message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeDisconnect(McpeDisconnect message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeResourcePacksInfo(McpeResourcePacksInfo message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeResourcePackStack(McpeResourcePackStack message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeText(McpeText message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetTime(McpeSetTime message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeStartGame(McpeStartGame message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeAddPlayer(McpeAddPlayer message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeAddEntity(McpeAddEntity message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeRemoveEntity(McpeRemoveEntity message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeAddItemEntity(McpeAddItemEntity message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeTakeItemEntity(McpeTakeItemEntity message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeMoveEntity(McpeMoveEntity message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeMovePlayer(McpeMovePlayer message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeRiderJump(McpeRiderJump message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeUpdateBlock(McpeUpdateBlock message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeAddPainting(McpeAddPainting message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeTickSync(McpeTickSync message)
        {
            throw new System.NotImplementedException();
        }

        /*public void HandleMcpeExplode(McpeExplode message)
        {
            throw new System.NotImplementedException();
        }*/

        public void HandleMcpeLevelSoundEventOld(McpeLevelSoundEventOld message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeLevelEvent(McpeLevelEvent message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeBlockEvent(McpeBlockEvent message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeEntityEvent(McpeEntityEvent message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeMobEffect(McpeMobEffect message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeUpdateAttributes(McpeUpdateAttributes message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeInventoryTransaction(McpeInventoryTransaction message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeMobEquipment(McpeMobEquipment message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeMobArmorEquipment(McpeMobArmorEquipment message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeInteract(McpeInteract message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeHurtArmor(McpeHurtArmor message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetEntityData(McpeSetEntityData message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetEntityMotion(McpeSetEntityMotion message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetEntityLink(McpeSetEntityLink message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetHealth(McpeSetHealth message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetSpawnPosition(McpeSetSpawnPosition message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeAnimate(McpeAnimate message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeRespawn(McpeRespawn message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeContainerOpen(McpeContainerOpen message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeContainerClose(McpeContainerClose message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpePlayerHotbar(McpePlayerHotbar message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeInventoryContent(McpeInventoryContent message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeInventorySlot(McpeInventorySlot message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeContainerSetData(McpeContainerSetData message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeCraftingData(McpeCraftingData message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeCraftingEvent(McpeCraftingEvent message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeGuiDataPickItem(McpeGuiDataPickItem message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeAdventureSettings(McpeAdventureSettings message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeBlockEntityData(McpeBlockEntityData message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeLevelChunk(McpeLevelChunk message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetCommandsEnabled(McpeSetCommandsEnabled message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetDifficulty(McpeSetDifficulty message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeChangeDimension(McpeChangeDimension message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetPlayerGameType(McpeSetPlayerGameType message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpePlayerList(McpePlayerList message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSimpleEvent(McpeSimpleEvent message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeTelemetryEvent(McpeTelemetryEvent message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSpawnExperienceOrb(McpeSpawnExperienceOrb message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeClientboundMapItemData(McpeClientboundMapItemData message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeMapInfoRequest(McpeMapInfoRequest message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeRequestChunkRadius(McpeRequestChunkRadius message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeChunkRadiusUpdate(McpeChunkRadiusUpdate message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeItemFrameDropItem(McpeItemFrameDropItem message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeGameRulesChanged(McpeGameRulesChanged message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeCamera(McpeCamera message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeBossEvent(McpeBossEvent message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeShowCredits(McpeShowCredits message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeAvailableCommands(McpeAvailableCommands message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeCommandOutput(McpeCommandOutput message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeUpdateTrade(McpeUpdateTrade message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeUpdateEquipment(McpeUpdateEquipment message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeResourcePackDataInfo(McpeResourcePackDataInfo message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeResourcePackChunkData(McpeResourcePackChunkData message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeTransfer(McpeTransfer message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpePlaySound(McpePlaySound message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeStopSound(McpeStopSound message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetTitle(McpeSetTitle message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeAddBehaviorTree(McpeAddBehaviorTree message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeStructureBlockUpdate(McpeStructureBlockUpdate message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeShowStoreOffer(McpeShowStoreOffer message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpePlayerSkin(McpePlayerSkin message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSubClientLogin(McpeSubClientLogin message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeInitiateWebSocketConnection(McpeInitiateWebSocketConnection message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetLastHurtBy(McpeSetLastHurtBy message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeBookEdit(McpeBookEdit message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeNpcRequest(McpeNpcRequest message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeModalFormRequest(McpeModalFormRequest message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeServerSettingsResponse(McpeServerSettingsResponse message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeShowProfile(McpeShowProfile message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetDefaultGameType(McpeSetDefaultGameType message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeRemoveObjective(McpeRemoveObjective message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetDisplayObjective(McpeSetDisplayObjective message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetScore(McpeSetScore message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeLabTable(McpeLabTable message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeUpdateBlockSynced(McpeUpdateBlockSynced message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeMoveEntityDelta(McpeMoveEntityDelta message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSetScoreboardIdentityPacket(McpeSetScoreboardIdentityPacket message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeUpdateSoftEnumPacket(McpeUpdateSoftEnumPacket message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeNetworkStackLatencyPacket(McpeNetworkStackLatencyPacket message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeScriptCustomEventPacket(McpeScriptCustomEventPacket message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeSpawnParticleEffect(McpeSpawnParticleEffect message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeAvailableEntityIdentifiers(McpeAvailableEntityIdentifiers message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeLevelSoundEventV2(McpeLevelSoundEventV2 message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeNetworkChunkPublisherUpdate(McpeNetworkChunkPublisherUpdate message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeBiomeDefinitionList(McpeBiomeDefinitionList message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeLevelSoundEvent(McpeLevelSoundEvent message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeLevelEventGeneric(McpeLevelEventGeneric message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeLecternUpdate(McpeLecternUpdate message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeVideoStreamConnect(McpeVideoStreamConnect message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeClientCacheStatus(MiNET.Net.McpeClientCacheStatus message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeClientCacheStatus(McpeClientCacheStatus message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeOnScreenTextureAnimation(McpeOnScreenTextureAnimation message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeMapCreateLockedCopy(McpeMapCreateLockedCopy message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeStructureTemplateDataExportRequest(McpeStructureTemplateDataExportRequest message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeStructureTemplateDataExportResponse(McpeStructureTemplateDataExportResponse message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeUpdateBlockProperties(McpeUpdateBlockProperties message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeClientCacheBlobStatus(McpeClientCacheBlobStatus message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeClientCacheMissResponse(McpeClientCacheMissResponse message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleMcpeNetworkSettingsPacket(McpeNetworkSettingsPacket message)
        {
            throw new System.NotImplementedException();
        }

        public void HandleFtlCreatePlayer(FtlCreatePlayer message)
        {
            throw new System.NotImplementedException();
        }
    }
}