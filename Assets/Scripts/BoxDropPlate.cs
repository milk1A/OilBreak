using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BoxDropPlate : MonoBehaviour
{
    [Header("Aim Button")]
    [SerializeField] private bool useAimClick = true;
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private Renderer[] buttonRenderers;
    [SerializeField] private Color pressedColor = Color.green;
    private Camera aimCamera;
    public bool CanPress(Camera camera)
    {
        return useAimClick && isActiveAndEnabled && !activated && fallingBox != null &&
            CenterAimController.CanInteract(camera) &&
            CenterAimController.TryGetAimHit(CenterAimController.GetAimRay(camera), interactionDistance, out RaycastHit hit) &&
            hit.collider.GetComponentInParent<BoxDropPlate>() == this;
    }
    [Header("Detection")]
    [SerializeField] private BoxCollider detectionCollider;
    [SerializeField, Min(0f)] private float extraHeight = 0.5f;

    [Header("Box")]
    [Tooltip("떨어뜨릴 상자의 Rigidbody. 발판과 별개의 오브젝트를 연결하세요.")]
    [SerializeField] private Rigidbody fallingBox;

    [Header("Drop Position")]
    [Tooltip("플레이어가 바라보는 방향으로 떨어진 거리")]
    [SerializeField, Min(0f)] private float forwardDistance = 2.5f;
    [Tooltip("플레이어 위치를 기준으로 상자가 나타나는 높이")]
    [SerializeField, Min(0f)] private float dropHeight = 4f;
    [Tooltip("비워두면 Player 태그 오브젝트의 정면을 사용합니다. 필요하면 캐릭터 방향 Transform을 연결하세요.")]
    [SerializeField] private Transform facingTransform;

    private bool activated;

    private void Awake()
    {
        if (detectionCollider == null)
            detectionCollider = GetComponent<BoxCollider>();

        if (fallingBox == null || fallingBox.transform == transform ||
            transform.IsChildOf(fallingBox.transform))
        {
            Debug.LogError("BoxDropPlate에 발판과 별개의 상자 Rigidbody를 연결하세요.", this);
            enabled = false;
            return;
        }

        fallingBox.gameObject.SetActive(false);
        aimCamera = Camera.main;
        if (useAimClick) detectionCollider.isTrigger = false;
    }

    private void Update()
    {
        if (activated || detectionCollider == null || fallingBox == null)
            return;

        if (useAimClick)
        {
            if (UnityEngine.InputSystem.Mouse.current != null &&
                UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame && CanPress(aimCamera))
            {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) DropBox(playerObject.transform);
            }
            return;
        }

        Vector3 scale = detectionCollider.transform.lossyScale;
        scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        Vector3 size = Vector3.Scale(detectionCollider.size, scale);
        Vector3 center = detectionCollider.transform.TransformPoint(detectionCollider.center);
        // 감지 영역을 발판 위쪽으로만 확장합니다.
        center += detectionCollider.transform.up * (extraHeight * 0.5f);
        size.y += extraHeight;

        Collider[] hits = Physics.OverlapBox(center, size * 0.5f,
            detectionCollider.transform.rotation, ~0, QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            if (hit == detectionCollider)
                continue;

            Transform player = FindPlayer(hit.transform);
            if (player == null)
                continue;

            DropBox(player);
            return;
        }
    }

    private void DropBox(Transform player)
    {
        Transform facing = facingTransform != null ? facingTransform : player;
        Vector3 forward = Vector3.ProjectOnPlane(facing.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        Vector3 spawnPosition = player.position + forward * forwardDistance + Vector3.up * dropHeight;

        activated = true;
        if (buttonRenderers == null || buttonRenderers.Length == 0)
            buttonRenderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in buttonRenderers)
        {
            if (renderer == null) continue;
            var properties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties);
            properties.SetColor("_BaseColor", pressedColor);
            properties.SetColor("_Color", pressedColor);
            renderer.SetPropertyBlock(properties);
        }
        GameAudio.Play(GameSound.PlatePress);
        GameAudio.Play(GameSound.TrapAppear);
        fallingBox.position = spawnPosition;
        fallingBox.transform.position = spawnPosition;
        fallingBox.gameObject.SetActive(true);
        fallingBox.isKinematic = false;
        fallingBox.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        fallingBox.useGravity = true;
        fallingBox.linearVelocity = Vector3.zero;
        fallingBox.angularVelocity = Vector3.zero;
        fallingBox.WakeUp();
        var recovery = fallingBox.GetComponent<BoxOutOfBoundsRecovery>();
        if (recovery == null) recovery = fallingBox.gameObject.AddComponent<BoxOutOfBoundsRecovery>();
        recovery.RememberDropPosition();
    }

    private static Transform FindPlayer(Transform target)
    {
        while (target != null)
        {
            if (target.CompareTag("Player"))
                return target;

            target = target.parent;
        }

        return null;
    }
}
