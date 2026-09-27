using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private UnityEngine.UI.Slider musicSlider;
    [SerializeField] private UnityEngine.UI.Slider effectsSlider;

    [Header("Restart")]
    [Tooltip("Name of the first scene, or the tutorial scene if the game starts there.")]
    [SerializeField] private string firstSceneName = "Stage1";

    private bool isOpen;
    public bool IsOpen => isOpen;
    private bool isLoading;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;

    private void Awake()
    {
        if (settingsPanel == null || settingsPanel == gameObject ||
            transform.IsChildOf(settingsPanel.transform))
        {
            Debug.LogError("Assign a settings panel separate from this controller and its parents.", this);
            enabled = false;
            return;
        }

        settingsPanel.SetActive(false);
        SetupSlider(musicSlider, GameAudio.MusicVolume, GameAudio.SetMusicVolume);
        SetupSlider(effectsSlider, GameAudio.EffectsVolume, GameAudio.SetEffectsVolume);
    }

    private void SetupSlider(UnityEngine.UI.Slider slider, float value, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null) return;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.SetValueWithoutNotify(value);
        slider.onValueChanged.AddListener(callback);
    }

    private void Update()
    {
        if (!isLoading && Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
            ToggleMenu();
    }

    public void ToggleMenu()
    {
        if (isOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        if (!enabled || isOpen || isLoading || settingsPanel == null)
            return;

        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        isOpen = true;
        settingsPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseMenu()
    {
        if (!isOpen)
            return;

        isOpen = false;
        PlayerPrefs.Save();
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
    }

    public void PlayFromBeginning()
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(firstSceneName) ||
            !Application.CanStreamedLevelBeLoaded(firstSceneName))
        {
            Debug.LogError("Check First Scene Name and add that scene to the build scene list.", this);
            return;
        }

        isLoading = true;
        CloseMenu();
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstSceneName);
    }

    public void QuitGame()
    {
        CloseMenu();
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDisable()
    {
        CloseMenu();
    }

    private void OnDestroy()
    {
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(GameAudio.SetMusicVolume);
        if (effectsSlider != null) effectsSlider.onValueChanged.RemoveListener(GameAudio.SetEffectsVolume);
    }
}
