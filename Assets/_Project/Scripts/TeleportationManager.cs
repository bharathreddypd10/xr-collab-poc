using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using Unity.XR.CoreUtils;

/// <summary>
/// Activates the built-in XRI Teleport Interactor (ProjectileCurve arc) on I key.
/// The XRInteractorLineVisual handles red/cyan colour and the arc shape automatically.
/// LateUpdate forces the interactor to always face camera-forward so the arc is
/// visible as a curve in the XR Device Simulator.
/// </summary>
[RequireComponent(typeof(XROrigin))]
public class TeleportationManager : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public XRRayInteractor teleportInteractor;
    public Transform       tableCenter;

    [Header("Keys")]
    public KeyCode debugKey = KeyCode.T;
    public KeyCode arcKey   = KeyCode.I;

    [Header("Arc Aim")]
    [Tooltip("Lower values make the teleport arc ease toward your look direction more slowly, easier to land on seats. Higher values snap faster.")]
    public float arcAimSmoothSpeed = 6f;

    private TeleportationProvider _provider;
    private int                   _debugIndex;
    private bool                  _arcActive;
    private Material              _lineMat;
    private GameObject            _reticle;

    // Controller tip glow
    private GameObject _controllerGlow;
    private Material   _glowMat;
    private float      _glowPulse;

    // ─────────────────────────────────────────────────────────────

    private void Start()
    {
        _provider = GetComponent<TeleportationProvider>();

        // Wire all anchors to our provider and face them toward the table
        var anchors = FindObjectsByType<TeleportationAnchor>(FindObjectsSortMode.None);
        foreach (var a in anchors)
        {
            a.teleportationProvider = _provider;
            FaceTowardTable(a.transform);
        }
        Debug.Log($"[TeleportationManager] Ready — {anchors.Length} anchors wired.");

        BuildReticle();
        BuildControllerGlow();

        if (teleportInteractor != null)
            teleportInteractor.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(debugKey)) DebugCycleTeleport();
        if (Input.GetKeyDown(arcKey))   BeginArc();
        if (Input.GetKeyUp(arcKey))     EndArc();
        if (_arcActive)
        {
            UpdateReticle();
            UpdateArcColor();
            UpdateControllerGlow();
        }
    }

    // After XRI's own Update — override the interactor direction so the
    // ProjectileCurve always shoots camera-forward at 35° down.
    // Without this, the simulator controller points straight down → straight line.
    private void LateUpdate()
    {
        if (!_arcActive || teleportInteractor == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 horiz = cam.transform.forward;
        horiz.y = 0f;
        if (horiz.sqrMagnitude < 0.01f) horiz = Vector3.forward;
        horiz.Normalize();

        float   rad = 35f * Mathf.Deg2Rad;
        Vector3 dir = horiz * Mathf.Cos(rad) + Vector3.down * Mathf.Sin(rad);
        Quaternion targetRot = Quaternion.LookRotation(dir);
        teleportInteractor.transform.rotation = Quaternion.Slerp(
            teleportInteractor.transform.rotation, targetRot, arcAimSmoothSpeed * Time.deltaTime);
    }

    // ─────────────────────────────────────────────────────────────
    //  Arc on / off
    // ─────────────────────────────────────────────────────────────

    private void BeginArc()
    {
        if (teleportInteractor == null) return;
        teleportInteractor.gameObject.SetActive(true);
        _arcActive = true;
        if (_controllerGlow != null) _controllerGlow.SetActive(true);

        // LineRenderer in the scene has no material (fileID 0) — assign one.
        // Use Universal Render Pipeline/Unlit and drive _BaseColor each frame
        // so red/cyan colour works reliably without vertex-colour shader complexity.
        var lr = teleportInteractor.GetComponent<LineRenderer>();
        if (lr != null)
        {
            _lineMat ??= new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _lineMat.SetColor("_BaseColor", new Color(1f, 0.2f, 0.2f)); // start red
            lr.material          = _lineMat;
            lr.startWidth        = 0.02f;
            lr.endWidth          = 0.005f;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // Keep XRInteractorLineVisual for arc shape — disable its colour override
        // so our _BaseColor approach controls the colour instead.
        var visual = teleportInteractor.GetComponent<XRInteractorLineVisual>();
        if (visual != null)
        {
            visual.lineWidth            = 0.02f;
            visual.setLineColorGradient = false; // let us control colour via material
        }
    }

    private void EndArc()
    {
        if (teleportInteractor == null) return;

        // Teleport if the arc is pointing at a valid target
        if (teleportInteractor.TryGetHitInfo(out Vector3 hitPos, out _, out _, out bool valid) && valid)
            TeleportTo(hitPos);

        teleportInteractor.gameObject.SetActive(false);
        _arcActive = false;
        _reticle?.SetActive(false);
        _controllerGlow?.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────
    //  Reticle
    // ─────────────────────────────────────────────────────────────

    private void UpdateReticle()
    {
        if (_reticle == null || teleportInteractor == null) return;
        bool hit = teleportInteractor.TryGetHitInfo(out Vector3 pos, out _, out _, out bool valid) && valid;
        _reticle.SetActive(hit);
        if (hit) _reticle.transform.position = pos + Vector3.up * 0.01f;
    }

    private void UpdateArcColor()
    {
        if (_lineMat == null) return;
        bool valid = teleportInteractor.TryGetHitInfo(out _, out _, out _, out bool isValid) && isValid;
        _lineMat.SetColor("_BaseColor", valid
            ? new Color(0f, 0.9f, 1f)   // cyan  — valid anchor
            : new Color(1f, 0.2f, 0.2f)); // red   — no target
    }

    // ── Reticle fields ────────────────────────────────────────────
    private LineRenderer _ringLine;
    private LineRenderer _outerRingLine;
    private Material     _reticleMat;
    private float        _pulseTime;

    private void BuildReticle()
    {
        _reticleMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

        // Root object that holds both rings
        _reticle = new GameObject("_TeleportReticle");

        // Inner solid ring
        var innerGo = new GameObject("InnerRing");
        innerGo.transform.SetParent(_reticle.transform);
        _ringLine = innerGo.AddComponent<LineRenderer>();
        SetupRingRenderer(_ringLine, 0.45f, 0.018f, 32);

        // Outer thinner ring
        var outerGo = new GameObject("OuterRing");
        outerGo.transform.SetParent(_reticle.transform);
        _outerRingLine = outerGo.AddComponent<LineRenderer>();
        SetupRingRenderer(_outerRingLine, 0.62f, 0.006f, 32);

        // Small flat disc underneath (subtle fill)
        var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = "Disc";
        disc.transform.SetParent(_reticle.transform);
        disc.transform.localScale = new Vector3(0.8f, 0.002f, 0.8f);
        Destroy(disc.GetComponent<Collider>());
        var discMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        discMat.SetColor("_BaseColor", new Color(0f, 0.7f, 1f, 0.15f));
        disc.GetComponent<Renderer>().material = discMat;

        _reticle.SetActive(false);
    }

    private void SetupRingRenderer(LineRenderer lr, float radius, float width, int segments)
    {
        lr.material          = _reticleMat;
        lr.startWidth        = width;
        lr.endWidth          = width;
        lr.loop              = true;
        lr.useWorldSpace     = false;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows    = false;
        lr.positionCount     = segments;

        for (int i = 0; i < segments; i++)
        {
            float a = i / (float)segments * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0.01f, Mathf.Sin(a) * radius));
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Teleport
    // ─────────────────────────────────────────────────────────────

    private void TeleportTo(Vector3 destination)
    {
        if (_provider == null) return;
        _provider.QueueTeleportRequest(new TeleportRequest
        {
            destinationPosition = destination,
            destinationRotation = FacingRotation(destination),
            matchOrientation    = MatchOrientation.TargetUpAndForward,
            requestTime         = Time.time
        });
        Debug.Log($"[TeleportationManager] Teleported to {destination:F1}");
    }

    private void DebugCycleTeleport()
    {
        if (_provider == null) return;
        var anchors = FindObjectsByType<TeleportationAnchor>(FindObjectsSortMode.None);
        if (anchors.Length == 0) return;
        var t = anchors[_debugIndex % anchors.Length];
        _debugIndex++;
        TeleportTo(t.transform.position);
        Debug.Log($"[TeleportationManager] Debug → {t.name}");
    }

    // ─────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────

    private void FaceTowardTable(Transform t)
    {
        if (tableCenter == null) return;
        Vector3 dir = tableCenter.position - t.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            t.rotation = Quaternion.LookRotation(dir.normalized);
    }

    private Quaternion FacingRotation(Vector3 from)
    {
        if (tableCenter == null) return Quaternion.identity;
        Vector3 dir = tableCenter.position - from;
        dir.y = 0f;
        return dir.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(dir.normalized)
            : Quaternion.identity;
    }

    // ─────────────────────────────────────────────────────────────
    //  Controller glow — pulsing orb at joystick tip
    // ─────────────────────────────────────────────────────────────

    private void BuildControllerGlow()
    {
        if (teleportInteractor == null) return;

        _controllerGlow = new GameObject("_ControllerGlow");
        // No parent — we track the controller tip position manually each frame
        // to avoid hierarchy scale / position issues with the interactor transform
        _controllerGlow.transform.localScale = Vector3.one * 0.012f;

        // Outer soft sphere
        var outer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        outer.transform.SetParent(_controllerGlow.transform);
        outer.transform.localPosition = Vector3.zero;
        outer.transform.localScale    = Vector3.one;
        Destroy(outer.GetComponent<Collider>());
        _glowMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        _glowMat.SetColor("_BaseColor", new Color(1f, 0.15f, 0.1f));
        outer.GetComponent<Renderer>().material = _glowMat;

        // Inner bright core
        var inner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        inner.transform.SetParent(_controllerGlow.transform);
        inner.transform.localPosition = Vector3.zero;
        inner.transform.localScale    = Vector3.one * 0.45f;
        Destroy(inner.GetComponent<Collider>());
        var coreMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        coreMat.SetColor("_BaseColor", Color.white);
        inner.GetComponent<Renderer>().material = coreMat;

        _controllerGlow.SetActive(false);
    }

    private void UpdateControllerGlow()
    {
        if (_controllerGlow == null || teleportInteractor == null) return;

        // Snap to the actual ray origin (controller tip) each frame
        Transform tip = teleportInteractor.rayOriginTransform
            ?? teleportInteractor.attachTransform
            ?? teleportInteractor.transform;
        _controllerGlow.transform.position = tip.position;

        // Pulse size
        _glowPulse += Time.deltaTime * 4f;
        float scale = 0.012f + Mathf.Sin(_glowPulse) * 0.002f;
        _controllerGlow.transform.localScale = Vector3.one * scale;

        bool valid = teleportInteractor.TryGetHitInfo(out _, out _, out _, out bool isValid) && isValid;
        Color c = valid
            ? new Color(0.1f, 0.4f, 1f)   // blue  — valid anchor
            : new Color(1f,   0.15f, 0.1f); // red   — no target
        if (_glowMat != null) _glowMat.SetColor("_BaseColor", c);
    }

    private void OnDestroy()
    {
        if (_lineMat  != null) Destroy(_lineMat);
        if (_glowMat  != null) Destroy(_glowMat);
        if (_reticleMat != null) Destroy(_reticleMat);
    }
}
