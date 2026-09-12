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

    [Header("Pressed Color")]
    [SerializeField] private Color pressedColor = Color.green;
    [Tooltip("Leave empty to color the renderers under Button Visual.")]
    [SerializeField] private Renderer[] buttonRenderers;

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
        if (playerCamera == null) return;

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
        ApplyPressedColor();
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
    private void ApplyPressedColor()
    {
        if (buttonRenderers == null || buttonRenderers.Length == 0)
        {
            Transform visual = buttonVisual != null ? buttonVisual : transform;
            buttonRenderers = visual.GetComponentsInChildren<Renderer>(true);
        }

        var properties = new MaterialPropertyBlock();
        foreach (Renderer renderer in buttonRenderers)
        {
            if (renderer == null) continue;
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                properties.Clear();
                renderer.GetPropertyBlock(properties, i);
                properties.SetColor("_BaseColor", pressedColor);
                properties.SetColor("_Color", pressedColor);
                renderer.SetPropertyBlock(properties, i);
            }
        }
    }
}
