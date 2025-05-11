using UnityEngine;
using UnityEngine.UI;

namespace SaveSettingsSystem
{

    /// <summary>
    ///控制游戏中音频设置的 UI 元素。
    /// </summary>
    /// <remarks>
    ///此控制器管理 UI 滑块和音频设置系统之间的连接。
    ///它处理滑块值的初始化，在设置更改时更新 UI，
    ///并将用户交互传播到设置系统。
    /// </remarks>
    public class SettingsUIController : MonoBehaviour
    {
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        private void Start()
        {
            // 绑定UI
            masterVolumeSlider.value = SimpleAudioSettings.MasterVolume;
            bgmVolumeSlider.value = SimpleAudioSettings.BGMVolume;
            sfxVolumeSlider.value = SimpleAudioSettings.SFXVolume;

            // 添加监听
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            // 监听设置变更
            SimpleSettingsManager.Instance.SettingsChanged += UpdateUI;
        }

        private void OnDestroy()
        {
            // 移除监听
            SimpleSettingsManager.Instance.SettingsChanged -= UpdateUI;
        }

        // UI更新
        private void UpdateUI()
        {
            masterVolumeSlider.value = SimpleAudioSettings.MasterVolume;
            bgmVolumeSlider.value = SimpleAudioSettings.BGMVolume;
            sfxVolumeSlider.value = SimpleAudioSettings.SFXVolume;
        }

        // 设置更新
        private void OnMasterVolumeChanged(float value) => SimpleAudioSettings.MasterVolume = value;
        private void OnBGMVolumeChanged(float value) => SimpleAudioSettings.BGMVolume = value;
        private void OnSFXVolumeChanged(float value) => SimpleAudioSettings.SFXVolume = value;

        // 重置按钮
        public void ResetSettings()
        {
            SimpleSettingsManager.Instance.ResetToDefaults();
        }
    }
}