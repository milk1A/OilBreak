using UnityEngine;
using Pathfinding;

public class BossChaser : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private AIPath aiPath;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private GameOverController gameOverController;


    [Header("Speed")]

    [Tooltip("보스 최소 속도")]
    [SerializeField]
    private float slowSpeed = 2f;

    [Tooltip("보스 최대 속도")]
    [SerializeField]
    private float fastSpeed = 6f;

    [Tooltip("몇 초마다 속도를 변경할지")]
    [SerializeField]
    private float speedChangeInterval = 2f;


    [Header("Catch")]

    [Tooltip("이 거리 안으로 들어오면 플레이어를 잡은 것으로 처리")]
    [SerializeField]
    private float catchDistance = 1.2f;


    [Header("Ground Lock")]

    [Tooltip("보스가 박스나 플레이어 위로 올라가는 것을 막기 위해 Y좌표를 고정")]
    [SerializeField]
    private bool lockYPosition = true;


    private bool chaseActive = false;
    private bool playerCaught = false;

    private float speedTimer = 0f;
    private float groundY;


    private void Awake()
    {
        // AIPath 자동 연결
        if (aiPath == null)
        {
            aiPath = GetComponent<AIPath>();
        }

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
    }


    private void Start()
    {
        // 보스 시작 Y값 저장
        groundY =
            transform.position.y;

        // 게임 시작 시 보스 정지
        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.maxSpeed = 0f;
        }

        chaseActive = false;
        playerCaught = false;
        speedTimer = 0f;
    }


    private void Update()
    {
        if (!chaseActive)
            return;

        if (playerCaught)
            return;

        if (aiPath == null ||
            player == null)
        {
            return;
        }


        // ================================
        // 플레이어를 잡았는지 검사
        // ================================

        CheckPlayerCaught();

        if (playerCaught)
            return;


        // ================================
        // 보스 속도 변경
        // ================================

        speedTimer += Time.deltaTime;

        if (speedTimer >=
            speedChangeInterval)
        {
            speedTimer = 0f;

            ChangeSpeed();
        }
    }


    private void LateUpdate()
    {
        if (!lockYPosition)
            return;

        // 보스가 박스나 Player 위로
        // 올라가버리는 현상 방지
        Vector3 position =
            transform.position;

        position.y =
            groundY;

        transform.position =
            position;
    }


    // =====================================================
    // 버튼 4개 완료 시 호출
    // =====================================================

    public void StartChase()
    {
        if (chaseActive)
            return;

        if (aiPath == null)
            return;

        chaseActive = true;
        playerCaught = false;

        speedTimer = 0f;

        aiPath.canMove = true;

        ChangeSpeed();
    }


    // =====================================================
    // 추격 중지
    // =====================================================

    public void StopChase()
    {
        chaseActive = false;

        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.maxSpeed = 0f;
        }
    }


    // =====================================================
    // 속도 랜덤 변경
    // =====================================================

    private void ChangeSpeed()
    {
        if (aiPath == null)
            return;

        aiPath.maxSpeed =
            Random.Range(
                slowSpeed,
                fastSpeed
            );
    }


    // =====================================================
    // 플레이어 잡기
    // =====================================================

    private void CheckPlayerCaught()
    {
        // X/Z 거리만 사용
        // 높이 차이는 무시
        Vector3 bossPosition =
            transform.position;

        Vector3 playerPosition =
            player.position;

        bossPosition.y = 0f;
        playerPosition.y = 0f;


        float distance =
            Vector3.Distance(
                bossPosition,
                playerPosition
            );


        if (distance >
            catchDistance)
        {
            return;
        }


        if (playerCaught)
            return;


        playerCaught = true;
        chaseActive = false;


        // 보스 즉시 정지
        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.maxSpeed = 0f;
        }


        // 플레이어 정지 + 현재 씬 재시작
        if (gameOverController != null)
        {
            gameOverController.LockPlayer();

            gameOverController.RestartScene();
        }
    }
}