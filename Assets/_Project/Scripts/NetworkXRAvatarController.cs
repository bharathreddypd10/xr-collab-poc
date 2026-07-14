using Fusion;
using UnityEngine;

public class NetworkXRAvatarController : NetworkBehaviour
{
    private NetworkTransform networkTransform;

    private void Awake()
    {
        networkTransform = GetComponent<NetworkTransform>();
    }

    public override void Spawned()
    {
        if (!HasInputAuthority)
        {
            // Disable remote player cameras/audio listeners so they don't override yours
            var remoteCamera = GetComponentInChildren<Camera>();
            if (remoteCamera != null) remoteCamera.enabled = false;
            
            var remoteAudio = GetComponentInChildren<AudioListener>();
            if (remoteAudio != null) remoteAudio.enabled = false;
        }
    }

    /// <summary>
    /// Call this function from your XR Teleport Anchor event or seat-snapping logic
    /// </summary>
    public void TeleportNetworkedPlayer(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (!HasInputAuthority) return;

        // Teleport suspends interpolation for this tick on all peers instead of blending to the new position
        if (networkTransform != null)
        {
            networkTransform.Teleport(targetPosition, targetRotation);
        }
        else
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }

        Debug.Log($"[Shared Mode] Successfully bypassed network physics. Moved to: {targetPosition}");
    }
}