using UnityEngine;
using System;

namespace SaveSettingsSystem
{
    public abstract class BaseSettingsManager<TSettings> : MonoBehaviour, ISaveSettings
        where TSettings : class, ISaveSettings
    {
        protected TSettings settings;

        [Tooltip("是否在Awake时自动注册到AllSettingsManager")]
        [SerializeField] protected bool autoRegister = true;

        public virtual event EventHandler SettingsChanged;

        protected virtual void Awake()
        {
            InitializeSettings();
            Load();

            if (settings != null)
            {
                settings.SettingsChanged += HandleSettingsChanged;
            }

            if (autoRegister && AllSettingsManager.HasInstance)
            {
                AllSettingsManager.Instance.RegisterManager(this);
            }
        }

        protected virtual void HandleSettingsChanged(object sender, EventArgs e)
        {
            SettingsChanged?.Invoke(sender, e);
        }



        /// <summary>
        /// 初始化settings  
        /// </summary>
        protected abstract void InitializeSettings();

        public virtual void Save()
        {
            settings?.Save();
        }

        public virtual void Load()
        {
            settings?.Load();
        }

        public virtual void ResetToDefault()
        {
            settings?.ResetToDefault();
        }

        protected virtual void OnDestroy()
        {
            if (settings != null)
            {
                settings.SettingsChanged -= HandleSettingsChanged;
            }

            if (autoRegister && AllSettingsManager.HasInstance)
            {
                AllSettingsManager.Instance.UnregisterManager(this);
            }
        }

        // 提供获取设置实例的方法
        public TSettings GetSettings() => settings;
    }
}