using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Network Prefabs")]
    public NetworkPrefabRef xrAvatarPrefab;

    [Header("Spawn Settings")]
    public Transform spawnPoint;

    private NetworkRunner runner;

    private Dictionary<PlayerRef, NetworkObject> spawnedPlayers =
        new Dictionary<PlayerRef, NetworkObject>();

    async void Start()
    {
        runner = gameObject.AddComponent<NetworkRunner>();

        runner.ProvideInput = false;

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "XRMeetingRoom",
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log("Fusion Started");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
{
    Debug.Log($"Player Joined: {player}");

    Vector3 spawnPosition = new Vector3(-2,0,2);

    NetworkObject avatar =
        runner.Spawn(
            xrAvatarPrefab,
            spawnPosition,
            Quaternion.identity,
            player
        );

    if (avatar != null)
    {
        Debug.Log("XRAvatar Spawned Successfully");
        Debug.Log("Spawned Object Name: " + avatar.name);

        spawnedPlayers.Add(player, avatar);
    }
    else
    {
        Debug.LogError("XRAvatar Spawn FAILED");
    }
}

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (spawnedPlayers.TryGetValue(player, out NetworkObject avatar))
        {
            runner.Despawn(avatar);

            spawnedPlayers.Remove(player);
        }
    }

    // =========================
    // REQUIRED CALLBACKS
    // =========================

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(
        NetworkRunner runner,
        NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token)
    {
    }

    public void OnConnectFailed(
        NetworkRunner runner,
        NetAddress remoteAddress,
        NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(
        NetworkRunner runner,
        SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(
        NetworkRunner runner,
        List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(
        NetworkRunner runner,
        Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(
        NetworkRunner runner,
        HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataReceived(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        float progress)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnObjectExitAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }
}