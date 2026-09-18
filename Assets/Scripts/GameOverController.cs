using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

public class GameOverController : MonoBehaviour
{
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Player")]
    [SerializeField] private ThirdPersonController playerMovement;

    [Header("Box Control")]
    [SerializeField] private BoxPickUp boxPickUp;

    [Header("Game Over")]
    [SerializeField] private float gameOverDelay = 2f;

    private bool isPlayerLocked = false;
    public bool IsPlayerLocked => isPlayerLocked;
    private bool isGameOverStarted = false;
    private bool isRestarting = false;


    private void Start()
    {
        // 게임 시작 시 Game Over UI 숨김
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        isPlayerLocked = false;
        isGameOverStarted = false;
        isRestarting = false;
    }


    // =====================================================
    // 플레이어 조작 정지
    // =====================================================

    public void LockPlayer()
    {
        if (isPlayerLocked)
            return;

        isPlayerLocked = true;

        // 플레이어 이동 정지
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // 박스 조작 정지
        if (boxPickUp != null)
        {
            boxPickUp.enabled = false;
        }

        // Main Camera / MouseOrbitCamera는 끄지 않음
    }


    // =====================================================
    // Game Over 시작
    // =====================================================

    public void TriggerGameOver()
    {
        if (isGameOverStarted)
            return;

        isGameOverStarted = true;

        // 혹시 아직 이동이 안 잠겼다면 잠금
        LockPlayer();

        StartCoroutine(GameOverRoutine());
    }


    // =====================================================
    // 일정 시간 후 Game Over UI 표시
    // =====================================================

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(gameOverDelay);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Restart 버튼 클릭 가능하게 커서 표시
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    // =====================================================
    // 현재 씬 재시작
    // =====================================================

    public void RestartScene()
    {
        // 여러 스크립트가 동시에 호출해도
        // 씬 로드는 한 번만 실행
        if (isRestarting)
            return;

        isRestarting = true;

        StartCoroutine(RestartSceneRoutine());
    }


    private IEnumerator RestartSceneRoutine()
    {
        Scene currentScene =
            SceneManager.GetActiveScene();

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                currentScene.buildIndex
            );

        // 로딩 완료될 때까지 대기
        while (!operation.isDone)
        {
            yield return null;
        }
    }
}