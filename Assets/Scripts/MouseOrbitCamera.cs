using UnityEngine;
using UnityEngine.InputSystem;

public class MouseOrbitCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [Header("Camera Settings")]
    [SerializeField] private float distance = 4f;
    [SerializeField] private float mouseSensitivity = 0.15f;
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
    private Camera viewCamera;
    private float startingYaw;
    private bool cursorWasInside;

    private void Start()
    {
        yaw = transform.eulerAngles.y;
        startingYaw = yaw;
        viewCamera = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        bool cursorInside = IsCursorInsideGame();
        if (cursorInside && cursorWasInside &&
            !(UnityEngine.EventSystems.EventSystem.current != null &&
              UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()))
            yaw += Mouse.current.delta.ReadValue().x * mouseSensitivity;
        cursorWasInside = cursorInside;

        if (limitRotation)
            yaw = startingYaw + Mathf.Clamp(Mathf.DeltaAngle(startingYaw, yaw),
                -maximumYawFromStart, maximumYawFromStart);

        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 pivot = target.position;
        Vector3 desired = pivot - rotation * Vector3.forward * Mathf.Max(minDistance, distance);
        float radius = EffectiveRadius();
        desired = ClampToMap(desired, radius);
        desired = ConstrainPosition(pivot, desired, radius);

        Vector3 smoothed = Vector3.Lerp(transform.position, desired,
            Mathf.Clamp01(followSmooth * Time.deltaTime));
        // Smoothing around corners can put the camera behind a wall again.
        // Recheck the actual rendered position, and move inward immediately.
        transform.position = ConstrainPosition(pivot, ClampToMap(smoothed, radius), radius);
        transform.rotation = rotation;
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

    private bool IsCursorInsideGame()
    {
        if (!Application.isFocused || Mouse.current == null) return false;
#if UNITY_EDITOR
        // A focused Game view may still receive mouse input outside its tab.
        UnityEditor.EditorWindow hovered = UnityEditor.EditorWindow.mouseOverWindow;
        if (hovered == null || hovered.GetType().Name != "GameView") return false;
#endif
        Vector2 position = Mouse.current.position.ReadValue();
        Rect gameRect = new Rect(0f, 0f, Screen.width, Screen.height);
        if (!gameRect.Contains(position)) return false;
        return viewCamera == null || viewCamera.pixelRect.Contains(position);
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) cursorWasInside = false;
    }

    private void OnDisable()
    {
        cursorWasInside = false;
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
