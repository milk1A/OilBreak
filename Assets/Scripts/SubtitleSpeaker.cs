using UnityEngine;

public class SubtitleSpeaker : MonoBehaviour
{
    [Header("Subtitle")]

    [TextArea(2, 5)]
    [SerializeField] private string subtitleMessage;

    [Tooltip("자막이 화면에 표시되는 시간")]
    [SerializeField] private float displayDuration = 2f;


    [Header("Detection")]

    [Tooltip("플레이어가 이 거리 안으로 들어오면 자막 출력")]
    [SerializeField] private float detectionDistance = 5f;


    [Header("References")]

    [SerializeField] private Transform player;
    [SerializeField] private SubtitleUI subtitleUI;


    [Header("Settings")]

    [Tooltip("체크하면 한 번만 자막이 나옵니다.")]
    [SerializeField] private bool oneShot = true;


    private bool hasPlayed = false;


    private void Start()
    {
        // Player 자동 검색
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }


        // SubtitleUI 자동 검색
        if (subtitleUI == null)
        {
            subtitleUI =
                FindFirstObjectByType<SubtitleUI>();
        }
    }


    private void Update()
    {
        if (player == null)
            return;

        if (oneShot && hasPlayed)
            return;


        // 플레이어와 스피커 사이 거리
        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );


        // 감지 거리 안으로 들어오면
        if (distance <= detectionDistance)
        {
            ShowSubtitle();
        }
    }


    private void ShowSubtitle()
    {
        if (oneShot && hasPlayed)
            return;


        hasPlayed = true;


        if (subtitleUI != null)
        {
            subtitleUI.ShowSubtitle(
                subtitleMessage,
                displayDuration
            );
        }
        else
        {
            Debug.LogWarning(
                "SubtitleUI를 찾을 수 없습니다."
            );
        }
    }


    // Scene 창에서 감지 거리 확인
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            detectionDistance
        );
    }
}
