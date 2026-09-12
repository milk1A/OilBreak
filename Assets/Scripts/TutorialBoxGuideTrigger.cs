using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TutorialBoxGuideTrigger : MonoBehaviour
{
    [SerializeField] private TutorialGuide tutorialGuide;
    private BoxCollider detectionCollider;
    private bool triggered;

    private void Awake()
    {
        detectionCollider = GetComponent<BoxCollider>();
        detectionCollider.isTrigger = true;
        if (tutorialGuide == null)
            Debug.LogWarning("Assign Tutorial Guide on ReadyBoxMove.", this);
    }

    private void FixedUpdate()
    {
        if (triggered || tutorialGuide == null || !tutorialGuide.isActiveAndEnabled ||
            !detectionCollider.enabled) return;

        // Query the trigger volume so a Rigidbody is not required on the empty object.
        Transform box = detectionCollider.transform;
        Vector3 scale = box.lossyScale;
        scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        Collider[] hits = Physics.OverlapBox(box.TransformPoint(detectionCollider.center),
            Vector3.Scale(detectionCollider.size, scale) * 0.5f,
            box.rotation, ~0, QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            Transform target = hit.transform;
            while (target != null)
            {
                if (target.CompareTag("Player"))
                {
                    triggered = true;
                    tutorialGuide.ShowBoxGuide();
                    return;
                }
                target = target.parent;
            }
        }
    }
}
