using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialGuide : MonoBehaviour
{
    [Header("Guide UI")]
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private GameObject guideText;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Messages")]
    [TextArea(2, 5)]
    [SerializeField] private string movementMessage = "WASD로 움직여 보세요.";
    [TextArea(2, 5)]
    [SerializeField] private string boxMessage = "마우스 왼쪽 버튼으로 상자를 집고, 다시 클릭해 내려놓으세요.\n마우스 휠로 상자와의 거리를 조절할 수 있어요.";

    private enum Step { Movement, WaitingForBoxArea, Box }
    private Step step;
    private TMPro.TMP_Text tmpText;
    private UnityEngine.UI.Text legacyText;
    private Vector3 previousPlayerPosition;

    private void Awake()
    {
        if (guidePanel == null || guideText == null || guidePanel == gameObject ||
            transform.IsChildOf(guidePanel.transform))
        {
            Debug.LogError("Attach TutorialGuide to an always-active object and assign a separate Guide Panel and Guide Text.", this);
            enabled = false;
            return;
        }

        tmpText = guideText.GetComponent<TMPro.TMP_Text>();
        legacyText = guideText.GetComponent<UnityEngine.UI.Text>();
        if (tmpText == null && legacyText == null)
        {
            Debug.LogError("Guide Text must have a Text or TextMeshPro component.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
        if (player != null) previousPlayerPosition = player.position;
        if (step == Step.Movement) ShowMessage(movementMessage);
    }

    private void LateUpdate()
    {
        if (step != Step.Movement || player == null) return;
        Vector3 delta = player.position - previousPlayerPosition;
        previousPlayerPosition = player.position;
        delta.y = 0f;
        Keyboard keys = Keyboard.current;
        bool movementInput = keys != null && (keys.wKey.isPressed || keys.aKey.isPressed ||
            keys.sKey.isPressed || keys.dKey.isPressed);

        // Require actual horizontal movement, not just pressing a key into a wall.
        if (movementInput && delta.sqrMagnitude > 0.000001f)
        {
            step = Step.WaitingForBoxArea;
            guidePanel.SetActive(false);
        }
    }

    public void ShowBoxGuide()
    {
        if (!isActiveAndEnabled || step == Step.Box) return;
        step = Step.Box;
        ShowMessage(boxMessage);
    }

    private void ShowMessage(string message)
    {
        if (tmpText != null) tmpText.text = message;
        else if (legacyText != null) legacyText.text = message;
        guidePanel.SetActive(true);
    }
}
