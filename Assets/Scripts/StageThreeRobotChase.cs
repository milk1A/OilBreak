using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.Playables;

// Scene-local chase for the flat F_Stage_3 map. No changes to shared boss/puzzle logic.
public class StageThreeRobotChase : MonoBehaviour
{
    [SerializeField] private Transform robot;
    [SerializeField] private Transform player;
    [SerializeField] private AnimationClip runClip;
    [SerializeField] private AnimationClip idleClip;
    [SerializeField, Min(0.1f)] private float speed = 3.5f;
    [SerializeField, Min(0.1f)] private float radius = 0.55f;
    [SerializeField] private LayerMask wallLayers = 1 << 3;
    [SerializeField] private GameObject watchedTrap;
    [SerializeField, Min(0f)] private float chaseDelay = 1f;
    [Header("Game Over")]
    [SerializeField] private GameOverController gameOverController;
    [SerializeField, Min(0.1f)] private float catchDistance = 1.2f;
    [SerializeField, Min(0.1f)] private float catchHeight = 3f;
    private bool caught;
    [Header("Chase Audio")]
    [SerializeField] private AudioClip bossRunStart;
    [SerializeField] private AudioClip runningSound;
    [SerializeField] private AudioClip caughtAlarm;
    [SerializeField, Min(0f)] private float runningSoundGap = 0.15f;
    private AudioSource chaseAudio;
    private bool starting, audioPaused, moving, restarting;
    private float nextRunningSound;

    private bool trapWasActive, startRequested;

    private float groundY, nextPathTime;
    private bool chasing, ready;
    private NavMeshData data;
    private NavMeshDataInstance meshInstance;
    private int agentType = -1;
    private NavMeshQueryFilter filter;
    private NavMeshAgent navigationAgent;
    private GameObject navigationObject;
    [SerializeField] private string navigationStatus = "Waiting for chase";
    private bool warnedUnreachable;
    private PlayableGraph animationGraph;
    private AnimationClipPlayable animationClip;
    private AnimationClip activeClip;
    private Animator animator;
    private bool originalRootMotion;

    private void Start()
    {
        if (robot == null) { enabled = false; return; }
        if (player == null)
        {
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
        if (player == null || runClip == null)
        {
            Debug.LogError("Robot chase requires a player and a running clip.", this);
            enabled = false;
            return;
        }
        groundY = robot.position.y;
        if (gameOverController == null)
            foreach (var controller in FindObjectsByType<GameOverController>())
                if (controller.gameObject.scene == gameObject.scene)
                { gameOverController = controller; break; }
        if (gameOverController != null) gameOverController.RestartRequested += StopChaseAudio;
        chaseAudio = gameObject.AddComponent<AudioSource>();
        chaseAudio.playOnAwake = false;
        chaseAudio.spatialBlend = 0f;
        trapWasActive = watchedTrap != null && watchedTrap.activeInHierarchy;
        animator = robot.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            originalRootMotion = animator.applyRootMotion;
            animator.applyRootMotion = false;
            animationGraph = PlayableGraph.Create("Robot chase animation");
            var output = AnimationPlayableOutput.Create(animationGraph, "Robot", animator);
            activeClip = idleClip != null ? idleClip : runClip;
            animationClip = AnimationClipPlayable.Create(animationGraph, activeClip);
            output.SetSourcePlayable(animationClip);
            animationGraph.Play();
        }
        ready = true;
    }

    private void LateUpdate()
    {
        // The imported robot clips are not marked as looping. Loop this instance only.
        if (animationGraph.IsValid() && activeClip != null && activeClip.length > 0f &&
            animationClip.GetTime() >= activeClip.length)
            animationClip.SetTime(animationClip.GetTime() % activeClip.length);
    }

