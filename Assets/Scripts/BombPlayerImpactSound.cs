using UnityEngine;

// Added only to the bomb spawned by StageThreeFinalTrap.
public class BombPlayerImpactSound : MonoBehaviour
{
    private Transform player;
    private System.Action onImpact;
    private bool played;

    public void Initialize(Transform target, System.Action callback)
    {
        player = target;
        onImpact = callback;
    }

    private void OnCollisionEnter(Collision collision) => CheckPlayer(collision.collider);
    private void OnTriggerEnter(Collider other) => CheckPlayer(other);

    private void CheckPlayer(Collider other)
    {
        if (played || player == null || other == null) return;
        if (other.transform != player && !other.transform.IsChildOf(player)) return;
        played = true;
        onImpact?.Invoke();
        onImpact = null;
    }
}
