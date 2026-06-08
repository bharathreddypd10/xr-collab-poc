using Fusion;
using UnityEngine;

public class NetworkAvatar : NetworkBehaviour
{
    [Header("Avatar Parts")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private Transform xrHead;
    private Transform xrLeftController;
    private Transform xrRightController;

    public override void Spawned()
    {
        if (!Object.HasInputAuthority)
            return;

        xrHead = Camera.main.transform;

        // Your hierarchy:
        // Main Camera
        // ├ Left Hand
        // └ Right Hand

        Transform leftHandParent =
            xrHead.Find("Left Hand");

        Transform rightHandParent =
            xrHead.Find("Right Hand");

        if (leftHandParent != null)
            xrLeftController =
                leftHandParent.Find("XR Controller Left");

        if (rightHandParent != null)
            xrRightController =
                rightHandParent.Find("XR Controller Right");

        Debug.Log($"XR Head : {xrHead}");
        Debug.Log($"XR Left Controller: {xrLeftController}");
        Debug.Log($"XR Right Controller: {xrRightController}");
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
            return;

        if (xrHead == null ||
            xrLeftController == null ||
            xrRightController == null)
            return;

        // Move avatar root with XR camera
        // transform.position = xrHead.position;
        // transform.rotation = Quaternion.Euler(
        //     0,
        //     xrHead.eulerAngles.y,
        //     0);

        // Head
        head.position = xrHead.position;
        head.rotation = xrHead.rotation;

        // Left Hand
        leftHand.position = xrLeftController.position;
        leftHand.rotation = xrLeftController.rotation;

        // Right Hand
        rightHand.position = xrRightController.position;
        rightHand.rotation = xrRightController.rotation;

        // Debug.Log($"Avatar Root: {transform.position}");
    }
}