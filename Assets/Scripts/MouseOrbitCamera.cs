using UnityEngine;
using UnityEngine.InputSystem;

public class MouseOrbitCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private float distance = 4f;
    [SerializeField] private float mouseSensitivity = 0.15f;

    [Header("Smooth")]
    [SerializeField] private float followSmooth = 15f;

    [Header("Camera Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float cameraRadius = 0.25f;
    [SerializeField] private float wallOffset = 0.15f;
    [SerializeField] private float minDistance = 0.5f;

    private float yaw;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;

        yaw = angles.y;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (Mouse.current == null)
            return;

        // ============================
        // 좌우 카메라 회전
        // ============================

        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();

        yaw +=
            mouseDelta.x *
            mouseSensitivity;

        Quaternion rotation =
            Quaternion.Euler(
                0f,
                yaw,
                0f
            );

        // ============================
        // 원래 원하는 카메라 위치
        // ============================

        Vector3 direction =
            -(rotation * Vector3.forward);

        Vector3 targetPosition =
            target.position;

        float finalDistance =
            distance;

        // ============================
        // 플레이어와 카메라 사이 벽 검사
        // ============================

        if (Physics.SphereCast(
            targetPosition,
            cameraRadius,
            direction,
            out RaycastHit hit,
            distance,
            collisionMask,
            QueryTriggerInteraction.Ignore))
        {
            finalDistance =
                hit.distance -
                wallOffset;

            finalDistance =
                Mathf.Clamp(
                    finalDistance,
                    minDistance,
                    distance
                );
        }

        // ============================
        // 실제 카메라 위치
        // ============================

        Vector3 desiredPosition =
            targetPosition +
            direction *
            finalDistance;

        transform.position =
            Vector3.Lerp(
                transform.position,
                desiredPosition,
                followSmooth *
                Time.deltaTime
            );

        transform.rotation =
            rotation;
    }
}