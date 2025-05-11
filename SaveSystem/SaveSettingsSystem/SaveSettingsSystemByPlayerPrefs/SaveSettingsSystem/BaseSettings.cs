using System;
using UnityEngine;

namespace SaveSettingsSystem
{
    /// <summary>
    /// 所有设置系统的抽象基类
    /// </summary>
    /// <typeparam name="TData">设置数据类型，必须是可序列化的引用类型</typeparam>
    /// <typeparam name="TSettingsSO">ScriptableObject设置类型</typeparam>
    [Serializable]
    public abstract class BaseSettings<TData, TSettingsSO> : ISaveSettings
        where TData : class, new()
        where TSettingsSO : ScriptableObject
    {
        protected readonly TSettingsSO settingsSO;//设置SO
        protected string settingsKey;//设置储存名
        public event EventHandler SettingsChanged;

        /// <summary>
        /// 初始化设置对象
        /// </summary>
        /// <param name="settings">设置SO实例</param>
        /// <param name="defaultKey">用于存储的键名，默认为类型名称</param>
        /// <exception cref="ArgumentNullException">如果settings为null则抛出</exception>
        protected BaseSettings(TSettingsSO settingsSO, string defaultKey = null)
        {
            this.settingsSO = settingsSO ?? throw new ArgumentNullException(paramName: nameof(settingsSO));//如果 defaultKey 不为 null，则使用 defaultKey 的值

            this.settingsKey = defaultKey ?? GetType().Name;//如果 defaultKey 为 null，则使用 GetType().Name（即当前类的名称）

            // 验证TData是否支持序列化
            if (!IsSerializable<TData>())
            {
#if UNITY_EDITOR_CHINESE
                AllSettingsManager.SettingsLogger.LogError($"类型 {typeof(TData).Name} 不支持序列化，这可能导致保存失败。" +
    "当前仅支持能够被序列化的类型,如:string、int、float、bool、enum、" +
    "以及具有公共字段/属性的 struct 或 class。");
#else
                AllSettingsManager.SettingsLogger.LogError($"Type {typeof(TData).Name} is not serializable, which may cause save failures. " +
    "Currently, only serializable types are supported, such as: string, int, float, bool, enum, " +
    "and structs or classes with public fields/properties.");
#endif
            }
        }

        /// <summary>
        /// 检查类型是否可序列化
        /// 不能被序列化的无法转换成json
        /// </summary>
        private bool IsSerializable<T>()
        {
            Type type = typeof(T);
            return type.IsSerializable ||
                   type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(decimal);
        }

        /// <summary>
        /// 触发通知(Notify)设置变更事件
        /// </summary>
        protected virtual void NotifySettingsChanged()
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 从设置SO获取设置数据
        /// </summary>
        /// <returns>当前设置的数据对象</returns>
        protected abstract TData GetDataFromSettings();

        /// <summary>
        /// 将设置数据应用到设置SO
        /// </summary>
        /// <param name="data">要应用的设置数据</param>
        protected abstract void ApplyDataToSettings(TData data);

        /// <summary>
        /// 保存设置到PlayerPrefs
        /// </summary>
        public virtual void Save()
        {
            try
            {
                var data = GetDataFromSettings();

                if (data == null)
                {
                    Debug.LogError($"获取设置数据失败: {settingsKey}");
                    return;
                }

                Save_Load_SettingsSystem_Functions.SaveByPlayerPrefs(settingsKey, data);
                NotifySettingsChanged(); // 保存成功后通知监听者
            }
            catch (Exception e)
            {
                Debug.LogError($"保存设置失败: {e.Message}");
            }
        }

        /// <summary>
        /// 从PlayerPrefs加载设置
        /// </summary>
        public virtual void Load()
        {
            try
            {
                var data = Save_Load_SettingsSystem_Functions.LoadByPlayerPrefs<TData>(settingsKey);

                if (data == null)
                {
                    AllSettingsManager.SettingsLogger.LogWarning($"[{GetType().Name}] 无法从PlayerPrefs加载'{settingsKey}'设置，正在使用默认值。这通常发生在首次运行时，这是正常现象。");
                    ResetToDefault();
                    return;
                }

                ApplyDataToSettings(data);
                NotifySettingsChanged(); // 加载时通知监听者
            }
            catch (Exception e)
            {
                Debug.LogError($"[{GetType().Name}] 加载设置失败: {e.Message}。将使用默认值。");
                ResetToDefault();
            }
#if UNITY_EDITOR
            finally
            {
                //todo-you can delete it when you don't need to konw where the settings are saved
                //todo-如果你不需要知道设置保存在哪里，可以删除这个代码
                string savePath = $"{Application.persistentDataPath}/PlayerPrefs";
                string registryPath = @"Software\Unity\UnityEditor\{Application.companyName}\{Application.productName}";
                Debug.Log($"[SaveSettingsSystem] 保存成功\n" +
                          $"逻辑存储路径: {savePath}\n" +
                          $"注册表位置: HKEY_CURRENT_USER\\{registryPath}\n" +
                          $"保存键值: {settingsKey}");
            }
#endif
        }

        /// <summary>
        /// 重置设置到默认值
        /// </summary>
        public abstract void ResetToDefault();

        /// <summary>
        /// 获取当前设置数据的副本
        /// </summary>
        /// <returns>当前设置数据</returns>
        public virtual TData GetCurrentData()
        {
            return GetDataFromSettings();
        }

        /// <summary>
        /// 应用设置并保存
        /// </summary>
        /// <param name="data">要应用的设置数据</param>
        public virtual void ApplyAndSave(TData data)
        {
            if (data == null)
            {
                AllSettingsManager.SettingsLogger.LogError("无法应用null设置数据");
                return;
            }

            ApplyDataToSettings(data);
            Save();
        }
    }
}