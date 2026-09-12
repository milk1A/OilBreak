using UnityEngine;

[CreateAssetMenu(menuName = "OILBREAK/Audio Config")]
public class GameAudioConfig : ScriptableObject
{
    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip clip;
    }

    public SceneMusic[] scenes;
    [Range(0f, 1f)] public float musicVolume = 0.35f;
    [Range(0f, 1f)] public float effectsVolume = 0.8f;
    public AudioClip stageStart;
    public AudioClip pickup;
    public AudioClip putDown;
    public AudioClip portal;
    public AudioClip trapAppear;
    public AudioClip platePress;
    public AudioClip trapBlocked;
}
