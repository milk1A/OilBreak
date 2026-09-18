using UnityEngine;
using UnityEngine.InputSystem;

public class MouseOrbitCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [Tooltip("Use upper-body height when Target itself has a CharacterController.")]
    [SerializeField] private bool usePlayerBodyHeight = true;
    [Tooltip("Additional world-space height above the target or body pivot.")]
    [SerializeField] private float targetHeightOffset = 0f;
    [Header("Camera Settings")]
    [SerializeField] private float distance = 4f;
    [SerializeField] private float mouseSensitivity = 0.15f;
    [Header("Vertical Look")]
    [SerializeField] private bool allowVerticalLook = true;
    [SerializeField, Range(0f, 85f)] private float fixedPitch = 45f;
    [SerializeField, Range(0f, 85f)] private float maximumPitch = 60f;
    [Header("Optional Rotation Limit")]
    [SerializeField] private bool limitRotation;
    [SerializeField, Range(0f, 180f)] private float maximumYawFromStart = 75f;
    [Header("Smooth")]
    [SerializeField] private float followSmooth = 15f;
    [Header("Camera Collision")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [Tooltip("Includes walls on Default and other layers. Player and pickup boxes are still ignored.")]
    [SerializeField] private bool checkAllWallLayers = true;
    [SerializeField, Min(0.01f)] private float cameraRadius = 0.25f;
    [SerializeField, Min(0f)] private float wallOffset = 0.15f;
    [SerializeField, Min(0f)] private float minDistance = 0.5f;
    [Header("Optional Map Boundary")]
    [Tooltip("A trigger BoxCollider enclosing the allowed camera area, including the player.")]
    [SerializeField] private BoxCollider cameraBounds;

    private float yaw;
    private float pitch;
    private Camera viewCamera;
    private float startingYaw;
    private bool positionInitialized;
    private float currentDistance;
    private CameraPlayerVisibility playerVisibility;

    private void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = Mathf.Clamp(Mathf.DeltaAngle(0f, transform.eulerAngles.x), 0f, maximumPitch);
        startingYaw = yaw;
        viewCamera = GetComponent<Camera>();
        playerVisibility = GetComponent<CameraPlayerVisibility>();
        if (playerVisibility == null) playerVisibility = gameObject.AddComponent<CameraPlayerVisibility>();
        if (viewCamera != null && GetComponent<CenterAimController>() == null)
            gameObject.AddComponent<CenterAimController>();
        if (!allowVerticalLook) pitch = fixedPitch;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            if (playerVisibility != null) playerVisibility.Restore();
            return;
        }
        if (CenterAimController.CanInteract(viewCamera) && Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            yaw += delta.x * mouseSensitivity;
            if (allowVerticalLook)
                pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, 0f, maximumPitch);
        }
        if (!allowVerticalLook) pitch = fixedPitch;

        if (limitRotation)
            yaw = startingYaw + Mathf.Clamp(Mathf.DeltaAngle(startingYaw, yaw),
                -maximumYawFromStart, maximumYawFromStart);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = GetPivotPosition();
        Vector3 desired = pivot - rotation * Vector3.forward * Mathf.Max(minDistance, distance);
        float radius = EffectiveRadius();
        desired = ClampToMap(desired, radius);
        desired = ConstrainPosition(pivot, desired, radius);

        Vector3 offset = desired - pivot;
        float safeDistance = offset.magnitude;
        Vector3 direction = safeDistance > 0.0001f
            ? offset / safeDistance
            : -(rotation * Vector3.forward);

        // Move inward immediately for walls, but ease outward when space opens.
        // Smooth only the distance: position interpolation cuts across corners.
        if (!positionInitialized || safeDistance < currentDistance)
            currentDistance = safeDistance;
        else
            currentDistance = Mathf.Lerp(currentDistance, safeDistance,
                1f - Mathf.Exp(-Mathf.Max(0f, followSmooth) * Time.deltaTime));

        positionInitialized = true;
        transform.position = pivot + direction * currentDistance;
        transform.rotation = rotation;
        playerVisibility.UpdateVisibility(target, transform.position, EffectiveRadius());
    }

    private float EffectiveRadius()
    {
        float radius = Mathf.Max(0.01f, cameraRadius);
        if (viewCamera == null || viewCamera.orthographic) return radius;
        float near = viewCamera.nearClipPlane;
        float halfHeight = near * Mathf.Tan(viewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * viewCamera.aspect;
        return Mathf.Max(radius, Mathf.Sqrt(near * near + halfHeight * halfHeight + halfWidth * halfWidth));
    }

    private Vector3 GetPivotPosition()
    {
        Vector3 pivot = target.position;
        if (usePlayerBodyHeight && target.TryGetComponent<CharacterController>(out var controller))
        {
            // The player root is normally at its feet. Cast from the upper body,
            // so the floor is not overlapping the camera's collision sphere.
            pivot = target.TransformPoint(controller.center + Vector3.up * controller.height * 0.25f);
        }
        return pivot + Vector3.up * targetHeightOffset;
    }

    private void OnDisable()
    {
        positionInitialized = false;
        if (playerVisibility != null) playerVisibility.Restore();
    }

    private Vector3 ConstrainPosition(Vector3 pivot, Vector3 candidate, float radius)
    {
        Vector3 delta = candidate - pivot;
        float length = delta.magnitude;
        if (length < 0.0001f) return pivot;
        Vector3 direction = delta / length;
        int mask = checkAllWallLayers || collisionMask.value == 0 ? ~0 : collisionMask.value;
        float safeDistance = length;
        foreach (RaycastHit hit in Physics.SphereCastAll(pivot, radius, direction,
            length, mask, QueryTriggerInteraction.Ignore))
        {
            if (IgnoreForCamera(hit.collider)) continue;
            safeDistance = Mathf.Min(safeDistance, Mathf.Max(0f, hit.distance - wallOffset));
        }
        // A minimum distance must never push the camera through a nearby wall.
        return pivot + direction * safeDistance;
    }

    private bool IgnoreForCamera(Collider hit)
    {
        if (hit == cameraBounds) return true;
        for (Transform t = hit.transform; t != null; t = t.parent)
        {
            if (t.CompareTag("Player") || t.CompareTag("PickupBox")) return true;
        }
        return hit.transform == target || hit.transform.IsChildOf(target);
    }

    private Vector3 ClampToMap(Vector3 position, float radius)
    {
        if (cameraBounds == null) return position;
        Transform boundsTransform = cameraBounds.transform;
        Vector3 local = boundsTransform.InverseTransformPoint(position) - cameraBounds.center;
        Vector3 scale = boundsTransform.lossyScale;
        Vector3 half = cameraBounds.size * 0.5f;
        for (int axis = 0; axis < 3; axis++)
        {
            float margin = radius / Mathf.Max(0.0001f, Mathf.Abs(scale[axis]));
            float extent = Mathf.Max(0f, half[axis] - margin);
            local[axis] = Mathf.Clamp(local[axis], -extent, extent);
        }
        return boundsTransform.TransformPoint(cameraBounds.center + local);
    }
}
