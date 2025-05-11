using UnityEngine;


namespace SaveSettingsSystem
{

    /// <summary>
    /// 音频设置的便捷访问类
    /// </summary>
    public static class SimpleAudioSettings
    {
        private const string MASTER_VOLUME_KEY = "audio.master_volume";
        private const string BGM_VOLUME_KEY = "audio.bgm_volume";
        private const string SFX_VOLUME_KEY = "audio.sfx_volume";

        // 静态构造函数注册默认值
        static SimpleAudioSettings()
        {
            if (SimpleSettingsManager.Instance != null)
            {
                SimpleSettingsManager.Instance.RegisterSetting(MASTER_VOLUME_KEY, 1.0f);
                SimpleSettingsManager.Instance.RegisterSetting(BGM_VOLUME_KEY, 1.0f);
                SimpleSettingsManager.Instance.RegisterSetting(SFX_VOLUME_KEY, 1.0f);
            }
        }

        // 主音量
        public static float MasterVolume
        {
            get => SimpleSettingsManager.Instance.GetSetting<float>(MASTER_VOLUME_KEY);
            set => SimpleSettingsManager.Instance.SetSetting(MASTER_VOLUME_KEY, Mathf.Clamp01(value));
        }

        // 背景音乐音量
        public static float BGMVolume
        {
            get => SimpleSettingsManager.Instance.GetSetting<float>(BGM_VOLUME_KEY);
            set => SimpleSettingsManager.Instance.SetSetting(BGM_VOLUME_KEY, Mathf.Clamp01(value));
        }

        // 音效音量
        public static float SFXVolume
        {
            get => SimpleSettingsManager.Instance.GetSetting<float>(SFX_VOLUME_KEY);
            set => SimpleSettingsManager.Instance.SetSetting(SFX_VOLUME_KEY, Mathf.Clamp01(value));
        }

        // 获取实际音量（考虑主音量）
        public static float GetActualBGMVolume() => MasterVolume * BGMVolume;
        public static float GetActualSFXVolume() => MasterVolume * SFXVolume;
    }
}