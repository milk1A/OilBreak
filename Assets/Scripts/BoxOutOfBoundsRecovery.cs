using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DefaultExecutionOrder(-600)]
public class BoxOutOfBoundsRecovery : MonoBehaviour
{
    [Tooltip("Optional trigger volume enclosing the map and the entire box drop path.")]
    [SerializeField] private BoxCollider allowedArea;
    [Tooltip("Optional safe return point. Otherwise uses the box position before gameplay.")]
    [SerializeField] private Transform returnPoint;
    [SerializeField, Min(1f)] private float fallDistance = 15f;
    private Rigidbody body;
    private Collider[] colliders;
    private bool[] colliderStates;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool initialKinematic;
    private bool initialGravity;
    private BoxPickUp[] pickupControllers;
    public int RecoveryVersion { get; private set; }

    public void RememberDropPosition()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialKinematic = body.isKinematic;
        initialGravity = body.useGravity;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialKinematic = body.isKinematic;
        initialGravity = body.useGravity;
        colliders = GetComponentsInChildren<Collider>(true);
        colliderStates = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++) colliderStates[i] = colliders[i].enabled;
        pickupControllers = FindObjectsByType<BoxPickUp>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    private void LateUpdate()
    {
        foreach (var pickup in pickupControllers)
            if (pickup != null && pickup.IsHoldingBox(body)) return;
        Vector3 home = initialPosition;
        Quaternion rotation = initialRotation;
        foreach (var pickup in pickupControllers)
            if (pickup != null && pickup.TryGetBoxOriginal(body, out Vector3 savedPosition, out Quaternion savedRotation))
            {
                home = savedPosition;
                rotation = savedRotation;
                break;
            }
        if (body.position.y >= home.y - fallDistance && (allowedArea == null || Contains(body.position))) return;
        RecoveryVersion++;
        // Stop residual momentum before restoring the initial physics state.
        if (!body.isKinematic)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        body.position = returnPoint != null ? returnPoint.position : home;
        body.rotation = returnPoint != null ? returnPoint.rotation : rotation;
        for (int i = 0; i < colliders.Length; i++)
            if (colliders[i] != null) colliders[i].enabled = colliderStates[i];
        body.isKinematic = initialKinematic;
        body.useGravity = initialGravity;
        Physics.SyncTransforms();
    }

    private bool Contains(Vector3 position)
    {
        Vector3 local = allowedArea.transform.InverseTransformPoint(position) - allowedArea.center;
        Vector3 half = allowedArea.size * 0.5f;
        return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
    }
}
