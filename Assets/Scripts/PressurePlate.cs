using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Trap")]
    [SerializeField] private FallingTrap fallingTrap;

    [Header("Game Over")]
    [SerializeField] private GameOverController gameOverController;

    [Header("Detection")]
    [SerializeField] private BoxCollider detectionCollider;

    [SerializeField] private float extraHeight = 0.5f;

    [Header("Settings")]
    [SerializeField] private bool oneShot = true;

    private bool activated = false;

    private void Awake()
    {
        if (detectionCollider == null)
        {
            detectionCollider = GetComponent<BoxCollider>();
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

            if (HasTagInParents(detected.transform, "Player"))
            {
                ActivateTrap();
                return;
            }

            if (HasTagInParents(detected.transform, "PickupBox"))
            {
                ActivateTrap();
                return;
            }
        }
    }

    private bool HasTagInParents(
        Transform target,
        string targetTag)
    {
        Transform current = target;

        while (current != null)
        {
            if (current.CompareTag(targetTag))
                return true;

            current = current.parent;
        }

        return false;
    }

    private void ActivateTrap()
    {
        if (activated && oneShot)
            return;

        activated = true;
        GameAudio.Play(GameSound.PlatePress);

        Debug.Log("발판 활성화!");

        // 플레이어 즉시 정지
        if (gameOverController != null)
        {
            gameOverController.LockPlayer();
        }

        // 낙하물 떨어뜨림
        if (fallingTrap != null)
        {
            fallingTrap.Drop();
        }
    }
}

