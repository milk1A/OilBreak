using UnityEngine;

[RequireComponent(typeof(PlayerRespawn))]
public class PlayerOutOfBoundsRecovery : MonoBehaviour
{
    [SerializeField, Min(1f)] private float fallDistance = 15f;
    [SerializeField, Min(0f)] private float boundaryMargin = 2f;
    [SerializeField, Min(0f)] private float outsideDelay = 0.5f;
    private PlayerRespawn respawn;
    private Bounds mapBounds;
    private bool hasBounds;
    private float initialHeight;
    private float outsideTime;

    private void Start()
    {
        respawn = GetComponent<PlayerRespawn>();
        initialHeight = transform.position.y;
        // Existing floor groups define the outer map limits without extra triggers.
        foreach (Transform item in FindObjectsByType<Transform>())
        {
            if (item.gameObject.scene != gameObject.scene || item.name != "PlaneObject") continue;
            foreach (Collider floor in item.GetComponentsInChildren<Collider>())
            {
                if (!floor.enabled || floor.isTrigger) continue;
                if (!hasBounds) { mapBounds = floor.bounds; hasBounds = true; }
                else mapBounds.Encapsulate(floor.bounds);
            }
        }
    }

    private void LateUpdate()
    {
        if (Time.timeScale <= 0f || respawn == null || !respawn.isActiveAndEnabled) return;
        foreach (GameOverController controller in FindObjectsByType<GameOverController>())
            if (controller.gameObject.scene == gameObject.scene && controller.IsPlayerLocked) return;
        Vector3 position = transform.position;
        bool below = position.y < initialHeight - fallDistance;
        bool outside = hasBounds &&
            (position.x < mapBounds.min.x - boundaryMargin || position.x > mapBounds.max.x + boundaryMargin ||
             position.z < mapBounds.min.z - boundaryMargin || position.z > mapBounds.max.z + boundaryMargin);
        outsideTime = outside ? outsideTime + Time.deltaTime : 0f;
        if (!below && (!outside || outsideTime < outsideDelay)) return;
        outsideTime = 0f;
        respawn.Respawn();
    }
}
