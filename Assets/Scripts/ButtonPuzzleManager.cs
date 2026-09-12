using UnityEngine;

public class ButtonPuzzleManager : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private int requiredButtonCount = 4;

    [Header("Portal")]
    [SerializeField] private GameObject portalObject;

    [Header("Boss")]
    [SerializeField] private BossChaser bossChaser;

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
        }
        else
        {
            Debug.LogError(
                "ButtonPuzzleManager의 BossChaser가 연결되지 않았습니다."
            );
        }
    }
}
