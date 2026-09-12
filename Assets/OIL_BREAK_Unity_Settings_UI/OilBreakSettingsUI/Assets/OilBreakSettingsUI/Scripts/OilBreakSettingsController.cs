using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace OilBreak.UI
{
    public class OilBreakSettingsController : MonoBehaviour
    {
        [Header("UI")]
        public Slider masterSlider;
        public Slider bgmSlider;
        public Slider sfxSlider;
        public Slider sensitivitySlider;
        public Toggle fullscreenToggle;
        public GameObject settingsPanel;

        [Header("Optional Audio Mixer")]
        public AudioMixer audioMixer;
        public string masterParameter = "MasterVolume";
        public string bgmParameter = "BGMVolume";
        public string sfxParameter = "SFXVolume";

        public static float MouseSensitivity { get; private set; } = 1f;

        private const string MasterKey = "OB_Master";
        private const string BgmKey = "OB_BGM";
        private const string SfxKey = "OB_SFX";
        private const string SensitivityKey = "OB_Sensitivity";
        private const string FullscreenKey = "OB_Fullscreen";

        private void Awake()
        {
            LoadSettings();
            ApplySettings();
        }

        public void LoadSettings()
        {
            masterSlider.value = PlayerPrefs.GetFloat(MasterKey, 0.8f);
            bgmSlider.value = PlayerPrefs.GetFloat(BgmKey, 0.7f);
            sfxSlider.value = PlayerPrefs.GetFloat(SfxKey, 0.8f);
            sensitivitySlider.value = PlayerPrefs.GetFloat(SensitivityKey, 1f);
            fullscreenToggle.isOn = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        }

        public void ApplySettings()
        {
            SetMixerVolume(masterParameter, masterSlider.value);
            SetMixerVolume(bgmParameter, bgmSlider.value);
            SetMixerVolume(sfxParameter, sfxSlider.value);
            AudioListener.volume = masterSlider.value;
            MouseSensitivity = sensitivitySlider.value;
            Screen.fullScreen = fullscreenToggle.isOn;

            PlayerPrefs.SetFloat(MasterKey, masterSlider.value);
            PlayerPrefs.SetFloat(BgmKey, bgmSlider.value);
            PlayerPrefs.SetFloat(SfxKey, sfxSlider.value);
            PlayerPrefs.SetFloat(SensitivityKey, sensitivitySlider.value);
            PlayerPrefs.SetInt(FullscreenKey, fullscreenToggle.isOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Open() => settingsPanel.SetActive(true);
        public void Close() => settingsPanel.SetActive(false);

        private void SetMixerVolume(string parameter, float linearValue)
        {
            if (audioMixer == null || string.IsNullOrEmpty(parameter)) return;
            audioMixer.SetFloat(parameter, Mathf.Log10(Mathf.Max(linearValue, 0.0001f)) * 20f);
        }
    }
}
