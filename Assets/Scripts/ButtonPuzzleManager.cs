using UnityEngine;

public class ButtonPuzzleManager : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private int requiredButtonCount = 4;

    [Header("Portal")]
    [SerializeField] private GameObject portalObject;

    [Header("Boss")]
    [SerializeField] private BossChaser bossChaser;

    [Header("Stage 3 Guide")]
    [Tooltip("BossScene의 GuideText 오브젝트를 연결하세요.")]
    [SerializeField] private GameObject guideText;

    [TextArea(2, 5)]
    [SerializeField] private string chaseGuideMessage = "망했다. 경비가 눈치챘다.\n잡히면 분해될거야.";

    private int pressedButtonCount = 0;
    private bool puzzleCompleted = false;

    private void Start()
    {
        pressedButtonCount = 0;
        puzzleCompleted = false;

        if (portalObject != null)
        {
            portalObject.SetActive(false);
        }
    }

    public void RegisterButtonPress()
    {
        if (puzzleCompleted)
            return;

        pressedButtonCount++;

        Debug.Log(
            "버튼: " +
            pressedButtonCount +
            " / " +
            requiredButtonCount
        );

        if (pressedButtonCount >= requiredButtonCount)
        {
            CompletePuzzle();
        }
    }

    private void CompletePuzzle()
    {
        if (puzzleCompleted)
            return;

        puzzleCompleted = true;

        if (portalObject != null)
        {
            portalObject.SetActive(true);
            Debug.Log("포탈 활성화 성공");
        }

        if (bossChaser != null)
        {
            Debug.Log("BossChaser StartChase 호출");
            bossChaser.StartChase();
            UpdateChaseGuide();
        }
        else
        {
            Debug.LogError(
                "ButtonPuzzleManager의 BossChaser가 연결되지 않았습니다."
            );
        }
    }
    private void UpdateChaseGuide()
    {
        if (gameObject.scene.name != "BossScene" || guideText == null)
            return;

        TMPro.TMP_Text tmpText = guideText.GetComponent<TMPro.TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = chaseGuideMessage;
            return;
        }

        UnityEngine.UI.Text legacyText = guideText.GetComponent<UnityEngine.UI.Text>();
        if (legacyText != null)
            legacyText.text = chaseGuideMessage;
        else
            Debug.LogWarning("Guide Text에 Text 또는 TextMeshPro가 붙은 오브젝트를 연결하세요.", this);
    }
}
