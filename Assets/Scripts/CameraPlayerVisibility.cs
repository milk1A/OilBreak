using System.Collections.Generic;
using UnityEngine;

// Only changes mesh visibility. Player movement, colliders and box interaction stay active.
public class CameraPlayerVisibility : MonoBehaviour
{
    [SerializeField] private bool hideWhenTooClose = true;
    [SerializeField, Min(0f)] private float extraClearance = 0.15f;
    [SerializeField, Min(0f)] private float restoreMargin = 0.25f;
    private Transform playerRoot;
    private readonly List<Renderer> bodyRenderers = new List<Renderer>();
    private readonly Dictionary<Renderer, bool> previousVisibility = new Dictionary<Renderer, bool>();
    private bool hidden;

    public void UpdateVisibility(Transform target, Vector3 cameraPosition, float nearPlaneRadius)
    {
        if (!isActiveAndEnabled || !hideWhenTooClose || target == null)
        {
            Restore();
            return;
        }

        Transform root = FindPlayerRoot(target);
        if (root != playerRoot)
        {
            Restore();
            playerRoot = root;
            bodyRenderers.Clear();
            if (root != null)
            {
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                    bool isBox = false;
                    for (Transform t = renderer.transform; t != null && t != root; t = t.parent)
                        if (t.CompareTag("PickupBox")) { isBox = true; break; }
                    if (!isBox) bodyRenderers.Add(renderer);
                }
            }
        }

        // Use mesh bounds rather than root distance: arms/head can intersect the
        // near clipping plane even when the camera is outside the character capsule.
        float clearance = Mathf.Max(0f, nearPlaneRadius) + extraClearance + (hidden ? restoreMargin : 0f);
        bool tooClose = false;
        foreach (Renderer renderer in bodyRenderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
            if ((renderer.bounds.ClosestPoint(cameraPosition) - cameraPosition).sqrMagnitude <= clearance * clearance)
            {
                tooClose = true;
                break;
            }
        }

        if (!tooClose) { Restore(); return; }
        if (hidden) return;
        hidden = true;
        foreach (Renderer renderer in bodyRenderers)
        {
            if (renderer == null) continue;
            previousVisibility[renderer] = renderer.forceRenderingOff;
            renderer.forceRenderingOff = true;
        }
    }

    private static Transform FindPlayerRoot(Transform target)
    {
        CharacterController controller = target.GetComponentInParent<CharacterController>();
        if (controller != null) return controller.transform;
        for (Transform t = target; t != null; t = t.parent)
            if (t.CompareTag("Player")) return t;
        return null;
    }

    public void Restore()
    {
        foreach (var pair in previousVisibility)
            if (pair.Key != null) pair.Key.forceRenderingOff = pair.Value;
        previousVisibility.Clear();
        hidden = false;
    }

    private void OnDisable() { Restore(); }
    private void OnDestroy() { Restore(); }
}
