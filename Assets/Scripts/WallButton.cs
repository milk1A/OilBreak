using UnityEngine;
using UnityEngine.InputSystem;

public class WallButton : MonoBehaviour
{

    [Header("Manager")]
    [SerializeField] private ButtonPuzzleManager puzzleManager;

    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 5f;

    [Header("Visual")]
    [SerializeField] private Transform buttonVisual;
    [SerializeField] private float pressDepth = 0.08f;

    private bool isPressed = false;

    private Vector3 originalLocalPosition;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (buttonVisual == null)
        {
            buttonVisual = transform;
        }

        originalLocalPosition =
            buttonVisual.localPosition;
    }

    private void Update()
    {
        if (isPressed)
            return;

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPressButton();
        }
    }

    private void TryPressButton()
    {
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            playerCamera.ScreenPointToRay(
                mousePosition
            );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            // 클릭한 Collider가 이 버튼 자신 또는 자식인지 확인
            WallButton clickedButton =
                hit.collider.GetComponentInParent<WallButton>();

            if (clickedButton != this)
                return;

            PressButton();
        }
    }

    private void PressButton()
    {
        if (isPressed)
            return;

        isPressed = true;
        GameAudio.Play(GameSound.PlatePress);

        Debug.Log(
            "버튼 활성화: " +
            gameObject.name
        );

        // 버튼 눌림 연출
        if (buttonVisual != null)
        {
            buttonVisual.localPosition =
                originalLocalPosition +
                Vector3.forward * pressDepth;
        }

        if (puzzleManager != null)
        {
            puzzleManager.RegisterButtonPress();
        }
    }
}
