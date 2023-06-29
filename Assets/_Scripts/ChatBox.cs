using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;
using JetBrains.Annotations;
using Unity.Services.Relay.Models;

public class ChatBox : MonoBehaviour
{
    public static readonly NativeQueue<FixedString128Bytes> Messages = new(Allocator.Persistent);
    public static readonly NativeQueue<int> Users= new(Allocator.Persistent);

    public TMP_InputField ChatInput;
    public TMP_Text ChatPrefab, UserPrefab;
    public RectTransform UserList, ChatWindow;
    public ScrollRect ChatWindowRect;

    List<World> clientWorlds = new();
    
    int OwnUser = -1;


    // Start is called before the first frame update
    void Start()
    {
        foreach (var world  in World.All) 
        {
            if(world.IsClient() && !world.IsThinClient())
            {
                clientWorlds.Add(world);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(OwnUser == -1)
        {
            //wait until we have a connection
            var query = clientWorlds[0].EntityManager.CreateEntityQuery(ComponentType.ReadOnly<NetworkId>());
            var connectionArray = query.ToComponentDataArray<NetworkId>(Allocator.Temp);
            if(connectionArray.Length > 0 ) 
            {
                //clients only have 1 connection
                OwnUser = connectionArray[0].Value;
            }
        }

        if(Users.TryDequeue(out var user))
        {
            var userFrame = Instantiate(UserPrefab, UserList);
            userFrame.text = $"User {user}";
            
            if (user == OwnUser) userFrame.color = Color.blue;
        }

        if(Messages.TryDequeue(out var message)) 
        {
            var chatmsg = Instantiate(ChatPrefab, ChatWindow);
            if (message.ConvertToString().StartsWith($"User {OwnUser}"))
            {
                chatmsg.color = Color.blue;               
            }
            chatmsg.text = $"{message}\n";

            ChatWindowRect.verticalNormalizedPosition = 0f;
        }   
    }

    public void SendChatMessage(string msg)
    {
        if(clientWorlds.Count > 0) 
        {
            SendRpc(clientWorlds[0], msg);
        }

        ChatInput.text = string.Empty;
        ChatInput.Select();
        ChatInput.ActivateInputField();
    }

    void SendRpc(World world, string msg, Entity targetEntity = default)
    {
        if(world == null || !world.IsCreated) return;

        var entity = world.EntityManager.CreateEntity(typeof(SendRpcCommandRequest), typeof(ChatMessage));
        world.EntityManager.SetComponentData(entity, new ChatMessage() { Message = msg });
       
        if(targetEntity != Entity.Null) 
        {
            world.EntityManager.SetComponentData(entity,
                    new SendRpcCommandRequest() { TargetConnection = targetEntity });
        }
    }
}
