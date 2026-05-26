// using Fusion;
// using Fusion.Sockets;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using System;
// using System.Collections.Generic;

// public class NetworkRunnerManager : MonoBehaviour, INetworkRunnerCallbacks
// {
//     // Assign PlayerCapsule prefab in Inspector
//     // Prefab MUST contain:
//     // - NetworkObject
//     // - NetworkTransform

//     public NetworkObject playerPrefab;

//     private NetworkRunner runner;

//     async void Start()
//     {
//         Debug.Log("[Fusion] Creating Runner");

//         runner = gameObject.AddComponent<NetworkRunner>();

//         runner.ProvideInput = true;

//         var sceneManager =
//             gameObject.AddComponent<NetworkSceneManagerDefault>();

//         runner.AddCallbacks(this);

//         Debug.Log("[Fusion] Starting Game");

//         await runner.StartGame(new StartGameArgs()
//         {
//             GameMode = GameMode.Shared,
//             SessionName = "TestRoom",

//             Scene = SceneRef.FromIndex(
//                 SceneManager.GetActiveScene().buildIndex
//             ),

//             SceneManager = sceneManager,

//             PlayerCount = 4
//         });

//         Debug.Log("[Fusion] StartGame Complete");
//     }

//     public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
//     {
//         Debug.Log($"[Fusion] Player Joined: {player}");

//         // In Shared Mode:
//         // each player spawns THEIR OWN object
//         if (runner.LocalPlayer == player)
//         {
//             Vector3 spawnPosition =
//                 new Vector3(
//                     player.RawEncoded * 3,
//                     1,
//                     0
//                 );

//             Debug.Log($"[Fusion] Spawning at {spawnPosition}");

//             var spawnedObject = runner.Spawn(
//                 playerPrefab,
//                 spawnPosition,
//                 Quaternion.identity,
//                 player
//             );

//             if (spawnedObject != null)
//             {
//                 Debug.Log(
//                     $"[Fusion] Spawn SUCCESS: {spawnedObject.name}"
//                 );
//             }
//             else
//             {
//                 Debug.LogError("[Fusion] Spawn FAILED");
//             }
//         }
//     }

//     public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
//     {
//         Debug.Log($"[Fusion] Player Left: {player}");
//     }

//     public void OnInput(NetworkRunner runner, NetworkInput input)
//     {
//         Vector2 move = Vector2.zero;

//         if (Input.GetKey(KeyCode.W) ||
//             Input.GetKey(KeyCode.UpArrow))
//         {
//             move.y += 1f;
//         }

//         if (Input.GetKey(KeyCode.S) ||
//             Input.GetKey(KeyCode.DownArrow))
//         {
//             move.y -= 1f;
//         }

//         if (Input.GetKey(KeyCode.A) ||
//             Input.GetKey(KeyCode.LeftArrow))
//         {
//             move.x -= 1f;
//         }

//         if (Input.GetKey(KeyCode.D) ||
//             Input.GetKey(KeyCode.RightArrow))
//         {
//             move.x += 1f;
//         }

//         input.Set(new NetworkInputData
//         {
//             Move = move.normalized
//         });
//     }

//     public void OnInputMissing(
//         NetworkRunner runner,
//         PlayerRef player,
//         NetworkInput input)
//     {
//     }

//     public void OnShutdown(
//         NetworkRunner runner,
//         ShutdownReason shutdownReason)
//     {
//     }

//     public void OnConnectedToServer(NetworkRunner runner)
//     {
//     }

//     public void OnDisconnectedFromServer(
//         NetworkRunner runner,
//         NetDisconnectReason reason)
//     {
//     }

//     public void OnConnectRequest(
//         NetworkRunner runner,
//         NetworkRunnerCallbackArgs.ConnectRequest request,
//         byte[] token)
//     {
//     }

//     public void OnConnectFailed(
//         NetworkRunner runner,
//         NetAddress remoteAddress,
//         NetConnectFailedReason reason)
//     {
//     }

//     public void OnUserSimulationMessage(
//         NetworkRunner runner,
//         SimulationMessagePtr message)
//     {
//     }

//     public void OnSessionListUpdated(
//         NetworkRunner runner,
//         List<SessionInfo> sessionList)
//     {
//     }

//     public void OnCustomAuthenticationResponse(
//         NetworkRunner runner,
//         Dictionary<string, object> data)
//     {
//     }

//     public void OnHostMigration(
//         NetworkRunner runner,
//         HostMigrationToken hostMigrationToken)
//     {
//     }

//     public void OnReliableDataReceived(
//         NetworkRunner runner,
//         PlayerRef player,
//         ReliableKey key,
//         ArraySegment<byte> data)
//     {
//     }

//     public void OnReliableDataProgress(
//         NetworkRunner runner,
//         PlayerRef player,
//         ReliableKey key,
//         float progress)
//     {
//     }

//     public void OnSceneLoadDone(NetworkRunner runner)
//     {
//         Debug.Log("[Fusion] Scene Load Done");
//     }

//     public void OnSceneLoadStart(NetworkRunner runner)
//     {
//         Debug.Log("[Fusion] Scene Load Start");
//     }

//     public void OnObjectExitAOI(
//         NetworkRunner runner,
//         NetworkObject obj,
//         PlayerRef player)
//     {
//     }

//     public void OnObjectEnterAOI(
//         NetworkRunner runner,
//         NetworkObject obj,
//         PlayerRef player)
//     {
//     }
// }

using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System;
using System.Collections.Generic;

public class NetworkRunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner runner;

    async void Start()
    {
        Debug.Log("[Fusion] Creating Runner");

        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = false;
        runner.AddCallbacks(this);

        Debug.Log("[Fusion] Starting Game");

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = PlayerPrefs.GetString("RoomCode", "TestRoom"),
            PlayerCount = 4
        });

        Debug.Log($"[Fusion] StartGame Complete: {result.Ok}");

        if (!result.Ok)
        {
            Debug.LogError($"[Fusion] StartGame failed: {result.ShutdownReason}");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Fusion] Player Joined: {player}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Fusion] Player Left: {player}");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"[Fusion] Shutdown: {shutdownReason}");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("[Fusion] Connected To Server");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"[Fusion] Disconnected: {reason}");
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
        Debug.LogError($"[Fusion] Connect Failed: {reason}");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

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
        Debug.Log("[Fusion] Scene Load Done");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("[Fusion] Scene Load Start");
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}