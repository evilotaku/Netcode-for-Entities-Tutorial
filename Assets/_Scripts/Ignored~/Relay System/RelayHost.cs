using System;
using UnityEngine;
using Unity.Entities;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using System.Net;
using System.Linq;
using Unity.Networking.Transport;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Relay.Models;

[DisableAutoCreation, UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class RelayHost : SystemBase
{   
    const int MaxConnections = 100;
    public string joinCode;
    public RelayServerData RelayData;
       
    enum HostStatus
    { Unknown, InitializeServices, Initializing, SigningIn, FailedToHost, Ready, Allocating, GettingJoinCode, GettingRelayData, }

    HostStatus status = HostStatus.Unknown;

    
    Task<Allocation> Allocate;
    Task<string> JoinCodeTask;
    Task Init, SignIn;

    protected override void OnCreate()
    {
        RequireForUpdate<EnableRelay>();
        status = HostStatus.InitializeServices;
    }
    
    protected override void OnUpdate()
    {
        switch (status)
        {
            case HostStatus.InitializeServices:
                Debug.Log($"RelayHost: {status}");
                Init = UnityServices.InitializeAsync();
                status = HostStatus.Initializing;
                return; 
            case HostStatus.Initializing:
                Debug.Log($"RelayHost: {status}");
                status = WaitForInitialization(Init, out SignIn);
                return;
            case HostStatus.SigningIn:
                Debug.Log($"RelayHost: {status}");
                status = WaitForSignIn(SignIn, out Allocate);
                return;            
            case HostStatus.Allocating:
                Debug.Log($"RelayHost: {status}");
                status = WaitForAllocations(Allocate, out JoinCodeTask);
                return;
            case HostStatus.GettingJoinCode:
                Debug.Log($"RelayHost: {status}");
                status = WaitForJoin(JoinCodeTask, out joinCode);
                return; 
            case HostStatus.GettingRelayData:
                Debug.Log($"RelayHost: {status}");
                status = BindToHost(Allocate, out RelayData);
                return;
            case HostStatus.FailedToHost:
                Debug.Log($"RelayHost: {status}");
                status = HostStatus.Unknown;
                return;
            case HostStatus.Ready:
                Debug.Log($"RelayHost: {status}");
                return;
            case HostStatus.Unknown:
            default:
                return;
        }
    }

    static HostStatus WaitForInitialization(Task initializeTask, out Task nextTask)
    {
        if (!initializeTask.IsCompleted)
        {
            nextTask = default;
            return HostStatus.Initializing;
        }

        if (initializeTask.IsFaulted)
        {
            Debug.LogError("UnityServices Initialization failed");
            Debug.LogException(initializeTask.Exception);
            nextTask = default;
            return HostStatus.FailedToHost;
        }

        if (AuthenticationService.Instance.IsSignedIn)
        {
            nextTask = Task.CompletedTask;
            return HostStatus.SigningIn;
        }
        else
        {
            nextTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
            return HostStatus.SigningIn;
        }
    }

    static HostStatus WaitForSignIn(Task signInTask, out Task<Allocation> allocationTask)
    {
        if (!signInTask.IsCompleted)
        {
            allocationTask = default;
            return HostStatus.SigningIn;
        }

        if (signInTask.IsFaulted)
        {
            Debug.LogError("Signing in failed");
            Debug.LogException(signInTask.Exception);
            allocationTask = default;
            return HostStatus.FailedToHost;
        }

        // Request list of valid regions
        allocationTask = RelayService.Instance.CreateAllocationAsync(MaxConnections);
        return HostStatus.Allocating;
    }  

    static HostStatus WaitForAllocations(Task<Allocation> allocationTask, out Task<string> joinCodeTask)
    {
        if (!allocationTask.IsCompleted)
        {
            joinCodeTask = null;
            return HostStatus.Allocating;
        }

        if (allocationTask.IsFaulted)
        {
            Debug.LogError("Create allocation request failed");
            Debug.LogException(allocationTask.Exception);
            joinCodeTask = null;
            return HostStatus.FailedToHost;
        }

        // Request the join code to the Relay service
        var allocation = allocationTask.Result;
        Debug.Log($"Allocation Acquiried...");
        joinCodeTask = RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        return HostStatus.GettingJoinCode;
    }

    static HostStatus WaitForJoin(Task<string> joinCodeTask, out string joinCode)
    {
        joinCode = null;
        if (!joinCodeTask.IsCompleted)
        {
            return HostStatus.GettingJoinCode;
        }

        if (joinCodeTask.IsFaulted)
        {
            Debug.LogError("Create join code request failed");
            Debug.LogException(joinCodeTask.Exception);
            return HostStatus.FailedToHost;
        }

        joinCode = joinCodeTask.Result;
        Debug.Log($"Relay Join Code: {joinCode}");
        return HostStatus.GettingRelayData;
    }

    static HostStatus BindToHost(Task<Allocation> allocationTask, out RelayServerData relayServerData)
    {
        var allocation = allocationTask.Result;
        try
        {
            // Format the server data, based on desired connectionType
            relayServerData = HostRelayData(allocation);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            relayServerData = default;
            return HostStatus.FailedToHost;
        }
        return HostStatus.Ready;
    }

    static RelayServerData HostRelayData(Allocation allocation, string connectionType = "dtls")
    {
        // Select endpoint based on desired connectionType
        var endpoint = allocation.ServerEndpoints.FirstOrDefault(endpoint => endpoint.ConnectionType == connectionType);       

        // Prepare the server endpoint using the Relay server IP and port
        var serverEndpoint = NetworkEndpoint.Parse(endpoint.Host, (ushort)endpoint.Port);

        // UTP uses pointers instead of managed arrays for performance reasons, so we use these helper functions to convert them
        var allocationIdBytes = RelayAllocationId.FromByteArray(allocation.AllocationIdBytes);
        var connectionData = RelayConnectionData.FromByteArray(allocation.ConnectionData);
        var key = RelayHMACKey.FromByteArray(allocation.Key);

        // Prepare the Relay server data and compute the nonce value
        // The host passes its connectionData twice into this function
        var relayServerData = new RelayServerData(ref serverEndpoint, 0, ref allocationIdBytes, ref connectionData,
            ref connectionData, ref key, true);
        Debug.Log($"Relay Server: {serverEndpoint} in {allocation.Region}");
        return relayServerData;
    }

    protected override void OnDestroy()
    {
    }
}