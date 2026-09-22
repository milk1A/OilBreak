using UnityEngine;
using UnityEngine.SceneManagement;

public class SimplePortalExit : MonoBehaviour
{
    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private TutorialStoryIntro exitCutscene;

    private bool isLoading = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading)
            return;

        if (!HasPlayerTag(other.transform))
            return;

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning(
                "다음 씬 이름이 설정되지 않았습니다."
            );
            return;
        }

        if (exitCutscene != null)
        {
            if (exitCutscene.PlayBeforeScene(nextSceneName))
            {
                isLoading = true;
                GameAudio.Play(GameSound.Portal);
            }
            else Debug.LogError("Exit Cutscene could not start. Disable Play On Awake and check its UI references.", this);
            return;
        }

        isLoading = true;
        GameAudio.Play(GameSound.Portal);

        Debug.Log(
            "포탈 진입 -> 다음 씬 이동: " +
            nextSceneName
        );

        SceneManager.LoadScene(nextSceneName);
    }

    private bool HasPlayerTag(Transform target)
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