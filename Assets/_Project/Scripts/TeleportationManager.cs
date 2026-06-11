using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(XROrigin))]
public class TeleportationManager : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public XRRayInteractor teleportInteractor; // Teleport Interactor — Right Hand
    public Transform       tableCenter;        // Avatar faces this after teleport

    [Header("Keys")]
    public KeyCode debugKey = KeyCode.T;
    public KeyCode arcKey   = KeyCode.I;

    private TeleportationProvider _provider;
    private int                   _debugIndex;
    private bool                  _arcActive;

    // visuals
    private LineRenderer _arcLine;
    private Material     _arcMat;
    private GameObject   _reticle;

    // arc state
    private bool    _arcHitValid;
    private Vector3 _arcHitPos;

    // ─────────────────────────────────────────────────────────────

    private void Start()
    {
        _provider = GetComponent<TeleportationProvider>();

        var anchors = FindObjectsByType<TeleportationAnchor>(FindObjectsSortMode.None);
        foreach (var a in anchors)
        {
            a.teleportationProvider = _provider;
            FaceTowardTable(a.transform);
        }
        Debug.Log($"[TeleportationManager] Ready — {anchors.Length} anchor(s) wired.");

        BuildVisuals();

        // Keep built-in interactor off — we draw our own arc
        if (teleportInteractor != null)
            teleportInteractor.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(debugKey)) DebugCycleTeleport();
        if (Input.GetKeyDown(arcKey))   BeginArc();
        if (Input.GetKeyUp(arcKey))     EndArc();

        if (_arcActive) DrawArc();
    }

    // ─────────────────────────────────────────────────────────────
    //  Arc on / off
    // ─────────────────────────────────────────────────────────────

    private void BeginArc()
    {
        _arcActive = true;
        _arcLine.gameObject.SetActive(true);
    }

    private void EndArc()
    {
        if (_arcHitValid) TeleportTo(_arcHitPos);

        _arcActive = false;
        _arcHitValid = false;
        _arcLine.positionCount = 0;
        _arcLine.gameObject.SetActive(false);
        if (_reticle != null) _reticle.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────
    //  Arc drawing — always follows camera forward, ignores controller rotation
    // ─────────────────────────────────────────────────────────────

    private void DrawArc()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Origin: always at waist/hip height to the right of camera.
        // This keeps the full arc curve visible from first-person — starting the arc
        // at eye level and shooting forward means you look straight along it (appears as a dot).
        Vector3 origin = cam.transform.position
                       + cam.transform.right   * 0.3f   // right-hand side
                       + Vector3.down          * 0.8f   // waist height
                       + cam.transform.forward * 0.2f;  // slightly in front

        // Direction: camera horizontal forward tilted 30° downward.
        Vector3 camFwd     = cam.transform.forward;
        Vector3 horizontal = new Vector3(camFwd.x, 0f, camFwd.z);
        if (horizontal.sqrMagnitude < 0.01f) horizontal = Vector3.forward;
        horizontal.Normalize();

        float rad = 30f * Mathf.Deg2Rad;
        Vector3 vel = (horizontal * Mathf.Cos(rad) + Vector3.down * Mathf.Sin(rad)) * 7f;

        // Simulate parabola
        var     pts    = new List<Vector3>();
        Vector3 pos    = origin;
        float   dt     = 0.05f;
        bool    hasHit = false;
        Vector3 hitPt  = Vector3.zero;
        Vector3 hitNrm = Vector3.up;
        int     mask   = Physics.DefaultRaycastLayers;

        for (int i = 0; i < 45; i++)
        {
            pts.Add(pos);
            vel.y        += -9.8f * dt;
            Vector3 next  = pos + vel * dt;
            if (Physics.Linecast(pos, next, out RaycastHit hit, mask))
            {
                pts.Add(hit.point);
                hitPt  = hit.point;
                hitNrm = hit.normal;
                hasHit = true;
                break;
            }
            pos = next;
            if (pos.y < origin.y - 25f) break;
        }

        // Is landing point near a TeleportationAnchor?
        _arcHitValid = false;
        _arcAnchorHit = null;
        if (hasHit)
        {
            foreach (var col in Physics.OverlapSphere(hitPt, 0.5f))
            {
                var a = col.GetComponent<TeleportationAnchor>();
                if (a != null) { _arcAnchorHit = a; break; }
            }

            if (_arcAnchorHit != null)
            {
                _arcHitPos   = _arcAnchorHit.transform.position;
                _arcHitValid = true;
            }
            else if (Vector3.Angle(hitNrm, Vector3.up) < 45f)
            {
                _arcHitPos   = hitPt;
                _arcHitValid = true;
            }
        }

        // Line colour: cyan = valid, red = invalid
        Color c = _arcHitValid
            ? new Color(0f, 0.9f, 1f, 1f)
            : new Color(1f, 0.2f, 0.2f, 1f);
        _arcLine.startColor = c;
        _arcLine.endColor   = new Color(c.r, c.g, c.b, 0.2f);

        _arcLine.positionCount = pts.Count;
        _arcLine.SetPositions(pts.ToArray());

        // Reticle
        if (_reticle != null)
        {
            _reticle.SetActive(_arcHitValid);
            if (_arcHitValid)
                _reticle.transform.position = _arcHitPos + Vector3.up * 0.01f;
        }
    }

    private TeleportationAnchor _arcAnchorHit;

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
    //  Visuals setup
    // ─────────────────────────────────────────────────────────────

    private void BuildVisuals()
    {
        // Arc line
        var go = new GameObject("_ArcLine");
        go.transform.SetParent(transform);
        _arcLine = go.AddComponent<LineRenderer>();
        _arcMat  = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        _arcMat.SetColor("_BaseColor", Color.white);
        _arcLine.sharedMaterial    = _arcMat;
        _arcLine.startWidth        = 0.025f;
        _arcLine.endWidth          = 0.008f;
        _arcLine.useWorldSpace     = true;
        _arcLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _arcLine.receiveShadows    = false;
        _arcLine.positionCount     = 0;
        _arcLine.gameObject.SetActive(false);

        // Reticle disc
        _reticle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _reticle.name = "_TeleportReticle";
        _reticle.transform.localScale = new Vector3(0.6f, 0.005f, 0.6f);
        Destroy(_reticle.GetComponent<Collider>());
        var reticleMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        reticleMat.SetColor("_BaseColor", new Color(0f, 0.9f, 1f, 0.6f));
        _reticle.GetComponent<Renderer>().material = reticleMat;
        _reticle.SetActive(false);
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

    private void OnDestroy()
    {
        if (_arcMat != null) Destroy(_arcMat);
    }
}