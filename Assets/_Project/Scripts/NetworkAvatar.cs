using Fusion;
using UnityEngine;

public class NetworkAvatar : NetworkBehaviour
{
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

        xrLeftHand = GameObject.Find("Left Hand").transform;
        xrRightHand = GameObject.Find("Right Hand").transform;

        Debug.Log("[Fusion] Local XR references assigned");
    }

    private void Update()
    {
        if (!Object.HasInputAuthority)
            return;

        if (xrHead != null)
        {
            head.position = xrHead.position;
            head.rotation = xrHead.rotation;
        }

        if (xrLeftHand != null)
        {
            leftHand.position = xrLeftHand.position;
            leftHand.rotation = xrLeftHand.rotation;
        }

        if (xrRightHand != null)
        {
            rightHand.position = xrRightHand.position;
            rightHand.rotation = xrRightHand.rotation;
        }
    }
}