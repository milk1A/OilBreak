using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleUI : MonoBehaviour
{
    [Header("Subtitle UI")]
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private Text subtitleText;

    private Coroutine subtitleCoroutine;

    private void Start()
    {
        // 게임 시작 시 자막 숨기기
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }
    }

    public void ShowSubtitle(string message, float duration)
    {
        // 기존 자막이 실행 중이면 중지
        if (subtitleCoroutine != null)
        {
            StopCoroutine(subtitleCoroutine);
        }

        subtitleCoroutine =
            StartCoroutine(
                ShowSubtitleRoutine(message, duration)
            );
    }

    private IEnumerator ShowSubtitleRoutine(
        string message,
        float duration)
    {
        // 자막 내용 변경
        if (subtitleText != null)
        {
            subtitleText.text = message;
        }

        // 자막 켜기
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(true);
        }

        // 지정된 시간만큼 기다리기
        yield return new WaitForSeconds(duration);

        // 자막 끄기
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }

        subtitleCoroutine = null;
    }
}