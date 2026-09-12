using UnityEngine;

public class PlayerDetector : MonoBehaviour
{

    [Header("Detection")]
    [SerializeField] private Transform player;

    [SerializeField] private float detectionRange = 5f;

    [Header("Boss")]
    [SerializeField] private BossLaser bossLaser;

    [Header("Settings")]
    [SerializeField] private bool oneShot = true;

    private bool triggered = false;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void Update()
    {
        if (triggered && oneShot)
            return;

        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance <= detectionRange)
        {
            TriggerBoss();
        }
    }

    private void TriggerBoss()
    {
        if (triggered && oneShot)
            return;

        triggered = true;

        Debug.Log("센서가 플레이어 감지!");

        if (bossLaser != null)
        {
            bossLaser.StartAttack();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );
    }
}
