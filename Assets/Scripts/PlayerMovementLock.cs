using UnityEngine;

public class PlayerMovementLock : MonoBehaviour
{
    [Header("잠글 이동 스크립트")]
    [SerializeField] private Behaviour movementScript;

    private bool isLocked;

    public bool IsLocked => isLocked;

    public void LockMovement()
    {
        if (isLocked)
            return;

        isLocked = true;

        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        Debug.Log("플레이어 이동 잠금");
    }

    public void UnlockMovement()
    {
        if (!isLocked)
            return;

        isLocked = false;

        if (movementScript != null)
        {
            movementScript.enabled = true;
        }

        Debug.Log("플레이어 이동 잠금 해제");
    }
}
