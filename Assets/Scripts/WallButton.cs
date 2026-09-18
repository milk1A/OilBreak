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
        if (isPressed || !CenterAimController.CanInteract(playerCamera))
            return;

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPressButton();
        }
    }

    public bool CanPressFrom(Camera camera)
    {
        if (!isActiveAndEnabled || isPressed || camera == null ||
            camera != playerCamera || !CenterAimController.CanInteract(camera))
            return false;

        Ray ray = CenterAimController.GetAimRay(camera);
        return CenterAimController.TryGetAimHit(ray, interactionDistance, out RaycastHit hit) &&
            hit.collider.GetComponentInParent<WallButton>() == this;
    }

    private void TryPressButton()
    {
        // Require the displayed green target and revalidate range/occlusion on click.
        if (CenterAimController.IsHighlightedButton(playerCamera, this) && CanPressFrom(playerCamera))
            PressButton();
    }

    private void PressButton()
    {
        if (isPressed || !CenterAimController.CanInteract(playerCamera))
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