    private void Update()
    {
        UpdateChaseAudio();
        if (!ready || caught || player == null || robot == null || Time.timeScale == 0f) return;
        moving = false;
        if (starting) return;
        if (chasing && navigationAgent != null && navigationAgent.isOnNavMesh)
            robot.position = navigationAgent.nextPosition;
        if (chasing && TryCatchPlayer()) return;
        if (!chasing)
        {
            if (!startRequested && watchedTrap != null)
            {
                bool active = watchedTrap.activeInHierarchy;
                if (trapWasActive && !active)
                {
                    startRequested = true;
                    StartCoroutine(PrepareChase());
                }
                trapWasActive = active;
            }
            return;
        }
        UpdateNavigation();
    }

    private void UpdateNavigation()
    {
        if (navigationAgent == null || !navigationAgent.isOnNavMesh)
        {
            navigationStatus = "Boss is not on the navigation mesh";
            return;
        }
        robot.position = navigationAgent.nextPosition;
        navigationAgent.speed = speed;
        moving = navigationAgent.velocity.sqrMagnitude > 0.001f;
        Vector3 facing = navigationAgent.desiredVelocity;
        facing.y = 0f;
        if (facing.sqrMagnitude > 0.001f)
            robot.rotation = Quaternion.RotateTowards(robot.rotation, Quaternion.LookRotation(facing), 360f * Time.deltaTime);
        if (Time.time < nextPathTime) return;
        nextPathTime = Time.time + 0.25f;
        Vector3 destination = new Vector3(player.position.x, groundY, player.position.z);
        NavMeshPath best = null;
        float bestDistance = float.PositiveInfinity;
        // SamplePosition alone can pick the other side of a thin wall. Search nearby
        // reachable points on the player's side instead of accepting a dead-end path.
        for (int i = 0; i < 25; i++)
        {
            float angle = ((i - 1) % 8) * Mathf.PI / 4f;
            float distance = i == 0 ? 0f : 0.5f * (1 + (i - 1) / 8);
            Vector3 probe = destination + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
            if (!NavMesh.SamplePosition(probe, out var target, 0.6f, filter)) continue;
            if (WallBetween(destination + Vector3.up, target.position + Vector3.up)) continue;
            float score = (target.position - destination).sqrMagnitude;
            if (score >= bestDistance) continue;
            var candidate = new NavMeshPath();
            if (!navigationAgent.CalculatePath(target.position, candidate) ||
                candidate.status != NavMeshPathStatus.PathComplete) continue;
            best = candidate;
            bestDistance = score;
            if (score < 0.01f) break;
        }
        if (best != null && navigationAgent.SetPath(best))
        {
            navigationStatus = "Following complete path";
            warnedUnreachable = false;
        }
        else
        {
            navigationStatus = "No connected route to player: check Block colliders and corridor width";
            if (!warnedUnreachable)
            {
                Debug.LogWarning("Robot chase: no complete route to the player. Check Block wall colliders and passage width (boss diameter = " + (radius * 2f) + ").", this);
                warnedUnreachable = true;
            }
        }
    }

