using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f; // 회전 속도
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private GameManager gameManager;

    private float targetRotationY = 0f;

    private void Awake()
    {
        // 최상위 GameManager 탐색
        gameManager = transform.root.GetComponent<GameManager>();
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        targetRotationY = transform.eulerAngles.y;
    }

    private void Update()
    {
        // 1. 바닥 착지 감지
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 2. 키 입력 및 회전 각도 설정
        float x = 0f;
        if (gameManager.MoveLeft())
        {
            x = -1f;
        }
        else if (gameManager.MoveRight())
        {
            x = 1f;
        }

        float z = 0f;
        if (gameManager.MoveDown()) // S 키
        {
            z = -1f;
            targetRotationY = -90f; // S 입력 시 -90도
        }
        else if (gameManager.MoveUp()) // W 키
        {
            z = 1f;
            targetRotationY = 90f;  // W 입력 시 +90도
        }

        // 3. Y축 기준 회전 적용 (부드러운 보간)
        Quaternion targetRotation = Quaternion.Euler(0f, targetRotationY, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // 즉시 딱 꺾이길 원하시면 위 두 줄 대신 아래 코드를 사용하세요:
        // transform.rotation = Quaternion.Euler(0f, targetRotationY, 0f);

        // 4. 이동 벡터 계산 (월드 절대 좌표 기준 이동)
        // 캐릭터 회전과 별개로 좌우(X), 전후(Z) 입력을 독립 적용하기 위해 Vector3.right / forward 사용
        Vector3 move = Vector3.right * x + Vector3.forward * z;
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        controller.Move(move * moveSpeed * Time.deltaTime);

        // 5. 점프 처리
        if (gameManager.PressJump() && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 6. 중력 가속도 적용
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
