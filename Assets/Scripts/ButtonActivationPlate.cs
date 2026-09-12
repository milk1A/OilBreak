using UnityEngine;

public class ButtonActivationPlate : MonoBehaviour
{

    [Header("Detection")]
    [SerializeField] private BoxCollider detectionCollider;

    [SerializeField] private float extraHeight = 0.5f;

    [Header("Wall Button")]
    [Tooltip("이 발판으로 활성화할 벽 버튼")]
    [SerializeField] private GameObject wallButton;

    [Header("Settings")]
    [SerializeField] private bool oneShot = true;

    private bool activated = false;

    private void Awake()
    {
        if (detectionCollider == null)
        {
            detectionCollider =
                GetComponent<BoxCollider>();
        }
    }

    private void Start()
    {
        activated = false;

        // 씬 시작 시 벽 버튼 비활성화
        if (wallButton != null)
        {
            wallButton.SetActive(false);
        }
    }

    private void Update()
    {
        if (activated && oneShot)
            return;

        CheckPlate();
    }

    private void CheckPlate()
    {
        if (detectionCollider == null)
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
            if (detected == detectionCollider)
                continue;

            if (HasTagInParents(
                    detected.transform,
                    "Player") ||
                HasTagInParents(
                    detected.transform,
                    "PickupBox"))
            {
                ActivateButton();
                return;
            }
        }
    }

    private void ActivateButton()
    {
        if (activated && oneShot)
            return;

        activated = true;

        if (wallButton != null)
        {
            wallButton.SetActive(true);
        }

        Debug.Log("벽 버튼 활성화!");
    }

    private bool HasTagInParents(
        Transform target,
        string targetTag)
    {
        Transform current = target;

        while (current != null)
        {
            if (current.CompareTag(targetTag))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

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

        Gizmos.color = Color.green;

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