    private bool WallBetween(Vector3 from, Vector3 to)
    {
        foreach (var hit in Physics.RaycastAll(from, to - from, Vector3.Distance(from, to), wallLayers, QueryTriggerInteraction.Ignore))
        {
            bool ignored = false;
            for (var t = hit.transform; t != null; t = t.parent)
                if (t == robot || t == player || t.CompareTag("Obstacle") || t.CompareTag("PickupBox")) ignored = true;
            if (!ignored) return true;
        }
        return false;
    }
    private bool TryCatchPlayer()
    {
        Vector3 delta = player.position - robot.position;
        if (Mathf.Abs(delta.y) > catchHeight) return false;
        delta.y = 0f;
        if (delta.sqrMagnitude > catchDistance * catchDistance) return false;
        // Do not catch a nearby player through a wall, even in a narrow corridor.
        Vector3 from = robot.position + Vector3.up;
        Vector3 to = player.position + Vector3.up;
        foreach (var hit in Physics.RaycastAll(from, to - from, Vector3.Distance(from, to), wallLayers, QueryTriggerInteraction.Ignore))
        {
            bool ignored = false;
            for (var t = hit.transform; t != null; t = t.parent)
                if (t == robot || t == player || t.CompareTag("Obstacle") || t.CompareTag("PickupBox")) ignored = true;
            if (!ignored) return false;
        }
        if (gameOverController == null || !gameOverController.isActiveAndEnabled)
        {
            Debug.LogError("Assign an active GameOverController to StageThreeRobotChase.", this);
            enabled = false;
            return true;
        }
        caught = true;
        chasing = false;
        if (navigationAgent != null && navigationAgent.isOnNavMesh) navigationAgent.isStopped = true;
        moving = false;
        chaseAudio.Stop();
        GameAudio.PlayBossCaught(caughtAlarm);
        if (animationGraph.IsValid()) animationClip.SetSpeed(0);
        // Keep the cursor available for the restart button.
        foreach (var camera in FindObjectsByType<MouseOrbitCamera>())
            if (camera.gameObject.scene == gameObject.scene) camera.enabled = false;
        foreach (var menu in FindObjectsByType<SettingsMenu>())
            if (menu.gameObject.scene == gameObject.scene) menu.enabled = false;
        gameOverController.TriggerGameOver();
        return true;
    }

    private IEnumerator PrepareChase()
    {
        starting = true;
        // The disappearance starts the countdown; audio length no longer delays movement.
        if (bossRunStart != null)
        {
            chaseAudio.Stop();
            chaseAudio.loop = false;
            chaseAudio.clip = bossRunStart;
            chaseAudio.volume = GameAudio.EffectsVolume;
            chaseAudio.Play();
        }
        yield return new WaitForSeconds(chaseDelay);
        starting = false;
        BeginChase();
    }

    private void UpdateChaseAudio()
    {
        if (chaseAudio == null || restarting) return;
        chaseAudio.volume = GameAudio.EffectsVolume;
        if (Time.timeScale == 0f)
        {
            if (!audioPaused && chaseAudio.isPlaying)
            {
                chaseAudio.Pause();
                audioPaused = true;
            }
            return;
        }
        if (audioPaused)
        {
            chaseAudio.UnPause();
            audioPaused = false;
        }
        // One source, one complete clip at a time, with a small gap between repeats.
        if (chasing && moving && !caught && runningSound != null &&
            !chaseAudio.isPlaying && Time.time >= nextRunningSound)
        {
            chaseAudio.clip = runningSound;
            chaseAudio.loop = false;
            chaseAudio.Play();
            nextRunningSound = Time.time + runningSound.length + runningSoundGap;
        }
    }

    private void StopChaseAudio()
    {
        restarting = true;
        GameAudio.StopBossCaught();
        if (chaseAudio != null) chaseAudio.Stop();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (chaseAudio != null) chaseAudio.Stop();
        if (navigationAgent != null && navigationAgent.isOnNavMesh) navigationAgent.isStopped = true;
    }

    private void BeginChase()
    {
        if (!BuildWallNavigation()) return;
        // Use Unity's corridor following instead of moving a Transform along mesh edges.
        // A separate unscaled object prevents the robot's animation/scale from moving the agent.
        navigationObject = new GameObject("Robot1 Navigation");
        navigationObject.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(navigationObject, gameObject.scene);
        navigationObject.transform.position = robot.position;
        navigationAgent = navigationObject.AddComponent<NavMeshAgent>();
        navigationAgent.agentTypeID = agentType;
        navigationAgent.areaMask = 1;
        navigationAgent.radius = radius;
        navigationAgent.height = 2f;
        navigationAgent.speed = speed;
        navigationAgent.acceleration = 30f;
        navigationAgent.stoppingDistance = 0f;
        navigationAgent.autoBraking = false;
        navigationAgent.autoRepath = true;
        navigationAgent.updateRotation = false;
        navigationAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        navigationObject.SetActive(true);
        if (!navigationAgent.Warp(robot.position))
        {
            Debug.LogError("Robot navigation agent could not enter the generated mesh.", this);
            Destroy(navigationObject);
            CleanupNavigation();
            return;
        }
        chasing = true;
        if (animationGraph.IsValid())
        {
            animationGraph.GetOutput(0).SetSourcePlayable(Playable.Null);
            animationClip.Destroy();
            activeClip = runClip;
            animationClip = AnimationClipPlayable.Create(animationGraph, runClip);
            animationGraph.GetOutput(0).SetSourcePlayable(animationClip);
        }
    }

