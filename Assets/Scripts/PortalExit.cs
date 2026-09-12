using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalExit : MonoBehaviour
{

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;

    private bool isLoading = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading)
            return;

        if (!HasPlayerTag(other.transform))
            return;

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning(
                "다음 씬 이름이 설정되지 않았습니다."
            );

            return;
        }

        isLoading = true;
        GameAudio.Play(GameSound.Portal);

        Debug.Log(
            "다음 씬으로 이동: " +
            nextSceneName
        );

        SceneManager.LoadScene(
            nextSceneName
        );
    }

    private bool HasPlayerTag(
        Transform target)
    {
        Transform current = target;

        while (current != null)
        {
            if (current.CompareTag("Player"))
                return true;

            current = current.parent;
        }

        return false;
    }
}
