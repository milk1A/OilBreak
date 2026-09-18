using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-500)]
public class TutorialStoryIntro : MonoBehaviour
{
    [Header("Story UI")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject storyText;
    [TextArea(3, 10)]
    [SerializeField] private string storyMessage = "여기에 스토리 설명을 입력하세요.";
    [SerializeField, Min(0f)] private float displayDuration = 5f;

    private readonly List<Behaviour> pausedBehaviours = new List<Behaviour>();
    private bool introActive;
    private float previousTimeScale;

    private void Awake()
    {
        if (storyPanel == null || storyText == null || storyPanel == gameObject ||
            transform.IsChildOf(storyPanel.transform))
        {
            Debug.LogError("Put TutorialStoryIntro on the Canvas and assign a separate Story Panel and Story Text.", this);
            enabled = false;
            return;
        }

        var tmp = storyText.GetComponent<TMPro.TMP_Text>();
        var text = storyText.GetComponent<Text>();
        if (tmp == null && text == null)
        {
            Debug.LogError("Story Text must have Text or TextMeshPro.", this);
            enabled = false;
            return;
        }
        if (tmp != null) tmp.text = storyMessage;
        else text.text = storyMessage;

        // Use a dedicated overlay canvas so all existing HUD is covered.
        Canvas overlay = storyPanel.GetComponent<Canvas>();
        if (overlay == null) overlay = storyPanel.AddComponent<Canvas>();
        overlay.overrideSorting = true;
        overlay.sortingOrder = 32767;
        Image background = storyPanel.GetComponent<Image>();
        if (background == null) background = storyPanel.AddComponent<Image>();
        background.color = Color.black;
        background.raycastTarget = true;
        RectTransform rect = storyPanel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        storyPanel.SetActive(true);

        previousTimeScale = Time.timeScale;
        introActive = true;
        Time.timeScale = 0f;
        foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (behaviour.gameObject.scene != gameObject.scene || !behaviour.enabled) continue;
            if (behaviour is StarterAssets.ThirdPersonController || behaviour is PlayerMovement ||
                behaviour is BoxPickUp || behaviour is MouseOrbitCamera ||
                behaviour is TutorialGuide || behaviour is TutorialBoxGuideTrigger ||
                behaviour is SettingsMenu)
            {
                pausedBehaviours.Add(behaviour);
                behaviour.enabled = false;
            }
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(displayDuration);
        FinishIntro();
    }

    private void FinishIntro()
    {
        if (!introActive) return;
        introActive = false;
        if (storyPanel != null) storyPanel.SetActive(false);
        Time.timeScale = previousTimeScale;
        foreach (Behaviour behaviour in pausedBehaviours)
            if (behaviour != null) behaviour.enabled = true;
        pausedBehaviours.Clear();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        FinishIntro();
    }
}