    private bool BuildWallNavigation()
    {
        var sources = new List<NavMeshBuildSource>();
        var bounds = new Bounds(robot.position, Vector3.one * 4f);
        bounds.Encapsulate(player.position);
        foreach (var collider in FindObjectsByType<Collider>())
        {
            if (collider.gameObject.scene != gameObject.scene || !collider.enabled || collider.isTrigger ||
                (wallLayers.value & (1 << collider.gameObject.layer)) == 0) continue;
            bool obstacle = false;
            for (var t = collider.transform; t != null; t = t.parent)
                if (t.CompareTag("Obstacle") || t.CompareTag("PickupBox") || t == robot || t == player) obstacle = true;
            if (obstacle) continue;
            bounds.Encapsulate(collider.bounds);
            var source = new NavMeshBuildSource {
                shape = NavMeshBuildSourceShape.Box, area = 1,
                transform = Matrix4x4.TRS(collider.bounds.center, Quaternion.identity, Vector3.one),
                size = collider.bounds.size
            };
            if (collider is BoxCollider box)
            {
                source.transform = box.transform.localToWorldMatrix * Matrix4x4.Translate(box.center);
                source.size = box.size;
            }
            else if (collider is MeshCollider mesh && mesh.sharedMesh != null)
            {
                source.shape = NavMeshBuildSourceShape.Mesh;
                source.transform = mesh.transform.localToWorldMatrix;
                source.sourceObject = mesh.sharedMesh;
            }
            sources.Add(source);
        }
        bounds.Expand(4f);
        sources.Add(new NavMeshBuildSource {
            shape = NavMeshBuildSourceShape.Box, area = 0,
            transform = Matrix4x4.TRS(new Vector3(bounds.center.x, groundY - 0.1f, bounds.center.z), Quaternion.identity, Vector3.one),
            size = new Vector3(bounds.size.x, 0.2f, bounds.size.z)
        });
        var settings = NavMesh.CreateSettings();
        agentType = settings.agentTypeID;
        settings.agentRadius = radius;
        settings.agentHeight = 2f;
        settings.agentClimb = 0.1f;
        settings.overrideVoxelSize = true;
        settings.voxelSize = 0.1f;
        data = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);
        if (data != null) meshInstance = NavMesh.AddNavMeshData(data);
        filter = new NavMeshQueryFilter { agentTypeID = agentType, areaMask = 1 };
        if (data != null && NavMesh.SamplePosition(robot.position, out var start, 1f, filter))
        {
            robot.position = start.position;
            return true;
        }
        Debug.LogError("Robot chase could not find a starting path. Move Robot1 away from Block walls.", this);
        CleanupNavigation();
        return false;
    }

    private void CleanupNavigation()
    {
        if (navigationAgent != null) navigationAgent.enabled = false;
        if (navigationObject != null) Destroy(navigationObject);
        if (meshInstance.valid) meshInstance.Remove();
        if (data != null) Destroy(data);
        if (agentType != -1) NavMesh.RemoveSettings(agentType);
        agentType = -1;
    }

    private void OnDestroy()
    {
        if (gameOverController != null) gameOverController.RestartRequested -= StopChaseAudio;
        if (animationGraph.IsValid()) animationGraph.Destroy();
        if (animator != null) animator.applyRootMotion = originalRootMotion;
        CleanupNavigation();
    }
}
