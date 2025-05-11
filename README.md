# SaveSettingsSystem - Unity设置管理系统

[English](#savesettingssystem---unity-settings-management-system) | [中文](#savesettingssystem---unity游戏设置管理系统)

## SaveSettingsSystem - Unity Settings Management System

### Overview

SaveSettingsSystem is a robust settings management framework for Unity. It provides a unified approach to handle game settings (audio, graphics, controls, etc.) with automatic serialization and persistence using PlayerPrefs.

### Core Features

1. **Centralized Management**
   - Global settings registry through `AllSettingsManager`
   - Automatic registration/unregistration of settings managers
   - Scene-specific or persistent settings support

2. **Type Safety & Validation**
   - Generic implementation for type safety
   - Automatic serialization validation
   - Error handling and logging

3. **Data Persistence**
   - Automatic PlayerPrefs serialization
   - Custom key management
   - Selective data deletion (NEW)
   - Complete data cleanup support (NEW)

4. **Event System**
   - Event-driven updates
   - Change notifications
   - UI synchronization

### System Architecture

#### Key Components

1. `AllSettingsManager`

   ```csharp
   public class AllSettingsManager : SingletonDD<AllSettingsManager>
   {
       public void SaveAllSettings()
       public void LoadAllSettings()
       public void ResetAllSettings()
       public T GetManager<T>() where T : MonoBehaviour
   }
   ```

2. `BaseSettings<TData, TSettingsSO>`

   ```csharp
   public abstract class BaseSettings<TData, TSettingsSO>
   where TData : class, new()
   where TSettingsSO : ScriptableObject
   {
       public virtual void Save()
       public virtual void Load()
       public abstract void ResetToDefault()
   }
   ```

3. `BaseSettingsManager<TSettings>`

   ```csharp
   public abstract class BaseSettingsManager<TSettings> : MonoBehaviour
   where TSettings : class, ISaveSettings
   ```

### Quick Start Guide

1. **Create Settings Data Container**

   ```csharp
   [CreateAssetMenu(fileName = "YourSettingsSO", menuName = "Settings/Your Settings")]
   public class YourSettingsSO : ScriptableObject
   {
       public float someValue = 1f;
   }
   ```

2. **Implement Settings Class**

   ```csharp
   public class YourSettings : BaseSettings<YourData, YourSettingsSO>
   {
       public YourSettings(YourSettingsSO settings) 
           : base(settings, "YourSettings") { }
       
       protected override YourData GetDataFromSettings()
       {
           return new YourData { someValue = settingsSO.someValue };
       }
       
       protected override void ApplyDataToSettings(YourData data)
       {
           settingsSO.someValue = data.someValue;
       }
   }
   ```

3. **Create Manager**

   ```csharp
   public class YourManager : BaseSettingsManager<YourSettings>
   {
       [SerializeField] private YourSettingsSO settingsSO;
       [SerializeField] private bool dontDestroyOnLoad;
       
       protected override void InitializeSettings()
       {
           settings = new YourSettings(settingsSO);
       }
   }
   ```

### Usage Examples

1. **Save/Load Settings**

   ```csharp
   // Through AllSettingsManager
   AllSettingsManager.Instance.SaveAllSettings();
   AllSettingsManager.Instance.LoadAllSettings();

   // Individual manager
   audioManager.Save();
   audioManager.Load();
   ```

2. **Delete Settings (NEW)**

   ```csharp
   // Delete specific setting
   Save_Load_SettingsSystem_Functions.DeletePlayerPrefsByKey("AudioSettings");

   // Delete all settings
   Save_Load_SettingsSystem_Functions.DeleteAllPlayerPrefs();
   ```

3. **Reset Settings**

   ```csharp
   AllSettingsManager.Instance.ResetAllSettings();
   ```

---

## SaveSettingsSystem - Unity游戏设置管理系统

### 概述

SaveSettingsSystem 是一个为Unity设计的稳健设置管理框架。它使用PlayerPrefs提供了统一的游戏设置（音频、图形、控制等）管理方案，具有自动序列化和持久化功能。

### 核心特性

1. **中央化管理**
   - 通过`AllSettingsManager`进行全局设置注册
   - 设置管理器的自动注册/注销
   - 支持场景特定或持久化设置

2. **类型安全和验证**
   - 泛型实现确保类型安全
   - 自动序列化验证
   - 错误处理和日志记录

3. **数据持久化**
   - 自动PlayerPrefs序列化
   - 自定义键管理
   - 选择性数据删除（新功能）
   - 完整数据清理支持（新功能）

4. **事件系统**
   - 事件驱动更新
   - 变更通知
   - UI同步

### 系统架构

#### 关键组件

1. `AllSettingsManager`
   - 全局设置注册表
   - 统一管理所有设置
   - 提供中心化访问点

2. `BaseSettings<TData, TSettingsSO>`
   - 所有设置的抽象基类
   - 处理序列化和持久化
   - 提供类型安全的数据访问

3. `BaseSettingsManager<TSettings>`
   - 管理具体设置实例
   - 处理UI绑定和生命周期
   - 自动注册到全局管理器

### 快速入门

1. **创建设置数据容器**

   ```csharp
   [CreateAssetMenu(fileName = "你的设置SO", menuName = "设置/你的设置")]
   public class YourSettingsSO : ScriptableObject
   {
       public float someValue = 1f;
   }
   ```

2. **实现设置类**

   ```csharp
   public class YourSettings : BaseSettings<YourData, YourSettingsSO>
   {
       public YourSettings(YourSettingsSO settings) 
           : base(settings, "YourSettings") { }
       
       protected override YourData GetDataFromSettings()
       {
           return new YourData { someValue = settingsSO.someValue };
       }
       
       protected override void ApplyDataToSettings(YourData data)
       {
           settingsSO.someValue = data.someValue;
       }
   }
   ```

3. **创建管理器**

   ```csharp
   public class YourManager : BaseSettingsManager<YourSettings>
   {
       [SerializeField] private YourSettingsSO settingsSO;
       [SerializeField] private bool dontDestroyOnLoad;
       
       protected override void InitializeSettings()
       {
           settings = new YourSettings(settingsSO);
       }
   }
   ```

### 使用示例

1. **保存/加载设置**

   ```csharp
   // 通过全局管理器
   AllSettingsManager.Instance.SaveAllSettings();
   AllSettingsManager.Instance.LoadAllSettings();

   // 单独管理器
   频繁操作，先获取 - 使用方式1，先获取引用再复用
   var audioManager = AllSettingsManager.Instance.GetManager<AudioManager>().Save();
   AllSettingsManager.Instance.GetManager<AudioManager>().Load();
   audioManager.Save();
   audioManager.Load();
   偶尔操作,需要更新 - 使用方式2，无需维护引用
   AllSettingsManager.Instance.GetManager<AudioManager>().Save();
   AllSettingsManager.Instance.GetManager<AudioManager>().Load();
     ```

2. **删除设置（新功能）**

   ```csharp
   // 删除特定设置
   Save_Load_SettingsSystem_Functions.DeletePlayerPrefsByKey("AudioSettings");

   // 删除所有设置
   Save_Load_SettingsSystem_Functions.DeleteAllPlayerPrefs();
   ```

3. **重置设置**

   ```csharp
   AllSettingsManager.Instance.ResetAllSettings();
   ```

### 最佳实践

1. **错误处理**
   - 验证数据有效性
   - 提供合理默认值
   - 记录有意义的错误信息

2. **性能优化**
   - 避免频繁保存
   - 批量处理设置变更
   - 合理使用事件系统

3. **场景管理**
   - 明确设置所有权
   - 正确的清理机制
   - 场景转换处理

4. **UI集成**
   - 响应式更新
   - 验证反馈
   - 用户友好的交互
