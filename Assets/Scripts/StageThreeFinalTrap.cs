using System.Collections;
using UnityEngine;

// This encounter owns only Trap3-4 and its assigned plates.
public class StageThreeFinalTrap : MonoBehaviour
{
    [SerializeField] private BoxCollider entryArea;
    [SerializeField] private GameObject trapRoot;
    [SerializeField] private BoxCollider safePlate;
    [SerializeField] private BoxCollider bombPlate;
    [SerializeField] private BoxCollider[] additionalBombPlates = new BoxCollider[0];
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameOverController gameOverController;
    [SerializeField, Min(0f)] private float plateExtraHeight = 0.5f;
    [SerializeField, Min(1f)] private float spawnHeight = 5f;
    [UnityEngine.Serialization.FormerlySerializedAs("restartDelay")]
    [SerializeField, Min(0.1f)] private float bombFallWait = 2f;
    [Header("Bomb Drop Audio")]
    [SerializeField] private AudioClip bombDropSound;
    private AudioSource bombDropAudio;
    [Header("Sliding Door")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    [SerializeField, Min(0f)] private float doorSlideDistance = 2.1f;
    [SerializeField, Min(0f)] private float doorOpenDuration = 0.7f;
    [SerializeField] private AudioClip doorOpenSound;
    private AudioSource doorAudio;
    private bool appeared, cleared, failed;

    private void Awake()
    {
        if (entryArea == null) entryArea = GetComponent<BoxCollider>();
        if (entryArea == null || trapRoot == null || trapRoot == gameObject || transform.IsChildOf(trapRoot.transform))
        {
            Debug.LogError("Assign a separate Trap3-4 and entry collider to StageThreeFinalTrap.", this);
            enabled = false;
            return;
        }
        entryArea.isTrigger = true;
        trapRoot.SetActive(false);
    }

    private void Start()
    {
        if (gameOverController == null)
            foreach (var item in FindObjectsByType<GameOverController>())
                if (item.gameObject.scene == gameObject.scene) { gameOverController = item; break; }
        if (bombPlate == null)
            foreach (var item in FindObjectsByType<BoxCollider>())
                if (item.gameObject.scene == gameObject.scene && item.name == "PressurePlate3-4")
                { bombPlate = item; break; }
        if (safePlate == null || bombPlate == null || bombPrefab == null || gameOverController == null)
            Debug.LogError("StageThreeFinalTrap: assign Safe Plate, Bomb Plate, Bomb Prefab and Game Over Controller.", this);
    }

    private void FixedUpdate()
    {
        if (failed) return;
        if (!appeared && !cleared && PlayerInside(entryArea, 0f) != null)
        {
            appeared = true;
            trapRoot.SetActive(true);
            GameAudio.Play(GameSound.TrapAppear);
        }
        if (appeared && !cleared && PlayerInside(safePlate, plateExtraHeight) != null)
        {
            cleared = true;
            GameAudio.Play(GameSound.PlatePress);
            StartCoroutine(OpenDoors());
        }
        Transform player = PlayerInside(bombPlate, plateExtraHeight);
        if (player == null && additionalBombPlates != null)
            foreach (BoxCollider plate in additionalBombPlates)
            {
                player = PlayerInside(plate, plateExtraHeight);
                if (player != null) break;
            }
        if (player != null && bombPrefab != null && gameOverController != null)
        {
            failed = true;
            StartCoroutine(DropAndGameOver(player));
        }
    }

    private IEnumerator OpenDoors()
    {
        if (doorOpenSound != null)
        {
            // Keep the source on the controller so hiding the doors does not cut it off.
            doorAudio = gameObject.AddComponent<AudioSource>();
            doorAudio.playOnAwake = false;
            doorAudio.loop = false;
            doorAudio.spatialBlend = 0f;
            doorAudio.volume = GameAudio.EffectsVolume;
            doorAudio.clip = doorOpenSound;
            doorAudio.Play();
        }
        Vector3 leftStart = leftDoor != null ? leftDoor.localPosition : Vector3.zero;
        Vector3 rightStart = rightDoor != null ? rightDoor.localPosition : Vector3.zero;
        float elapsed = 0f;
        while (elapsed < doorOpenDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / doorOpenDuration));
            if (leftDoor != null) leftDoor.localPosition = leftStart + Vector3.left * doorSlideDistance * t;
            if (rightDoor != null) rightDoor.localPosition = rightStart + Vector3.right * doorSlideDistance * t;
            yield return null;
        }
        if (leftDoor != null)
        {
            leftDoor.localPosition = leftStart + Vector3.left * doorSlideDistance;
            leftDoor.gameObject.SetActive(false);
        }
        if (rightDoor != null)
        {
            rightDoor.localPosition = rightStart + Vector3.right * doorSlideDistance;
            rightDoor.gameObject.SetActive(false);
        }
        // Only the assigned leaves disappear; a separate frame can remain visible.
        if (leftDoor == null && rightDoor == null) trapRoot.SetActive(false);
    }

    private void Update()
    {
        if (doorAudio != null) doorAudio.volume = GameAudio.EffectsVolume;
        if (bombDropAudio != null) bombDropAudio.volume = GameAudio.EffectsVolume;
    }

    private Transform PlayerInside(BoxCollider area, float extraHeight)
    {
        if (area == null || !area.enabled || !area.gameObject.activeInHierarchy) return null;
        Vector3 scale = area.transform.lossyScale;
        scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        Vector3 size = Vector3.Scale(area.size, scale);
        size.y += extraHeight;
        Vector3 center = area.transform.TransformPoint(area.center) + Vector3.up * extraHeight * 0.5f;
        foreach (var hit in Physics.OverlapBox(center, size * 0.5f, area.transform.rotation, ~0, QueryTriggerInteraction.Ignore))
            for (Transform t = hit.transform; t != null; t = t.parent)
                if (t.CompareTag("Player")) return t;
        return null;
    }

    private IEnumerator DropAndGameOver(Transform player)
    {
        gameOverController.LockPlayer();
        // Prevent a simultaneous boss catch from replacing this scripted failure.
        foreach (var chase in FindObjectsByType<StageThreeRobotChase>())
            if (chase.gameObject.scene == gameObject.scene) chase.enabled = false;
        foreach (var camera in FindObjectsByType<MouseOrbitCamera>())
            if (camera.gameObject.scene == gameObject.scene) camera.enabled = false;
        foreach (var menu in FindObjectsByType<SettingsMenu>())
            if (menu.gameObject.scene == gameObject.scene) menu.enabled = false;
        GameAudio.Play(GameSound.PlatePress);
        GameObject bomb = Instantiate(bombPrefab, player.position + Vector3.up * spawnHeight, Quaternion.identity);
        bomb.name = "Bomb (Trap3-4)";
        bomb.SetActive(true);
        Rigidbody body = bomb.GetComponent<Rigidbody>();
        if (body == null) body = bomb.AddComponent<Rigidbody>();
        body.isKinematic = false;
        body.useGravity = true;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        if (bombDropSound != null)
        {
            bombDropAudio = gameObject.AddComponent<AudioSource>();
            bombDropAudio.playOnAwake = false;
            bombDropAudio.loop = false;
            bombDropAudio.spatialBlend = 0f;
            bombDropAudio.volume = GameAudio.EffectsVolume;
            bombDropAudio.clip = bombDropSound;
            bombDropAudio.Play();
        }
        yield return new WaitForSeconds(bombFallWait);
        gameOverController.TriggerGameOver();
    }
}
