using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameSound { Pickup, PutDown, Portal, TrapAppear, PlatePress, TrapBlocked }

public class GameAudio : MonoBehaviour
{
    private static GameAudio instance;
    private GameAudioConfig config;
    private AudioSource music;
    private AudioSource effects;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (instance != null) return;
        new GameObject("Game Audio").AddComponent<GameAudio>();
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        config = Resources.Load<GameAudioConfig>("GameAudioConfig");
        music = gameObject.AddComponent<AudioSource>();
        effects = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = effects.playOnAwake = false;
        music.spatialBlend = effects.spatialBlend = 0f;
        music.loop = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (config == null) Debug.LogError("Missing Resources/GameAudioConfig.asset", this);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;
        music.Stop();
        StopAllCoroutines();
        StartCoroutine(StartSceneAudio(scene));
    }

    private IEnumerator StartSceneAudio(Scene scene)
    {
        // Let the scene's Start methods and camera setup finish first.
        yield return null;
        if (config == null || !scene.isLoaded) yield break;
        if (config.stageStart != null) effects.PlayOneShot(config.stageStart, config.effectsVolume);
        if (config.scenes == null) yield break;
        foreach (var entry in config.scenes)
        {
            if (entry == null || entry.sceneName != scene.name || entry.clip == null) continue;
            music.clip = entry.clip;
            music.volume = config.musicVolume;
            music.Play();
            break;
        }
    }

    public static void Play(GameSound sound)
    {
        if (instance == null || instance.config == null) return;
        var c = instance.config;
        AudioClip clip = null;
        switch (sound)
        {
            case GameSound.Pickup: clip = c.pickup; break;
            case GameSound.PutDown: clip = c.putDown; break;
            case GameSound.Portal: clip = c.portal; break;
            case GameSound.TrapAppear: clip = c.trapAppear; break;
            case GameSound.PlatePress: clip = c.platePress; break;
            case GameSound.TrapBlocked: clip = c.trapBlocked; break;
        }
        // Persistent source lets portal audio finish after the scene changes.
        if (clip != null) instance.effects.PlayOneShot(clip, c.effectsVolume);
    }

    public static void PlayBlockedIfTrap(Collider surface)
    {
        if (surface == null) return;
        for (Transform t = surface.transform; t != null; t = t.parent)
        {
            if (t.CompareTag("Obstacle"))
            {
                Play(GameSound.TrapBlocked);
                return;
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }
}
