using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RisingTrapTrigger : MonoBehaviour
{
    [Tooltip("Place this separate object at the final raised position in the scene.")]
    [SerializeField] private Transform trapRoot;
    [SerializeField, Min(0f)] private float hideDepth = 3f;
    [SerializeField, Min(0f)] private float riseDuration = 0.4f;
    [SerializeField] private bool playSound = true;

    private BoxCollider detectionCollider;
    private Vector3 raisedPosition;
    private Vector3 hiddenPosition;
    private bool triggered;
    private float elapsed;

    public bool HasTriggered => triggered;
    public Transform TrapRoot => trapRoot;

    public void CompleteRise()
    {
        if (!triggered || trapRoot == null) return;
        trapRoot.position = raisedPosition;
        enabled = false;
    }

    private void Awake()
    {
        detectionCollider = GetComponent<BoxCollider>();
        detectionCollider.isTrigger = true;
        if (trapRoot == null || trapRoot == transform || transform.IsChildOf(trapRoot))
        {
            Debug.LogError("Assign a separate trap object to RisingTrapTrigger.", this);
            enabled = false;
            return;
        }

        // Physics-driven traps need a different movement implementation.
        foreach (Rigidbody body in trapRoot.GetComponentsInChildren<Rigidbody>(true))
        {
            if (!body.isKinematic)
            {
                Debug.LogError("RisingTrapTrigger requires trap Rigidbodies to be kinematic.", this);
                enabled = false;
                return;
            }
        }

        raisedPosition = trapRoot.position;
        hiddenPosition = raisedPosition - Vector3.up * hideDepth;
        trapRoot.position = hiddenPosition;
        trapRoot.gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (trapRoot == null) return;
        if (!triggered)
        {
            if (!detectionCollider.enabled) return;
            Transform box = detectionCollider.transform;
            Vector3 scale = box.lossyScale;
            scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            Collider[] hits = Physics.OverlapBox(box.TransformPoint(detectionCollider.center),
                Vector3.Scale(detectionCollider.size, scale) * 0.5f,
                box.rotation, ~0, QueryTriggerInteraction.Ignore);
            foreach (Collider hit in hits)
            {
                for (Transform candidate = hit.transform; candidate != null; candidate = candidate.parent)
                {
                    if (!candidate.CompareTag("Player")) continue;
                    triggered = true;
                    // Detection is one-shot; keep the return path clear while the trap rises.
                    detectionCollider.enabled = false;
                    if (playSound) GameAudio.Play(GameSound.TrapAppear);
                    break;
                }
                if (triggered) break;
            }
        }

        if (!triggered) return;
        elapsed += Time.fixedDeltaTime;
        float progress = riseDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / riseDuration);
        trapRoot.position = Vector3.Lerp(hiddenPosition, raisedPosition, progress);
        if (progress >= 1f) enabled = false;
    }
}
