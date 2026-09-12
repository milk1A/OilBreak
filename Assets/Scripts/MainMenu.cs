using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("First gameplay scene, or the tutorial scene.")]
    [SerializeField] private string firstSceneName = "Stage1";

    private bool isLoading;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstSceneName);
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
