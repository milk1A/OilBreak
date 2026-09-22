using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-500)]
public class TutorialStoryIntro : MonoBehaviour
{
    [Header("Playback")]
    [SerializeField] private bool playOnAwake = true;
    private string destinationScene;
    [Header("Story UI")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject storyText;
    [TextArea(3, 10)]
    [SerializeField] private string storyMessage = "여기에 스토리 설명을 입력하세요.";
    [SerializeField, Min(0f)] private float displayDuration = 5f;

    [Header("Image Cutscene (assign slides to use button navigation)")]
    [SerializeField] private Image cutsceneImage;
    [SerializeField] private Sprite[] slides;
    [SerializeField] private Button nextButton;

    private readonly List<Behaviour> pausedBehaviours = new List<Behaviour>();
    private bool introActive;
    private float previousTimeScale;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private int slideIndex;
    private bool finishing;
    private bool UsesSlides => slides != null && slides.Length > 0;

    private void Awake()
    {
        if (playOnAwake) BeginIntro();
        else if (storyPanel != null && storyPanel != gameObject &&
            !transform.IsChildOf(storyPanel.transform)) storyPanel.SetActive(false);
    }

    public bool PlayBeforeScene(string sceneName)
    {
        if (!isActiveAndEnabled || introActive || playOnAwake) return false;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("Add the destination scene to the build scene list: " + sceneName, this);
            return false;
        }
        destinationScene = sceneName;
        BeginIntro();
        return introActive;
    }

    private void BeginIntro()
    {
        slideIndex = 0;
        finishing = false;
        if (storyPanel == null || storyPanel == gameObject ||
            transform.IsChildOf(storyPanel.transform))
        {
            Debug.LogError("Put TutorialStoryIntro on the Canvas and assign a separate Story Panel and Story Text.", this);
            enabled = false;
            return;
        }

        if (UsesSlides)
        {
            if (cutsceneImage == null || nextButton == null ||
                cutsceneImage.gameObject == storyPanel ||
                !cutsceneImage.transform.IsChildOf(storyPanel.transform) ||
                !nextButton.transform.IsChildOf(storyPanel.transform) ||
                System.Array.Exists(slides, slide => slide == null))
            {
                Debug.LogError("Assign all Slides, a child Cutscene Image and a child Next Button inside Story Panel.", this);
                enabled = false;
                return;
            }
            cutsceneImage.gameObject.SetActive(true);
            cutsceneImage.preserveAspect = true;
            cutsceneImage.raycastTarget = false;
            cutsceneImage.sprite = slides[0];
            nextButton.gameObject.SetActive(true);
            nextButton.interactable = true;
            nextButton.transform.SetAsLastSibling();
            nextButton.onClick.AddListener(NextSlide);
            if (storyText != null && storyText != storyPanel &&
                storyText != nextButton.gameObject && !nextButton.transform.IsChildOf(storyText.transform))
                storyText.SetActive(false);
        }
        else
        {
        var tmp = storyText != null ? storyText.GetComponent<TMPro.TMP_Text>() : null;
        var text = storyText != null ? storyText.GetComponent<Text>() : null;
        if (tmp == null && text == null)
        {
            Debug.LogError("Story Text must have Text or TextMeshPro.", this);
            enabled = false;
            return;
        }
        if (tmp != null) tmp.text = storyMessage;
        else text.text = storyMessage;
        }

        // Use a dedicated overlay canvas so all existing HUD is covered.
        Canvas overlay = storyPanel.GetComponent<Canvas>();
        if (overlay == null) overlay = storyPanel.AddComponent<Canvas>();
        overlay.overrideSorting = true;
        overlay.sortingOrder = 32767;
        if (storyPanel.GetComponent<GraphicRaycaster>() == null)
            storyPanel.AddComponent<GraphicRaycaster>();
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
        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
        if (!UsesSlides) StartCoroutine(TimedIntro());
    }

    private IEnumerator TimedIntro()
    {
        if (UsesSlides) yield break;
        yield return new WaitForSecondsRealtime(displayDuration);
        CompleteIntro();
    }

    public void NextSlide()
    {
        if (!introActive || !UsesSlides || finishing) return;
        slideIndex++;
        if (slideIndex < slides.Length)
            cutsceneImage.sprite = slides[slideIndex];
        else
        {
            finishing = true;
            nextButton.interactable = false;
            StartCoroutine(FinishAfterClick());
        }
    }

    private IEnumerator FinishAfterClick()
    {
        // Keep gameplay paused for the frame in which the UI button was clicked.
        yield return null;
        CompleteIntro();
    }

    private void CompleteIntro()
    {
        string scene = destinationScene;
        destinationScene = null;
        FinishIntro();
        if (!string.IsNullOrEmpty(scene))
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    private void FinishIntro()
    {
        if (!introActive) return;
        introActive = false;
        if (storyPanel != null) storyPanel.SetActive(false);
        Time.timeScale = previousTimeScale;
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
        if (nextButton != null) nextButton.onClick.RemoveListener(NextSlide);
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
