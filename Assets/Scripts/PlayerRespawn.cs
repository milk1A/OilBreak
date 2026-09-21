using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;

    [Header("Box")]
    [SerializeField] private BoxPickUp boxPickUp;

    private Rigidbody rb;
    private CharacterController characterController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Respawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 장애물을 Trigger로 만들었을 경우도 대응
        if (other.CompareTag("Obstacle"))
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        if (respawnPoint == null)
        {
            Debug.LogError("Assign Respawn Point on PlayerRespawn.", this);
            return;
        }

        // 플레이어가 죽으면 모든 박스 초기화
        if (boxPickUp != null)
        {
            boxPickUp.ResetAllBoxes();
        }

        if (characterController != null)
        {
            characterController.enabled = false;

            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;

            characterController.enabled = true;

            return;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;
    }
    }