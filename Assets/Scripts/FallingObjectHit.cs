using UnityEngine;

public class FallingObjectHit : MonoBehaviour
{
    [Header("Game Over")]
    [SerializeField] private GameOverController gameOverController;

    private bool hasHitPlayer = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHitPlayer)
            return;

        if (HasPlayerTag(collision.transform))
        {
            hasHitPlayer = true;

            Debug.Log(
                "낙하물이 플레이어에게 닿았습니다."
            );

            if (gameOverController != null)
            {
                gameOverController.TriggerGameOver();
            }
        }
    }

    private bool HasPlayerTag(Transform target)
    {
        Transform current = target;

        while (current != null)
        {
            if (current.CompareTag("Player"))
                return true;

            current = current.parent;
        }

        return false;
    }
}
