using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;


public struct ChatMessage : IRpcCommand
{
    public FixedString128Bytes Message;
}

public struct ChatUser : IRpcCommand
{
    public int UserData;
}

public struct ChatUserInitialized : IComponentData { }
