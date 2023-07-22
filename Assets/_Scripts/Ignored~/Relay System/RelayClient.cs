using System;
using UnityEngine;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine.Rendering;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using System.Linq;
using System.Threading.Tasks;

[DisableAutoCreation, UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class RelayClient : SystemBase
{
    public RelayServerData ClientData;
    enum Status { Unknown, Failed, Init, SigningIn, GettingJoinCode, JoiningRelay, Ready }
    
    string joinCode;
  
    Status status;

    Task<JoinAllocation> JoinRelay;
    Task Init, SignIn;

    protected override void OnCreate()
    {      
        
        RequireForUpdate<EnableRelay>();
        status = Status.Unknown;
    }

    public void GetJoinCodeFromHost()
    {
        status = Status.GettingJoinCode;
    }

    public void JoinUsingCode(string _joinCode)
    {
        joinCode = _joinCode; 
        Init = UnityServices.InitializeAsync();
        status = Status.Init;
    }
    
    protected override void OnUpdate()
    {
        switch (status)
        {
            case Status.Init:
                Debug.Log($"RelayClient: {status}");
                if (Init.IsCompleted)
                {
                    if (!AuthenticationService.Instance.IsSignedIn)
                    {
                        SignIn = AuthenticationService.Instance.SignInAnonymouslyAsync();
                        status = Status.SigningIn;
                    }
                }
                return;            
            case Status.SigningIn:
                Debug.Log($"RelayClient: {status}");
                if (SignIn.IsCompleted)
                    status = JoinUsingJoinCode(joinCode, out JoinRelay);
                return;            
            case Status.GettingJoinCode:
                Debug.Log($"RelayClient: {status}");
                var host = World.GetOrCreateSystemManaged<RelayHost>();
                status = JoinUsingJoinCode(host.joinCode, out JoinRelay);
                return;             
            case Status.JoiningRelay:
                Debug.Log($"RelayClient: {status}");
                status = WaitForJoin(JoinRelay, out ClientData);
                return;
            case Status.Failed:
                Debug.Log($"RelayClient: {status}");
                Debug.Log(status);
                status = Status.Unknown;
                return; 
            case Status.Ready: { return; }
            case Status.Unknown: { return; }
        }
    }

    static Status JoinUsingJoinCode(string hostServerJoinCode, out Task<JoinAllocation> joinTask)
    {
        if (hostServerJoinCode == null)
        {
            joinTask = null;
            return Status.GettingJoinCode;
        }

        // Send the join request to the Relay service
        joinTask = RelayService.Instance.JoinAllocationAsync(hostServerJoinCode);
        return Status.JoiningRelay;
    }

    static Status WaitForJoin(Task<JoinAllocation> joinTask, out RelayServerData relayClientData)
    {
        if (!joinTask.IsCompleted)
        {
            relayClientData = default;
            return Status.JoiningRelay;
        }

        if (joinTask.IsFaulted)
        {
            relayClientData = default;
            Debug.LogError("Join Relay request failed");
            Debug.LogException(joinTask.Exception);
            return Status.Failed;
        }

        return BindToRelay(joinTask, out relayClientData);
    }

    static Status BindToRelay(Task<JoinAllocation> joinTask, out RelayServerData relayClientData)
    {        
        var allocation = joinTask.Result;
        
        try
        {           
            relayClientData = GetRelayData(allocation);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            relayClientData = default;
            return Status.Failed;
        }

        return Status.Ready;
    }

    static RelayServerData GetRelayData(JoinAllocation allocation)
    {
        // Select endpoint based on desired connectionType
        var endpoint = allocation.ServerEndpoints.FirstOrDefault(endpoint => endpoint.ConnectionType == "dtls");

        // Prepare the server endpoint using the Relay server IP and port
        var serverEndpoint = NetworkEndpoint.Parse(endpoint.Host, (ushort)endpoint.Port);

        // UTP uses pointers instead of managed arrays for performance reasons, so we use these helper functions to convert them
        var allocationIdBytes = RelayAllocationId.FromByteArray(allocation.AllocationIdBytes);
        var connectionData = RelayConnectionData.FromByteArray(allocation.ConnectionData);
        var hostConnectionData = RelayConnectionData.FromByteArray(allocation.HostConnectionData);
        var key = RelayHMACKey.FromByteArray(allocation.Key);

        // Prepare the Relay server data and compute the nonce values
        // A player joining the host passes its own connectionData as well as the host's
        var relayServerData = new RelayServerData(ref serverEndpoint, 0, ref allocationIdBytes, ref connectionData,
            ref hostConnectionData, ref key, true);

        return relayServerData;
    }
    
    protected override void OnDestroy()
    {
    }
}