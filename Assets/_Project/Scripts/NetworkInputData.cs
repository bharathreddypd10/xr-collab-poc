using Fusion;
using UnityEngine;

// Renamed completely to ensure zero duplication errors with Fusion templates
public struct XRMeetingInputData : INetworkInput
{
    public Vector2 Move;
    public Vector3 TeleportPosition;
    public Quaternion TeleportRotation;
    public NetworkBool DoTeleport; 
}