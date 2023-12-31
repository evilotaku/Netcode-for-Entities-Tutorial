using UnityEngine;
using Unity.NetCode;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;

public struct GoInGameRequest : IRpcCommand { }

[BurstCompile, WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct GoInGameClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>()
            .WithNone<NetworkStreamInGame>();
        state.RequireForUpdate<PlayerPrefab>();
        state.RequireForUpdate(state.GetEntityQuery(builder));

    }

    [BurstCompile]   
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach ((RefRO<NetworkId> id, Entity entity) in SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess().WithNone<NetworkStreamInGame>())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(entity);
            var req = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<GoInGameRequest>(req);
            commandBuffer.AddComponent(req, new SendRpcCommandRequest { TargetConnection = entity });
        }
        commandBuffer.Playback(state.EntityManager);
    }
}

[BurstCompile, WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem
{
    private ComponentLookup<NetworkId> networkIdFromEntity;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRequest>()
            .WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate<PlayerPrefab>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
        networkIdFromEntity = state.GetComponentLookup<NetworkId>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var prefab = SystemAPI.GetSingleton<PlayerPrefab>();
        state.EntityManager.GetName(prefab.Player, out FixedString64Bytes prefabName);
        FixedString128Bytes worldName = state.WorldUnmanaged.Name;

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        networkIdFromEntity.Update(ref state);

        foreach ((RefRO<ReceiveRpcCommandRequest> reqSrc, Entity reqEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequest>().WithEntityAccess())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(reqSrc.ValueRO.SourceConnection);
            var networkId = networkIdFromEntity[reqSrc.ValueRO.SourceConnection];

            Debug.Log($"'{worldName}' setting connection '{networkId.Value}' to in game, spawning a Ghost '{prefabName}' for them!");

            Entity player = commandBuffer.Instantiate(prefab.Player);
            Entity character = commandBuffer.Instantiate(prefab.Character);
            Entity camera = commandBuffer.Instantiate(prefab.Camera);

            commandBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });            
            commandBuffer.SetComponent(character, new GhostOwner { NetworkId = networkId.Value });
            commandBuffer.SetComponent(camera, new GhostOwner { NetworkId = networkId.Value });
            commandBuffer.SetComponent(player, new ThirdPersonPlayer { ControlledCharacter = character, ControlledCamera = camera });
            commandBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });
            commandBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = character });
            commandBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = camera });

            commandBuffer.DestroyEntity(reqEntity);
        }
        commandBuffer.Playback(state.EntityManager);
    }
}