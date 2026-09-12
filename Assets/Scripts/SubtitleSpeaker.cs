using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SubtitleSpeaker : MonoBehaviour
{
    [Header("Subtitle")]
    [TextArea(2, 5)]
    [SerializeField] private string subtitleMessage;
    [SerializeField, Min(0.01f)] private float displayDuration = 2f;

    [Header("Detection")]
    [SerializeField] private BoxCollider detectionCollider;

    [Header("References")]
    [SerializeField] private SubtitleUI subtitleUI;

    [Header("Settings")]
    [SerializeField] private bool oneShot = true;

    private bool hasPlayed;
    private bool wasOccupied;

    private void Awake()
    {
        if (detectionCollider == null)
            detectionCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        if (subtitleUI == null)
            subtitleUI = FindFirstObjectByType<SubtitleUI>();
    }

    private void Update()
    {
        if (oneShot && hasPlayed)
            return;

        bool occupied = IsPlayerTouchingBox();
        // Display on entry only, so remaining inside does not reset the timer.
        if (occupied && !wasOccupied)
            ShowSubtitle();

        wasOccupied = occupied;
    }

    private bool IsPlayerTouchingBox()
    {
        if (detectionCollider == null || !detectionCollider.enabled ||
            !detectionCollider.gameObject.activeInHierarchy)
            return false;

        Transform boxTransform = detectionCollider.transform;
        Vector3 scale = boxTransform.lossyScale;
        scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        Vector3 halfSize = Vector3.Scale(detectionCollider.size, scale) * 0.5f;
        Vector3 center = boxTransform.TransformPoint(detectionCollider.center);

        // Works with CharacterController without requiring a Rigidbody on the speaker.
        Collider[] hits = Physics.OverlapBox(center, halfSize,
            boxTransform.rotation, ~0, QueryTriggerInteraction.Collide);

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

    private void ShowSubtitle()
    {
        if (subtitleUI == null)
        {
            Debug.LogWarning("Assign SubtitleUI to SubtitleSpeaker.", this);
            return;
        }

        hasPlayed = true;
        subtitleUI.ShowSubtitle(subtitleMessage, displayDuration);
    }

    private void OnDisable()
    {
        wasOccupied = false;
    }
}
