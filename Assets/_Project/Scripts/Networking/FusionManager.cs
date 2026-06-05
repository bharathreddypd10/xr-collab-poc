using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static FusionManager Instance;

    private List<SessionInfo> activeSessions =
        new List<SessionInfo>();

    [Header("Avatar Prefab")]
    public NetworkPrefabRef playerAvatarPrefab;

    private NetworkRunner runner;

    [SerializeField]
    private Transform[] spawnPoints;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();

            runner.ProvideInput = true;

            runner.AddCallbacks(this);

            var sceneManager =
                gameObject.AddComponent<NetworkSceneManagerDefault>();

            // Optional lobby join for room listing
await runner.JoinSessionLobby(SessionLobby.Shared);
        }
    }

    public async Task<bool> StartSession(string roomName)
{
    if (runner == null)
        return false;

    if (runner.IsRunning)
    {
        await runner.Shutdown();
    }

    var sceneManager =
        GetComponent<NetworkSceneManagerDefault>();

    if (sceneManager == null)
    {
        sceneManager =
            gameObject.AddComponent<NetworkSceneManagerDefault>();
    }

    var result = await runner.StartGame(new StartGameArgs
    {
        GameMode = GameMode.Shared,
        SessionName = roomName,
        Scene = SceneRef.FromIndex(
            SceneManager.GetActiveScene().buildIndex),
        SceneManager = sceneManager
    });

    Debug.Log($"Session Start Result : {result.Ok}");

    return result.Ok;
}

    public bool RoomExists(string roomCode)
    {
        foreach (var session in activeSessions)
        {
            if (session.Name == roomCode)
            {
                return true;
            }
        }

        return false;
    }

    public void OnPlayerJoined(
        NetworkRunner runner,
        PlayerRef player)
    {
        Debug.Log($"Player Joined: {player}");

        if (player == runner.LocalPlayer)
        {
            int index =
                (player.PlayerId - 1) % spawnPoints.Length;

            runner.Spawn(
                playerAvatarPrefab,
                spawnPoints[index].position,
                spawnPoints[index].rotation,
                player
            );

        // Move XR Rig to the same chair
        GameObject xrRig = GameObject.Find("XRPlayerRig");
        Transform cameraOffset =
    xrRig.transform.Find("Camera Offset");

        if (cameraOffset != null)
        {
            cameraOffset.SetPositionAndRotation(
                spawnPoints[index].position,
                spawnPoints[index].rotation
            );
        }

            Debug.Log("Spawned Local Avatar");
        }
    }

    public void OnPlayerLeft(
        NetworkRunner runner,
        PlayerRef player)
    {
        Debug.Log($"Player Left: {player}");
    }

    public void OnSessionListUpdated(
        NetworkRunner runner,
        List<SessionInfo> sessionList)
    {
        activeSessions = sessionList;

        Debug.Log($"Active Rooms: {activeSessions.Count}");

        foreach (var session in activeSessions)
        {
            Debug.Log($"Room: {session.Name}");
        }
    }

    public void OnConnectedToServer(
        NetworkRunner runner)
    {
        Debug.Log("Connected To Server");
    }

    public void OnDisconnectedFromServer(
        NetworkRunner runner,
        NetDisconnectReason reason)
    {
        Debug.Log($"Disconnected: {reason}");
    }

    public void OnShutdown(
        NetworkRunner runner,
        ShutdownReason shutdownReason)
    {
        Debug.Log($"Shutdown: {shutdownReason}");
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
        Debug.Log($"Connect Failed: {reason}");
    }

    public void OnUserSimulationMessage(
        NetworkRunner runner,
        SimulationMessagePtr message)
    {
    }

    public void OnCustomAuthenticationResponse(
        NetworkRunner runner,
        Dictionary<string, object> data)
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

    public void OnSceneLoadDone(
        NetworkRunner runner)
    {
        Debug.Log("Scene Load Done");
    }

    public void OnSceneLoadStart(
        NetworkRunner runner)
    {
        Debug.Log("Scene Load Start");
    }

    public void OnObjectEnterAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }

    public void OnObjectExitAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }

    public void OnInput(
        NetworkRunner runner,
        NetworkInput input)
    {
    }

    public void OnInputMissing(
        NetworkRunner runner,
        PlayerRef player,
        NetworkInput input)
    {
    }

    public void OnHostMigration(
        NetworkRunner runner,
        HostMigrationToken hostMigrationToken)
    {
    }
}