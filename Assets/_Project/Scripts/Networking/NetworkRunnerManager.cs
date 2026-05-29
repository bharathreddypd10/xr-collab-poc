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

using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;

public class NetworkRunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Avatar Spawn")]
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private float spawnSpacing = 2f;
    [SerializeField] private float spawnHeight = 1.2f;
    [SerializeField] private float localSpawnDistance = 1.5f;

    private NetworkRunner runner;
    private readonly Dictionary<PlayerRef, NetworkObject> spawnedAvatars = new Dictionary<PlayerRef, NetworkObject>();
    private readonly HashSet<PlayerRef> spawnInProgress = new HashSet<PlayerRef>();
    private Coroutine localAvatarSpawnRoutine;

    private void OnEnable()
    {
        Debug.Log($"[Fusion] NetworkRunnerManager enabled on {gameObject.name}");
    }

    async void Start()
    {
        var roomCode = PlayerPrefs.GetString("RoomCode", "TestRoom");

        Debug.Log($"[Fusion] Creating Runner | Room={roomCode}");

        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = false;
        runner.AddCallbacks(this);

        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null)
        {
            sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        var objectProvider = gameObject.GetComponent<NetworkObjectProviderDefault>();
        if (objectProvider == null)
        {
            objectProvider = gameObject.AddComponent<NetworkObjectProviderDefault>();
        }

        objectProvider.DelayIfSceneManagerIsBusy = false;

        Debug.Log("[Fusion] Starting Game");

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomCode,
            PlayerCount = 4,
            SceneManager = sceneManager,
            ObjectProvider = objectProvider
        });

        Debug.Log($"[Fusion] StartGame Complete: {result.Ok}");

        if (!result.Ok)
        {
            Debug.LogError($"[Fusion] StartGame failed: {result.ShutdownReason}");
            return;
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Fusion] Player Joined: {player}");

        StartCoroutine(AssignPlayerLabelWhenReady(player));

        if (runner.LocalPlayer != player)
        {
            Debug.Log($"[Fusion] Skipping spawn for remote player: {player}");
            return;
        }

        Debug.Log($"[Fusion] Spawning local avatar for player: {player}");
        SpawnOrAssignLocalAvatar(player, "OnPlayerJoined");
    }

    private async void SpawnOrAssignLocalAvatar(PlayerRef player, string source)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[Fusion] playerPrefab is missing on NetworkRunnerManager.");
            return;
        }

        if (spawnedAvatars.ContainsKey(player))
        {
            Debug.Log($"[Fusion] Avatar already tracked for player {player} ({source}).");
            return;
        }

        if (!spawnInProgress.Add(player))
        {
            Debug.Log($"[Fusion] Avatar spawn already in progress for player {player} ({source}).");
            return;
        }

        try
        {
            Vector3 spawnPosition = GetSpawnPosition(player);
            Debug.Log($"[Fusion] Attempting spawn at {spawnPosition} for player {player} ({source})");

            NetworkSpawnOp spawnOp = runner.SpawnAsync(
                playerPrefab,
                spawnPosition,
                Quaternion.identity,
                player,
                null,
                default,
                null);

            Debug.Log($"[Fusion] SpawnAsync status for player {player} ({source}): {spawnOp.Status}");

            NetworkObject spawnedAvatar = await spawnOp;

            if (spawnedAvatar == null)
            {
                Debug.LogError($"[Fusion] Avatar spawn returned null for player {player} ({source})");
                return;
            }

            spawnedAvatars[player] = spawnedAvatar;

            Debug.Log($"[Fusion] Calling SetPlayerObject for player {player} with {spawnedAvatar.name}");
            runner.SetPlayerObject(player, spawnedAvatar);
            Debug.Log($"[Fusion] SetPlayerObject complete for player {player}. LocalPlayerObject={runner.GetPlayerObject(player)?.name ?? "None"}");

            ApplyAvatarLabel(spawnedAvatar, BuildPlayerLabel(player));
            Debug.Log($"[Fusion] Avatar Spawn SUCCESS: {spawnedAvatar.name} | Player={player} | Label={BuildPlayerLabel(player)}");
        }
        catch (NetworkObjectSpawnException ex)
        {
            Debug.LogError($"[Fusion] Avatar spawn failed for player {player} ({source}): {ex.Status} | {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Fusion] Avatar spawn threw exception for player {player}: {ex}");
        }
        finally
        {
            spawnInProgress.Remove(player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Fusion] Player Left: {player}");

        if (spawnedAvatars.TryGetValue(player, out NetworkObject spawnedAvatar))
        {
            if (spawnedAvatar != null && spawnedAvatar.HasStateAuthority)
            {
                runner.Despawn(spawnedAvatar);
            }

            spawnedAvatars.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"[Fusion] Shutdown: {shutdownReason}");
        spawnedAvatars.Clear();

        if (localAvatarSpawnRoutine != null)
        {
            StopCoroutine(localAvatarSpawnRoutine);
            localAvatarSpawnRoutine = null;
        }
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("[Fusion] Connected To Server");

        if (localAvatarSpawnRoutine != null)
        {
            StopCoroutine(localAvatarSpawnRoutine);
        }

        localAvatarSpawnRoutine = StartCoroutine(EnsureLocalAvatarSpawnedWhenReady());
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

    private IEnumerator AssignPlayerLabelWhenReady(PlayerRef player)
    {
        const float timeoutSeconds = 5f;
        float endTime = Time.realtimeSinceStartup + timeoutSeconds;

        while (Time.realtimeSinceStartup < endTime)
        {
            if (runner != null && runner.TryGetPlayerObject(player, out NetworkObject playerObject) && playerObject != null)
            {
                ApplyAvatarLabel(playerObject, BuildPlayerLabel(player));
                yield break;
            }

            yield return null;
        }

        Debug.LogWarning($"[Fusion] Could not find avatar to label for Player={player}");
    }

    private IEnumerator EnsureLocalAvatarSpawnedWhenReady()
    {
        const float timeoutSeconds = 5f;
        float endTime = Time.realtimeSinceStartup + timeoutSeconds;

        while (Time.realtimeSinceStartup < endTime)
        {
            if (runner != null && runner.IsRunning && runner.LocalPlayer.IsRealPlayer)
            {
                if (runner.GetPlayerObject(runner.LocalPlayer) == null)
                {
                    Debug.Log("[Fusion] Connected fallback: spawning local avatar.");
                    SpawnOrAssignLocalAvatar(runner.LocalPlayer, "Connected fallback");
                }
                else
                {
                    Debug.Log($"[Fusion] Connected fallback: local player object already exists ({runner.GetPlayerObject(runner.LocalPlayer).name}).");
                }

                yield break;
            }

            yield return null;
        }

        Debug.LogWarning("[Fusion] Connected fallback timed out before local player became ready.");
    }

    private static string BuildPlayerLabel(PlayerRef player)
    {
        return $"Player {player.AsIndex + 1}";
    }

    private Vector3 GetSpawnPosition(PlayerRef player)
    {
        Camera mainCamera = Camera.main;
        if (runner != null && mainCamera != null && player == runner.LocalPlayer)
        {
            return mainCamera.transform.position + mainCamera.transform.forward * localSpawnDistance;
        }

        return new Vector3(player.AsIndex * spawnSpacing, spawnHeight, 0f);
    }

    private static void ApplyAvatarLabel(NetworkObject avatar, string label)
    {
        if (avatar == null)
        {
            return;
        }

        TMP_Text labelText = avatar.GetComponentInChildren<TMP_Text>(true);
        if (labelText == null)
        {
            Debug.LogWarning($"[Fusion] Avatar label text not found on {avatar.name}");
            return;
        }

        labelText.text = label;
    }
}
