using System;
using UnityEngine;
using SaveSettingsSystem;

    /// <summary>
    /// AudioSettings 类继承自 BaseSettings，用于管理音频设置数据。
    /// <para>
    /// 1. 继承基类构造器，通过 <c>: BaseSettings(Anysettings,AnyData,AnySettingSO")</c> 调用 BaseSettings 基类的构造函数，
    ///    需要传递两个参数：
    ///    - <c>settings</c>：AudioSettingsSO 实例，包含音频设置的 ScriptableObject 数据；
    ///    - <c>"AudioSettings"</c>：用于 PlayerPrefs 存储的默认键名。
    /// </para>
    /// <para>
    /// 2. 确保初始化：创建 AudioSettings 实例时必须提供一个 AudioSettingsSO 对象，确保设置系统有可用的数据容器。
    /// </para>
    /// <para>
    /// 3. 固定存储键：默认构造函数使用固定键 "AudioSettings"，确保数据始终存储在同一位置，
    ///    避免因使用默认类名作为键而导致存储不一致的问题。
    /// </para>
    /// <para>
    /// 4. 同时也提供了一个允许自定义键名的构造函数，调用者可以通过传入自定义键来灵活设置存储键。
    /// </para>
    /// </summary>
    /// <param name="settings">AudioSettingsSO 实例，包含音频设置的数据</param>
[Serializable]
public class AudioSettings : BaseSettings<AudioSettings.AudioVolumeData, AudioSettingsSO>
{
    /// <summary>
    /// AudioVolumeData是一个可序列化的类，用于存储音量设置的数据
    /// </summary>
    [Serializable]
    public class AudioVolumeData
    {
        public float masterVolume = 1f;
        public float bgmVolume = 1f;
        public float sfxVolume = 1f;
    }

    /// <summary>
    /// AudioSettings 类继承自 BaseSettings，用于管理音频设置数据。
    /// <para>
    /// 1. 继承基类构造器，通过 <c>: BaseSettings(AnySettingSO,defaultKey)</c> 调用 BaseSettings 基类的构造函数，
    ///    需要传递两个参数：
    ///    - <c>settings</c>：AudioSettingsSO 实例，包含音频设置的 ScriptableObject 数据；
    ///    - <c>"AudioSettings"</c>：用于 PlayerPrefs 存储的默认键名。
    /// </para>
    /// <para>
    /// 2. 确保初始化：创建 AudioSettings 实例时必须提供一个 AudioSettingsSO 对象，确保设置系统有可用的数据容器。
    /// </para>
    /// <para>
    /// 3. 固定存储键：默认构造函数使用固定键 "AudioSettings"，确保数据始终存储在同一位置，
    ///    避免因使用默认类名作为键而导致存储不一致的问题。
    /// </para>
    /// <para>
    /// 4. 同时也提供了一个允许自定义键名的构造函数，调用者可以通过传入自定义键来灵活设置存储键。
    /// </para>
    /// </summary>
    /// <param name="settings">AudioSettingsSO 实例，包含音频设置的数据</param>
    public AudioSettings(AudioSettingsSO settingsSO) : base(settingsSO, "AudioSettings")
    {
    }


    // 音量变化的特定事件，与基类的OnSettingsChanged不同
    public event EventHandler OnVolumeChanged;

    #region 音量控制逻辑

    /// <summary>
    /// Get的是当前的音量值，Set是设置新的音量值
    /// </summary>
    /// <value></value>
    public float MasterVolume
    {
        get => settingsSO.masterVolume;
        set
        {
            float clampedValue = Mathf.Clamp01(value);
            if (!Mathf.Approximately(settingsSO.masterVolume, clampedValue))
            {
                settingsSO.masterVolume = clampedValue;
                VolumeChanged(); // 使用统一的事件通知方法
            }
        }
    }

    /// <summary>
    /// Get的是当前的音量值，Set是设置新的音量值
    /// </summary>
    /// <value></value>
    public float BGMVolume
    {
        get => settingsSO.bgmVolume;
        set
        {
            float clampedValue = Mathf.Clamp01(value);
            if (!Mathf.Approximately(settingsSO.bgmVolume, clampedValue))
            {
                settingsSO.bgmVolume = clampedValue;
                VolumeChanged(); // 使用统一的事件通知方法
            }
        }
    }

    /// <summary>
    /// Get的是当前的音量值，Set是设置新的音量值
    /// </summary>
    /// <value></value>
    public float SFXVolume
    {
        get => settingsSO.sfxVolume;
        set
        {
            float clampedValue = Mathf.Clamp01(value);
            if (!Mathf.Approximately(settingsSO.sfxVolume, clampedValue))
            {
                settingsSO.sfxVolume = clampedValue;
                VolumeChanged(); // 使用统一的事件通知方法
            }
        }
    }

    /// <summary>
    /// 统一的音量改变事件通知方法
    /// </summary>
    private void VolumeChanged()
    {
        // 先触发特定事件，再触发通用事件
        OnVolumeChanged?.Invoke(this, EventArgs.Empty);
        NotifySettingsChanged();
    }

    /// <summary>
    /// 考虑主音量的影响计算最终音量的值
    /// </summary>
    /// <returns></returns>
    public float GetActualBGMVolume()
    {
        return Mathf.Clamp01(MasterVolume * BGMVolume);
    }

    /// <summary>
    /// 考虑主音量的影响计算最终音量的值
    /// </summary>
    /// <returns></returns>
    public float GetActualSFXVolume()
    {
        return Mathf.Clamp01(MasterVolume * SFXVolume);
    }

    #endregion

    #region 接口实现

    /// <summary>
    /// 重置所有音量到默认值，默认值音量都为1f.
    /// </summary>
    public override void ResetToDefault()
    {
        // 直接设置值，然后统一触发事件
        settingsSO.masterVolume = 1f;
        settingsSO.bgmVolume = 1f;
        settingsSO.sfxVolume = 1f;
        VolumeChanged();
    }

    /// <summary>
    /// 重写了BaseSettings从当前 AudioSettings 提取数据，封装成 AudioVolumeData
    /// </summary>
    /// <returns></returns>
    protected override AudioVolumeData GetDataFromSettings()
    {
        return new AudioVolumeData
        {
            masterVolume = MasterVolume,
            bgmVolume = BGMVolume,
            sfxVolume = SFXVolume
        };
    }


    /// <summary>
    /// 将 AudioVolumeData 的数据应用到 settingsSO（音量设置 ScriptableObject）中，同时避免不必要的事件触发
    /// 重写了 BaseSettings 的 ApplyDataToSettings 方法
    /// </summary>
    /// <param name="data">需要存储的数据</param>
    protected override void ApplyDataToSettings(AudioVolumeData data)
    {
        // 这里避免触发多次事件，先记录旧状态
        bool changed = false;
        changed |= !Mathf.Approximately(settingsSO.masterVolume, data.masterVolume);
        changed |= !Mathf.Approximately(settingsSO.bgmVolume, data.bgmVolume);
        changed |= !Mathf.Approximately(settingsSO.sfxVolume, data.sfxVolume);

        // 直接设置值，不通过属性，避免触发多次事件
        settingsSO.masterVolume = data.masterVolume;
        settingsSO.bgmVolume = data.bgmVolume;
        settingsSO.sfxVolume = data.sfxVolume;

        // 如果有修改再触发事件
        if (changed)
        {
            OnVolumeChanged?.Invoke(this, EventArgs.Empty);
            NotifySettingsChanged();
        }
    }

    #endregion
}