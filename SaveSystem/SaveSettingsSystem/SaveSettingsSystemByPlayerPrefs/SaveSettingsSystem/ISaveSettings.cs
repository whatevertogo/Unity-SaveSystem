using System;

namespace SaveSettingsSystem
{
    /// <summary>
    /// 可保存设置的接口定义
    /// </summary>
    public interface ISaveSettings
    {
        /// <summary>
        /// 设置变更时触发的事件
        /// </summary>
        event EventHandler SettingsChanged;
        
        /// <summary>
        /// 保存设置
        /// </summary>
        void Save();
        
        /// <summary>
        /// 加载设置
        /// </summary>
        void Load();
        
        /// <summary>
        /// 重置设置到默认值
        /// </summary>
        void ResetToDefault();
    }
}