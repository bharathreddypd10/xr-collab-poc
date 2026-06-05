using Fusion;
using UnityEngine;

public class NetworkAvatar : NetworkBehaviour
{
    [Header("Avatar Parts")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private Transform xrHead;
    private Transform xrLeftHand;
    private Transform xrRightHand;

    public override void Spawned()
    {
        if (!Object.HasInputAuthority)
            return;

        xrHead = Camera.main.transform;

        // Your hierarchy:
        // Main Camera
        // ├ Left Hand
        // └ Right Hand

        xrLeftHand = xrHead.Find("Left Hand");
        xrRightHand = xrHead.Find("Right Hand");

        Debug.Log($"XR Head : {xrHead}");
        Debug.Log($"XR Left : {xrLeftHand}");
        Debug.Log($"XR Right: {xrRightHand}");
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
            return;

        if (xrHead == null ||
            xrLeftHand == null ||
            xrRightHand == null)
            return;

        // Move avatar root with XR camera
        transform.position = xrHead.position;
        transform.rotation = Quaternion.Euler(
            0,
            xrHead.eulerAngles.y,
            0);

        // Head
        head.position = xrHead.position;
        head.rotation = xrHead.rotation;

        // Left Hand
        leftHand.position = xrLeftHand.position;
        leftHand.rotation = xrLeftHand.rotation;

        // Right Hand
        rightHand.position = xrRightHand.position;
        rightHand.rotation = xrRightHand.rotation;

        // Debug.Log($"Avatar Root: {transform.position}");
    }
}