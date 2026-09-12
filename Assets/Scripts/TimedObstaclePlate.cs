using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TimedObstaclePlate : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private BoxCollider detectionCollider;
    [SerializeField, Min(0f)] private float extraHeight = 0.5f;

    [Header("Obstacle")]
    [Tooltip("Assign the obstacle root, including its colliders. Keep the plate separate.")]
    [SerializeField] private GameObject obstacle;
    [SerializeField, Min(0.01f)] private float openDuration = 2f;

    private bool wasOccupied;
    private bool isOpen;
    private float closeTime;

    private void Awake()
    {
        if (detectionCollider == null)
            detectionCollider = GetComponent<BoxCollider>();

        if (obstacle == null || obstacle.transform == transform ||
            transform.IsChildOf(obstacle.transform))
        {
            Debug.LogError("Assign an obstacle separate from the TimedObstaclePlate and its parents.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (obstacle == null || detectionCollider == null)
            return;

        if (isOpen && Time.time >= closeTime)
            RestoreObstacle();

        bool occupied = IsPlayerOnPlate();
        // Only a new step activates the plate. Standing on it cannot extend the timer.
        if (occupied && !wasOccupied && !isOpen)
        {
            isOpen = true;
            closeTime = Time.time + Mathf.Max(0.01f, openDuration);
            obstacle.SetActive(false);
        }

        wasOccupied = occupied;
    }

    private bool IsPlayerOnPlate()
    {
        Vector3 scale = detectionCollider.transform.lossyScale;
        scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        Vector3 size = Vector3.Scale(detectionCollider.size, scale);
        Vector3 center = detectionCollider.transform.TransformPoint(detectionCollider.center);
        center += detectionCollider.transform.up * (extraHeight * 0.5f);
        size.y += extraHeight;

        Collider[] hits = Physics.OverlapBox(center, size * 0.5f,
            detectionCollider.transform.rotation, ~0, QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            if (hit == detectionCollider)
                continue;

            Transform current = hit.transform;
            while (current != null)
            {
                if (current.CompareTag("Player"))
                    return true;

                current = current.parent;
            }
        }

        return false;
    }

    private void RestoreObstacle()
    {
        isOpen = false;
        if (obstacle != null)
            obstacle.SetActive(true);
    }

    private void OnDisable()
    {
        // Do not leave the obstacle disabled if the plate is disabled mid-countdown.
        if (isOpen)
            RestoreObstacle();

        wasOccupied = false;
    }
}
