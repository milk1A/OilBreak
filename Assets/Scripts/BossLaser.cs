using UnityEngine;

public class BossLaser : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform player;

    [SerializeField]
    private Transform laserOrigin;

    [SerializeField]
    private GameOverController gameOverController;


    [Header("Boss Rotation")]
    [SerializeField]
    private Transform bossBody;

    [SerializeField]
    private float rotationSpeed = 8f;


    [Header("Laser")]
    [SerializeField]
    private LineRenderer laserLine;

    [Tooltip("레이저 굵기")]
    [SerializeField]
    private float laserWidth = 0.25f;


    [Header("Aim")]
    [Tooltip("Player Transform 기준으로 몇 미터 위를 조준할지")]
    [SerializeField]
    private float playerAimHeight = 1.2f;


    [Header("Attack")]
    [SerializeField]
    private float attackDelay = 0.5f;


    private bool attacking = false;
    private bool playerHit = false;

    private float attackTimer = 0f;


    private void Start()
    {
        // Player 자동 검색
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player =
                    playerObject.transform;
            }
        }


        if (bossBody == null)
        {
            bossBody =
                transform;
        }


        // LineRenderer 초기 설정
        if (laserLine != null)
        {
            laserLine.useWorldSpace = true;

            laserLine.positionCount = 2;

            laserLine.enabled = false;

            laserLine.startWidth =
                laserWidth;

            laserLine.endWidth =
                laserWidth;

            laserLine.startColor =
                Color.red;

            laserLine.endColor =
                Color.red;
        }
    }


    private void Update()
    {
        if (!attacking)
            return;

        if (player == null)
            return;


        // 플레이어 방향으로 회전
        RotateTowardsPlayer();


        attackTimer +=
            Time.deltaTime;


        // 발사 딜레이
        if (attackTimer <
            attackDelay)
        {
            return;
        }


        FireLaser();
    }


    // =====================================================
    // 센서가 플레이어 감지했을 때 호출
    // =====================================================

    public void StartAttack()
    {
        if (attacking)
            return;


        attacking = true;
        playerHit = false;

        attackTimer = 0f;


        // 이전 레이저가 남아있다면 숨김
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
    }


    // =====================================================
    // 플레이어 바라보기
    // =====================================================

    private void RotateTowardsPlayer()
    {
        Vector3 direction =
            player.position -
            bossBody.position;


        // 좌우 회전만 사용
        direction.y = 0f;


        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }


        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );


        bossBody.rotation =
            Quaternion.Slerp(
                bossBody.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }


    // =====================================================
    // 레이저 발사
    // =====================================================

    private void FireLaser()
    {
        if (laserOrigin == null)
            return;


        Vector3 startPosition =
            laserOrigin.position;


        Vector3 targetPosition =
            player.position +
            Vector3.up *
            playerAimHeight;


        // ============================
        // 레이저 표시
        // ============================

        if (laserLine != null)
        {
            laserLine.enabled = true;

            laserLine.SetPosition(
                0,
                startPosition
            );

            laserLine.SetPosition(
                1,
                targetPosition
            );
        }


        // ============================
        // 플레이어 명중
        // ============================

        if (playerHit)
            return;


        playerHit = true;

        // ★ 명중 후 Update 중단
        attacking = false;


        if (gameOverController != null)
        {
            // 즉시 플레이어 정지
            gameOverController.LockPlayer();

            // 2초 후 Game Over UI
            gameOverController.TriggerGameOver();
        }
    }
}
