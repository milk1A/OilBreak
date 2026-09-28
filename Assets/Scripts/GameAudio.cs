using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameSound { Pickup, PutDown, Portal, TrapAppear, PlatePress, TrapBlocked, Wheel, CutsceneNext }

public class GameAudio : MonoBehaviour
{
    private static GameAudio instance;
    private GameAudioConfig config;
    private AudioSource music;
    private AudioSource effects;
    private AudioSource pickupEffects;
    private AudioSource cutsceneMusic;
    private AudioSource uiEffects;
    private Object cutsceneOwner;
    public static void BeginCutsceneMusic(Object owner, AudioClip clip)
    {
        if (instance == null || clip == null) return;
        instance.cutsceneOwner = owner;
        instance.music.Pause();
        instance.cutsceneMusic.clip = clip;
        instance.cutsceneMusic.volume = MusicVolume;
        instance.cutsceneMusic.Play();
    }
    public static void EndCutsceneMusic(Object owner)
    {
        if (instance == null || instance.cutsceneOwner != owner) return;
        instance.cutsceneOwner = null;
        instance.cutsceneMusic.Stop();
        instance.music.UnPause();
    }
    public static float MusicVolume => PlayerPrefs.GetFloat("MusicVolume", instance != null && instance.config != null ? instance.config.musicVolume : 0.35f);
    public static float EffectsVolume => PlayerPrefs.GetFloat("EffectsVolume", instance != null && instance.config != null ? instance.config.effectsVolume : 0.8f);
    public static void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", Mathf.Clamp01(value));
        if (instance != null) instance.music.volume = instance.cutsceneMusic.volume = MusicVolume;
    }
    public static void SetEffectsVolume(float value)
    {
        PlayerPrefs.SetFloat("EffectsVolume", Mathf.Clamp01(value));
        if (instance != null) instance.effects.volume = instance.pickupEffects.volume = instance.uiEffects.volume = EffectsVolume;
    }

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
        cutsceneMusic = gameObject.AddComponent<AudioSource>();
        cutsceneMusic.playOnAwake = false;
        cutsceneMusic.loop = true;
        cutsceneMusic.spatialBlend = 0f;
        effects = gameObject.AddComponent<AudioSource>();
        uiEffects = gameObject.AddComponent<AudioSource>();
        uiEffects.playOnAwake = false;
        uiEffects.loop = false;
        uiEffects.spatialBlend = 0f;
        uiEffects.ignoreListenerPause = true;
        uiEffects.volume = EffectsVolume;
        pickupEffects = gameObject.AddComponent<AudioSource>();
        pickupEffects.playOnAwake = false;
        pickupEffects.loop = false;
        pickupEffects.spatialBlend = 0f;
        effects.volume = pickupEffects.volume = EffectsVolume;
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
        if (config.stageStart != null) effects.PlayOneShot(config.stageStart);
        if (config.scenes == null) yield break;
        foreach (var entry in config.scenes)
        {
            if (entry == null || entry.sceneName != scene.name || entry.clip == null) continue;
            music.clip = entry.clip;
            music.volume = MusicVolume;
            music.Play();
            if (cutsceneOwner != null) music.Pause();
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
            case GameSound.Wheel: clip = c.wheel; break;
            case GameSound.CutsceneNext: clip = c.cutsceneNext; break;
            case GameSound.Portal: clip = c.portal; break;
            case GameSound.TrapAppear: clip = c.trapAppear; break;
            case GameSound.PlatePress: clip = c.platePress; break;
            case GameSound.TrapBlocked: clip = c.trapBlocked; break;
        }
        // Persistent source lets portal audio finish after the scene changes.
        if (clip == null)
        {
            if (sound == GameSound.CutsceneNext)
                Debug.LogWarning("Cutscene button audio is missing. Assign GameAudioConfig > Cutscene Next.");
            return;
        }
        if (sound == GameSound.CutsceneNext)
        {
            instance.uiEffects.volume = EffectsVolume;
            if (EffectsVolume <= 0f)
                Debug.LogWarning("Cutscene button sound is muted: increase the Effects Volume slider.");
            instance.uiEffects.PlayOneShot(clip);
            return;
        }
        if (sound == GameSound.Pickup)
        {
            instance.pickupEffects.Stop();
            instance.pickupEffects.clip = clip;
            instance.pickupEffects.Play();
        }
        else instance.effects.PlayOneShot(clip);
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
