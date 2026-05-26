using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[DefaultExecutionOrder(100)]
public class XRAvatarRigFollower : MonoBehaviour
{
    [Header("Rig")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;

    [Header("Avatar")]
    [SerializeField] private Transform head;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    [SerializeField] private float avatarFloorOffset;

    [Header("Spawn")]
    [SerializeField] private bool snapRigToSpawnAnchor = true;
    [SerializeField] private string spawnAnchorNamePrefix = "TeleportAnchor_Table";
    [SerializeField] private float runnerWaitSeconds = 5f;

    private readonly List<Transform> spawnAnchors = new List<Transform>();
    private Coroutine snapRoutine;
    private bool hasSnappedToSpawnAnchor;

    private void Awake()
    {
        ResolveAvatarReferences();
    }

    private void OnEnable()
    {
        ResolveAvatarReferences();
        ResolveRigReferences();

        if (snapRigToSpawnAnchor && !hasSnappedToSpawnAnchor)
        {
            SnapRigToSpawnAnchor(0);
            snapRoutine = StartCoroutine(SnapRigToSpawnAnchorWhenRunnerIsReady());
        }

        FollowRig();
    }

    private void OnDisable()
    {
        if (snapRoutine != null)
        {
            StopCoroutine(snapRoutine);
            snapRoutine = null;
        }
    }

    private void LateUpdate()
    {
        FollowRig();
    }

    private IEnumerator SnapRigToSpawnAnchorWhenRunnerIsReady()
    {
        var endTime = Time.realtimeSinceStartup + runnerWaitSeconds;

        while (Time.realtimeSinceStartup < endTime)
        {
            ResolveRigReferences();

            var runner = FindFirstObjectByType<NetworkRunner>(FindObjectsInactive.Exclude);
            if (runner != null && runner.IsRunning && runner.LocalPlayer.IsRealPlayer)
            {
                SnapRigToSpawnAnchor(runner.LocalPlayer.AsIndex);
                yield break;
            }

            yield return null;
        }

        SnapRigToSpawnAnchor(0);
    }

    private void SnapRigToSpawnAnchor(int playerIndex)
    {
        ResolveRigReferences();
        CollectSpawnAnchors();

        if (xrOrigin == null || cameraTarget == null || spawnAnchors.Count == 0)
        {
            return;
        }

        var anchor = spawnAnchors[Mathf.Abs(playerIndex) % spawnAnchors.Count];
        var anchorForward = Vector3.ProjectOnPlane(anchor.forward, Vector3.up);

        if (anchorForward.sqrMagnitude < 0.001f)
        {
            anchorForward = Vector3.ProjectOnPlane(-anchor.position, Vector3.up);
        }

        if (anchorForward.sqrMagnitude < 0.001f)
        {
            anchorForward = xrOrigin.transform.forward;
        }

        xrOrigin.MatchOriginUpCameraForward(Vector3.up, anchorForward.normalized);

        var targetCameraPosition = anchor.position;
        targetCameraPosition.y = cameraTarget.position.y;
        xrOrigin.MoveCameraToWorldLocation(targetCameraPosition);

        hasSnappedToSpawnAnchor = true;
        FollowRig();
    }

    private void FollowRig()
    {
        ResolveRigReferences();

        if (cameraTarget == null)
        {
            return;
        }

        var avatarPosition = cameraTarget.position;
        avatarPosition.y = xrOrigin != null
            ? xrOrigin.transform.position.y + avatarFloorOffset
            : transform.position.y;

        var avatarForward = Vector3.ProjectOnPlane(cameraTarget.forward, Vector3.up);
        if (avatarForward.sqrMagnitude < 0.001f)
        {
            avatarForward = transform.forward;
        }

        transform.SetPositionAndRotation(
            avatarPosition,
            Quaternion.LookRotation(avatarForward.normalized, Vector3.up));

        FollowPart(head, cameraTarget);
        FollowPart(leftHand, leftHandTarget);
        FollowPart(rightHand, rightHandTarget);
    }

    private void FollowPart(Transform avatarPart, Transform target)
    {
        if (avatarPart == null || target == null)
        {
            return;
        }

        avatarPart.SetPositionAndRotation(target.position, target.rotation);
    }

    private void ResolveAvatarReferences()
    {
        if (head == null)
        {
            head = FindChildByExactName(transform, "Head");
        }

        if (leftHand == null)
        {
            leftHand = FindChildByExactName(transform, "LeftHand");
        }

        if (rightHand == null)
        {
            rightHand = FindChildByExactName(transform, "RightHand");
        }
    }

    private void ResolveRigReferences()
    {
        if (xrOrigin == null || !xrOrigin.isActiveAndEnabled)
        {
            xrOrigin = FindFirstObjectByType<XROrigin>(FindObjectsInactive.Exclude);
        }

        if (xrOrigin == null)
        {
            return;
        }

        if (cameraTarget == null && xrOrigin.Camera != null)
        {
            cameraTarget = xrOrigin.Camera.transform;
        }

        var rigRoot = xrOrigin.transform;

        if (leftHandTarget == null)
        {
            leftHandTarget = FindChildByKeywords(rigRoot, "left", "controller")
                ?? FindChildByKeywords(rigRoot, "left", "hand");
        }

        if (rightHandTarget == null)
        {
            rightHandTarget = FindChildByKeywords(rigRoot, "right", "controller")
                ?? FindChildByKeywords(rigRoot, "right", "hand");
        }
    }

    private void CollectSpawnAnchors()
    {
        spawnAnchors.Clear();

        var teleportAnchors = FindObjectsByType<TeleportationAnchor>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        foreach (var teleportAnchor in teleportAnchors)
        {
            if (!teleportAnchor.name.StartsWith(spawnAnchorNamePrefix))
            {
                continue;
            }

            spawnAnchors.Add(teleportAnchor.teleportAnchorTransform != null
                ? teleportAnchor.teleportAnchorTransform
                : teleportAnchor.transform);
        }

        spawnAnchors.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
    }

    private static Transform FindChildByExactName(Transform root, string childName)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private static Transform FindChildByKeywords(Transform root, string firstKeyword, string secondKeyword)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            var lowerName = child.name.ToLowerInvariant();
            if (lowerName.Contains(firstKeyword) && lowerName.Contains(secondKeyword))
            {
                return child;
            }
        }

        return null;
    }
}
