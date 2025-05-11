using UnityEngine;
using System;
using System.Collections.Generic;

namespace SaveSettingsSystem
{



    /// <summary>
    /// 简易设置系统管理器，用于小型游戏项目
    /// </summary>
    public class SimpleSettingsManager : MonoBehaviour
    {
        private static SimpleSettingsManager instance;
        public static SimpleSettingsManager Instance => instance;

        // 设置变更事件
        public event Action SettingsChanged;

        // 设置值字典
        private Dictionary<string, object> settings = new Dictionary<string, object>();
        // 默认值字典
        private Dictionary<string, object> defaultSettings = new Dictionary<string, object>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                LoadAllSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // 注册设置及默认值
        public void RegisterSetting<T>(string key, T defaultValue)
        {
            defaultSettings[key] = defaultValue;
            if (!settings.ContainsKey(key))
            {
                settings[key] = defaultValue;
            }
        }

        // 获取设置值
        public T GetSetting<T>(string key)
        {
            if (settings.TryGetValue(key, out object value))
            {
                return (T)value;
            }

            if (defaultSettings.TryGetValue(key, out object defaultValue))
            {
                return (T)defaultValue;
            }

            return default;
        }

        // 更新设置值
        public void SetSetting<T>(string key, T value)
        {
            settings[key] = value;
            SettingsChanged?.Invoke();
        }

        // 加载所有设置
        public void LoadAllSettings()
        {
            foreach (var key in defaultSettings.Keys)
            {
                if (defaultSettings[key] is int)
                    settings[key] = PlayerPrefs.GetInt(key, (int)defaultSettings[key]);
                else if (defaultSettings[key] is float)
                    settings[key] = PlayerPrefs.GetFloat(key, (float)defaultSettings[key]);
                else if (defaultSettings[key] is string)
                    settings[key] = PlayerPrefs.GetString(key, (string)defaultSettings[key]);
                else if (defaultSettings[key] is bool)
                    settings[key] = PlayerPrefs.GetInt(key, (bool)defaultSettings[key] ? 1 : 0) == 1;
            }

            SettingsChanged?.Invoke();
        }

        // 保存所有设置
        public void SaveAllSettings()
        {
            foreach (var entry in settings)
            {
                if (entry.Value is int)
                    PlayerPrefs.SetInt(entry.Key, (int)entry.Value);
                else if (entry.Value is float)
                    PlayerPrefs.SetFloat(entry.Key, (float)entry.Value);
                else if (entry.Value is string)
                    PlayerPrefs.SetString(entry.Key, (string)entry.Value);
                else if (entry.Value is bool)
                    PlayerPrefs.SetInt(entry.Key, (bool)entry.Value ? 1 : 0);
            }

            PlayerPrefs.Save();
        }

        // 重置为默认值
        public void ResetToDefaults()
        {
            settings = new Dictionary<string, object>(defaultSettings);
            SettingsChanged?.Invoke();
        }

        // 应用退出时自动保存
        private void OnApplicationQuit()
        {
            SaveAllSettings();
        }
    }
}