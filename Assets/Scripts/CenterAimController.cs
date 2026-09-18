using UnityEngine;
using UnityEngine.UI;

// Created automatically by MouseOrbitCamera; no scene or prefab wiring required.
[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(100)]
public class CenterAimController : MonoBehaviour
{
    // Shared by all scenes and by both the visual crosshair and physics ray.
    private static readonly Vector2 AimViewportPosition = new Vector2(0.5f, 0.6f);
    private Camera aimCamera;
    private MouseOrbitCamera orbit;
    private SettingsMenu[] menus;
    private GameOverController[] gameOvers;
    private GameObject crosshairCanvas;
    private RectTransform crosshair;
    private BoxPickUp[] pickupControllers;
    private Image horizontalMark;
    private Image verticalMark;
    private WallButton highlightedButton;

    public static bool IsHighlightedButton(Camera camera, WallButton button)
    {
        if (camera == null || button == null) return false;
        var aim = camera.GetComponent<CenterAimController>();
        return aim != null && aim.highlightedButton == button && CanInteract(camera);
    }

    private void Awake()
    {
        aimCamera = GetComponent<Camera>();
        orbit = GetComponent<MouseOrbitCamera>();
        menus = FindObjectsByType<SettingsMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        gameOvers = FindObjectsByType<GameOverController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        pickupControllers = FindObjectsByType<BoxPickUp>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        crosshairCanvas = new GameObject("Center Aim UI", typeof(RectTransform), typeof(Canvas));
        crosshairCanvas.transform.SetParent(transform, false);
        var canvas = crosshairCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        crosshair = new GameObject("Crosshair", typeof(RectTransform)).GetComponent<RectTransform>();
        crosshair.SetParent(crosshairCanvas.transform, false);
        crosshair.anchorMin = crosshair.anchorMax = Vector2.zero;
        AddMark("Horizontal outline", new Vector2(18f, 4f), Color.black);
        AddMark("Vertical outline", new Vector2(4f, 18f), Color.black);
        horizontalMark = AddMark("Horizontal", new Vector2(16f, 2f), Color.white);
        verticalMark = AddMark("Vertical", new Vector2(2f, 16f), Color.white);
        crosshairCanvas.SetActive(false);
    }

    private Image AddMark(string label, Vector2 size, Color color)
    {
        var mark = new GameObject(label, typeof(RectTransform), typeof(Image));
        mark.transform.SetParent(crosshair, false);
        mark.GetComponent<RectTransform>().sizeDelta = size;
        var graphic = mark.GetComponent<Image>();
        graphic.color = color;
        graphic.raycastTarget = false;
        return graphic;
    }

    private bool IsGameplayActive()
    {
        if (!isActiveAndEnabled || !Application.isFocused || Time.timeScale <= 0f ||
            aimCamera == null || !aimCamera.isActiveAndEnabled || orbit == null || !orbit.isActiveAndEnabled)
            return false;
        foreach (var menu in menus)
            if (menu != null && menu.IsOpen) return false;
        foreach (var gameOver in gameOvers)
            if (gameOver != null && gameOver.IsPlayerLocked) return false;
        return true;
    }

    public static bool CanInteract(Camera camera)
    {
        if (camera == null) return false;
        var aim = camera.GetComponent<CenterAimController>();
        // Do not pass the UI click that closes a menu through to world interaction.
        return aim != null && aim.IsGameplayActive() && Cursor.lockState == CursorLockMode.Locked;
    }

    public static Ray GetAimRay(Camera camera)
    {
        return camera.ViewportPointToRay(new Vector3(AimViewportPosition.x, AimViewportPosition.y, 0f));
    }

    public static bool TryGetAimHit(Ray ray, float distance, out RaycastHit closest)
    {
        closest = default;
        float nearest = float.PositiveInfinity;
        foreach (var hit in Physics.RaycastAll(ray, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            bool player = false;
            for (Transform t = hit.collider.transform; t != null; t = t.parent)
                if (t.CompareTag("Player")) { player = true; break; }
            if (player || hit.distance >= nearest) continue;
            nearest = hit.distance;
            closest = hit;
        }
        return nearest < float.PositiveInfinity;
    }

    private void LateUpdate()
    {
        highlightedButton = null;
        bool active = IsGameplayActive();
        crosshairCanvas.SetActive(active);
        Rect rect = aimCamera.pixelRect;
        crosshair.anchoredPosition = rect.position + Vector2.Scale(rect.size, AimViewportPosition);
        if (!Application.isFocused) return;
        Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !active;
        bool canPickup = false;
        if (active)
        {
            foreach (var pickup in pickupControllers)
            {
                if (pickup != null && pickup.UsesCamera(aimCamera) && pickup.TryGetPickupTarget(out _))
                {
                    canPickup = true;
                    break;
                }
            }
        }
        if (active && TryGetAimHit(GetAimRay(aimCamera), aimCamera.farClipPlane, out RaycastHit buttonHit))
        {
            var button = buttonHit.collider.GetComponentInParent<WallButton>();
            if (button != null && button.CanPressFrom(aimCamera))
                highlightedButton = button;
        }
        Color color = canPickup || highlightedButton != null ? Color.green : Color.white;
        horizontalMark.color = verticalMark.color = color;
    }

    private void OnDisable()
    {
        highlightedButton = null;
        if (crosshairCanvas != null) crosshairCanvas.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        if (crosshairCanvas != null) Destroy(crosshairCanvas);
    }
}
