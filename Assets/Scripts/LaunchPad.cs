using System.Collections;
using UnityEngine;
using StarterAssets;

public class LaunchPad : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private BoxCollider detectionCollider;

    [Tooltip("발판 위쪽 감지 영역 추가 높이")]
    [SerializeField] private float extraHeight = 0.5f;

    [Header("Launch Target")]
    [Tooltip("튕겨 보낼 목표 위치")]
    [SerializeField] private Transform launchTarget;

    [Header("Launch Settings")]
    [Tooltip("목표 위치까지 이동하는 시간")]
    [SerializeField] private float launchDuration = 0.8f;

    [Tooltip("포물선의 최대 높이")]
    [SerializeField] private float arcHeight = 3f;

    [Tooltip("한 번 작동한 뒤 다시 감지할 때까지 시간")]
    [SerializeField] private float cooldown = 1f;

    private bool isLaunching = false;

    private void Awake()
    {
        if (detectionCollider == null)
        {
            detectionCollider = GetComponent<BoxCollider>();
        }
    }

    private void Update()
    {
        if (isLaunching)
            return;

        CheckPad();
    }

    private void CheckPad()
    {
        if (detectionCollider == null || launchTarget == null)
            return;

        Vector3 center =
            detectionCollider.transform.TransformPoint(
                detectionCollider.center
            );

        Vector3 worldSize =
            Vector3.Scale(
                detectionCollider.size,
                detectionCollider.transform.lossyScale
            );

        worldSize.y += extraHeight;

        Collider[] detectedObjects =
            Physics.OverlapBox(
                center,
                worldSize * 0.5f,
                detectionCollider.transform.rotation,
                ~0,
                QueryTriggerInteraction.Collide
            );

        foreach (Collider detected in detectedObjects)
        {
            // 발판 자기 자신 무시
            if (detected == detectionCollider)
                continue;

            // =============================
            // Player 감지
            // =============================

            Transform player =
                FindParentWithTag(
                    detected.transform,
                    "Player"
                );

            if (player != null)
            {
                StartCoroutine(
                    LaunchPlayer(player)
                );

                return;
            }

            // =============================
            // PickupBox 감지
            // =============================

            Transform box =
                FindParentWithTag(
                    detected.transform,
                    "PickupBox"
                );

            if (box != null)
            {
                Rigidbody boxRb =
                    box.GetComponent<Rigidbody>();

                if (boxRb != null)
                {
                    StartCoroutine(
                        LaunchBox(boxRb)
                    );

                    return;
                }
            }
        }
    }

    // =====================================================
    // 플레이어 튕기기
    // =====================================================

    private IEnumerator LaunchPlayer(
        Transform player)
    {
        isLaunching = true;

        CharacterController controller =
            player.GetComponent<CharacterController>();

        ThirdPersonController movement =
            player.GetComponent<ThirdPersonController>();

        // 이동 입력 잠깐 정지
        if (movement != null)
        {
            movement.enabled = false;
        }

        // CharacterController가 transform 이동을 방해하지 않도록
        if (controller != null)
        {
            controller.enabled = false;
        }

        Vector3 startPosition =
            player.position;

        Vector3 endPosition =
            launchTarget.position;

        float time = 0f;

        while (time < launchDuration)
        {
            time += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    time / launchDuration
                );

            // 시작 → 목표 위치
            Vector3 position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    t
                );

            // 포물선
            float arc =
                4f *
                arcHeight *
                t *
                (1f - t);

            position.y += arc;

            player.position =
                position;

            yield return null;
        }

        player.position =
            endPosition;

        if (controller != null)
        {
            controller.enabled = true;
        }

        if (movement != null)
        {
            movement.enabled = true;
        }

        yield return new WaitForSeconds(
            cooldown
        );

        isLaunching = false;
    }

    // =====================================================
    // 박스 튕기기
    // =====================================================

    private IEnumerator LaunchBox(
        Rigidbody box)
    {
        isLaunching = true;

        box.linearVelocity =
            Vector3.zero;

        box.angularVelocity =
            Vector3.zero;

        box.useGravity = false;
        box.isKinematic = true;

        Vector3 startPosition =
            box.position;

        Vector3 endPosition =
            launchTarget.position;

        float time = 0f;

        while (time < launchDuration)
        {
            time += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    time / launchDuration
                );

            Vector3 position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    t
                );

            float arc =
                4f *
                arcHeight *
                t *
                (1f - t);

            position.y += arc;

            box.position =
                position;

            yield return null;
        }

        box.position =
            endPosition;

        box.isKinematic = false;
        box.useGravity = true;

        yield return new WaitForSeconds(
            cooldown
        );

        isLaunching = false;
    }

    // =====================================================
    // 부모를 타고 올라가면서 Tag 탐색
    // =====================================================

    private Transform FindParentWithTag(
        Transform target,
        string targetTag)
    {
        Transform current = target;

        while (current != null)
        {
            if (current.CompareTag(targetTag))
            {
                return current;
            }

            current = current.parent;
        }

        return null;
    }

    // =====================================================
    // Scene에서 감지 영역 표시
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        BoxCollider box =
            detectionCollider;

        if (box == null)
        {
            box = GetComponent<BoxCollider>();
        }

        if (box == null)
            return;

        Vector3 center =
            box.transform.TransformPoint(
                box.center
            );

        Vector3 worldSize =
            Vector3.Scale(
                box.size,
                box.transform.lossyScale
            );

        worldSize.y += extraHeight;

        Gizmos.color = Color.cyan;

        Matrix4x4 oldMatrix =
            Gizmos.matrix;

        Gizmos.matrix =
            Matrix4x4.TRS(
                center,
                box.transform.rotation,
                Vector3.one
            );

        Gizmos.DrawWireCube(
            Vector3.zero,
            worldSize
        );

        Gizmos.matrix =
            oldMatrix;
    }
}