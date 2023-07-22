using System;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class ClientChatSystem : SystemBase
{

    BeginSimulationEntityCommandBufferSystem CommandBufferSystem;
    
    protected override void OnCreate()
    {       
        RequireForUpdate<NetworkId>();
        CommandBufferSystem = World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        if (World.IsThinClient())
        {
            EntityManager.DestroyEntity(GetEntityQuery(new EntityQueryDesc()
            {
                All = new[] { ComponentType.ReadOnly<ReceiveRpcCommandRequest>() },
                Any = new[] { ComponentType.ReadOnly<ChatMessage>(), ComponentType.ReadOnly<ChatUser>() }
            }));
        }
                
        var buffer = CommandBufferSystem.CreateCommandBuffer();
        var connectionArray = GetComponentLookup<NetworkId>(true);
        FixedString32Bytes worldName = World.Name;

        Entities.WithName("RegisterUser").WithReadOnly(connectionArray).ForEach(
            (Entity entity, ref ReceiveRpcCommandRequest rpcCmd, ref ChatUser user) =>
            {
                var conId = connectionArray[rpcCmd.SourceConnection].Value;
                Debug.Log(
                    $"[{worldName}] Received {user.UserData} from connection {conId}");
                buffer.DestroyEntity(entity);
                ChatBox.Users.Enqueue(user.UserData);
            }).Run();

        Entities.WithName("ReceiveChatMessage").ForEach(
        (Entity entity, ref ReceiveRpcCommandRequest rpcCmd, ref ChatMessage chat) =>
        {
            buffer.DestroyEntity(entity);            
            ChatBox.Messages.Enqueue(chat.Message);
        }).Run();

        CommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
    
    protected override void OnDestroy()
    {
    }
}