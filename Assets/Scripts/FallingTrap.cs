using UnityEngine;

public class FallingTrap : MonoBehaviour
{
    [Header("Falling Object")]
    [SerializeField] private GameObject fallingObject;
    [SerializeField] private Rigidbody fallingRigidbody;
    [SerializeField] private Collider fallingCollider;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Spawn")]
    [SerializeField] private float spawnHeight = 5f;

    private Renderer[] renderers;
    private bool hasDropped = false;

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

        if (fallingObject == null)
        {
            Debug.LogError("Falling Object가 연결되어 있지 않습니다.");
            return;
        }

        if (fallingRigidbody == null)
        {
            fallingRigidbody =
                fallingObject.GetComponent<Rigidbody>();
        }

        if (fallingCollider == null)
        {
            fallingCollider =
                fallingObject.GetComponent<Collider>();
        }

        renderers =
            fallingObject.GetComponentsInChildren<Renderer>();

        // GameObject 자체는 끄지 않는다.
        // 화면에만 안 보이도록 Renderer를 끈다.
        SetVisible(false);

        if (fallingCollider != null)
        {
            fallingCollider.enabled = false;
        }

        if (fallingRigidbody != null)
        {
            fallingRigidbody.useGravity = false;
            fallingRigidbody.isKinematic = true;

            fallingRigidbody.linearVelocity = Vector3.zero;
            fallingRigidbody.angularVelocity = Vector3.zero;
        }
    }

    public void Drop()
    {
        if (hasDropped)
            return;

        hasDropped = true;

        if (fallingObject == null || player == null)
        {
            Debug.LogError(
                "FallingObject 또는 Player가 연결되어 있지 않습니다."
            );
            return;
        }

        // 현재 플레이어 위치 바로 위
        Vector3 spawnPosition =
            player.position +
            Vector3.up * spawnHeight;

        fallingObject.transform.position =
            spawnPosition;

        // 여기서 실제로 보이게 함
        SetVisible(true);

        if (fallingCollider != null)
        {
            fallingCollider.enabled = true;
        }

        // 중력 활성화
        if (fallingRigidbody != null)
        {
            fallingRigidbody.isKinematic = false;
            fallingRigidbody.useGravity = true;

            fallingRigidbody.linearVelocity = Vector3.zero;
            fallingRigidbody.angularVelocity = Vector3.zero;
        }

        Debug.Log(
            "낙하물 등장 및 낙하 시작: " +
            spawnPosition
        );
    }

    private void SetVisible(bool visible)
    {
        if (renderers == null)
            return;

        foreach (Renderer r in renderers)
        {
            if (r != null)
            {
                r.enabled = visible;
            }
        }
    }
}
