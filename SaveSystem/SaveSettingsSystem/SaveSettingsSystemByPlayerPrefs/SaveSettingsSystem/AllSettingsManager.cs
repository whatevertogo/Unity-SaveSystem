using UnityEngine;
using System.Collections.Generic;
using CDTU.Utils;

namespace SaveSettingsSystem
{
    /// <summary>
    /// 总的设置管理器，用于管理所有子设置管理器
    /// </summary>
    public class AllSettingsManager : SingletonDD<AllSettingsManager>
    {
        public static bool HasInstance => Instance != null;
        /// <summary>
        /// 所有注册的设置管理器
        /// 使用哈希表来避免重复注册，因为哈希值都是唯一的
        /// 这样可以提高性能，因为哈希表的查找速度比列表快
        /// </summary>
        /// <typeparam name="ISaveSettings"></typeparam>
        /// <returns></returns>
        private HashSet<ISaveSettings> allSettings = new HashSet<ISaveSettings>();//使用哈希表
        /// <summary>
        /// 所有注册的设置管理器
        /// </summary>
        public IReadOnlyCollection<ISaveSettings> AllSettings => allSettings;

        /// <summary>
        /// 通过用于缓存注册的管理器
        /// </summary>
        private Dictionary<System.Type, MonoBehaviour> managerCache = new Dictionary<System.Type, MonoBehaviour>();

        #region 简易日志分类记录器
        /// <summary>
        /// 简易日志分类记录器，根据编译条件控制日志输出
        /// </summary>
        public static class SettingsLogger
        {
            private const string LOG_PREFIX = "[Settings] ";

            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
            public static void Log(string message)
            {
                Debug.Log($"{LOG_PREFIX}{message}");
            }

            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
            public static void LogWarning(string message)
            {
                Debug.LogWarning($"{LOG_PREFIX}{message}");
            }
            
            // 错误日志在所有版本都需要输出
            public static void LogError(string message)
            {
                Debug.LogError($"{LOG_PREFIX}{message}");
            }
        }
        #endregion

        /// <summary>
        /// 注册Manager
        /// </summary>
        /// <param name="manager"></param>
        public void RegisterManager(ISaveSettings manager)
        {
            if (manager == null || !(manager is MonoBehaviour monoBehaviour)) return;
            
            var type = monoBehaviour.GetType();
            if (!managerCache.ContainsKey(type))
            {
                managerCache[type] = monoBehaviour;
                allSettings.Add(manager);
                SettingsLogger.Log($"注册管理器: {type.Name}");
            }
        }

        /// <summary>
        /// 取消注册Manager
        /// </summary>
        /// <param name="manager"></param>

        public void UnregisterManager(ISaveSettings manager)
        {
            if (manager == null || !(manager is MonoBehaviour monoBehaviour)) return;
            
            var type = monoBehaviour.GetType();
            if (managerCache.ContainsKey(type))
            {
                allSettings.Remove(manager);
                managerCache.Remove(type);
                SettingsLogger.Log($"注销管理器: {type.Name}");
            }
        }

        /// <summary>
        /// 获取特定类型的Manager实例
        /// </summary>
        /// <typeparam name="T">Manager类型</typeparam>
        /// <returns>返回对应类型的Manager实例，如果不存在则返回null</returns>
        public T GetManager<T>() where T : MonoBehaviour
        {
            return managerCache.TryGetValue(typeof(T), out var manager) ? manager as T : null;
        }

        private void SafeExecute(System.Action action, string operationName)
        {
            try
            {
                action();
            }
            catch (System.Exception e)
            {
                SettingsLogger.LogError($"{operationName}失败: {e.Message}\n堆栈信息: {e.StackTrace}");
            }
        }

        public void SaveAllSettings()
        {
            foreach (var settings in allSettings)
            {
                SafeExecute(() => settings.Save(), "保存设置");
            }
        }

        public void LoadAllSettings()
        {
            foreach (var settings in allSettings)
            {
                SafeExecute(() => 
                {
                    try
                    {
                        settings.Load();
                    }
                    catch
                    {
                        settings.ResetToDefault();
                        throw;
                    }
                }, "加载设置");
            }
        }

        public void ResetAllSettings()
        {
            foreach (var settings in allSettings)
            {
                settings.ResetToDefault();
            }
            SaveAllSettings();
        }

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            // 在编辑器模式下，仅在真正退出游戏时保存，而不是每次退出 Play Mode 都保存
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                SaveAllSettings();
            }
#else
            SaveAllSettings();
#endif
        }
    }
}