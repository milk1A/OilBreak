using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("First gameplay scene, or the tutorial scene.")]
    [SerializeField] private string firstSceneName = "Stage1";

    [Tooltip("The game's title screen scene.")]
    [SerializeField] private string startScreenSceneName = "Start_S";

    private bool isLoading;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        LoadMenuScene(firstSceneName);
    }

    public void GoToStartScreen()
    {
        LoadMenuScene(startScreenSceneName);
    }

    private void LoadMenuScene(string sceneName)
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(sceneName) ||
            !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("Check the scene name and build scene list: " + sceneName, this);
            return;
        }

        isLoading = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
