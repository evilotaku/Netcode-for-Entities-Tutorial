using System;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class ServerChatSystem : SystemBase
{

    private BeginSimulationEntityCommandBufferSystem m_CommandBufferSystem;

    
    private NativeList<int> m_Users;

    protected override void OnCreate()
    {      
        m_CommandBufferSystem = World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        m_Users = new NativeList<int>(Allocator.Persistent);
    }
       
    protected override void OnUpdate()
    {
        var buffer = m_CommandBufferSystem.CreateCommandBuffer();
        var connectionArray = GetComponentLookup<NetworkId>(true);
        FixedString32Bytes worldName = World.Name;
                
        Entities.WithName("ReceiveChatMessage").WithReadOnly(connectionArray).ForEach(
            (Entity entity, ref ReceiveRpcCommandRequest rpcCmd, ref ChatMessage chat) =>
            {
                var conId = connectionArray[rpcCmd.SourceConnection].Value;
                Debug.Log(
                    $"[{worldName}] Received {chat.Message} on connection {conId}.");
                buffer.DestroyEntity(entity);
                var broadcastEntity = buffer.CreateEntity();
                buffer.AddComponent(broadcastEntity, new ChatMessage() { Message = FixedString.Format("User {0}: {1}", conId, chat.Message) });
                buffer.AddComponent<SendRpcCommandRequest>(broadcastEntity);
            }).Run();
        m_CommandBufferSystem.AddJobHandleForProducer(Dependency);

        var users = m_Users;
        Entities.WithName("AddNewUser").WithNone<ChatUserInitialized>().ForEach(
            (Entity entity, ref NetworkId id) =>
            {
                var connectionId = id.Value;

                // Notify all connections about new chat user (including himself)
                var broadcastEntity = buffer.CreateEntity();
                buffer.AddComponent(broadcastEntity, new ChatUser() { UserData = connectionId });
                buffer.AddComponent<SendRpcCommandRequest>(broadcastEntity);
                Debug.Log($"[{worldName}] New user 'User {connectionId}' connected. Broadcasting user entry to all connections;");
                                
                for (int i = 0; i < users.Length; ++i)
                {
                    var existingUser = buffer.CreateEntity();
                    var user = users[i];
                    buffer.AddComponent(existingUser, new ChatUser() { UserData = user });
                    buffer.AddComponent<SendRpcCommandRequest>(existingUser);
                    buffer.SetComponent(existingUser, new SendRpcCommandRequest { TargetConnection = entity });
                    Debug.Log($"[{worldName}] Sending user 'User {user}' to new connection {connectionId}");
                }

                // Add connection to user list
                users.Add(connectionId);

                // Mark this connection/user so we don't process again
                buffer.AddComponent<ChatUserInitialized>(entity);
            }).Run();
    }

    protected override void OnDestroy()
    {
        m_Users.Dispose();
    }
}