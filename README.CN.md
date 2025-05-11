# SaveSettingsSystem 使用说明

SaveSettingsSystem 是一个用于Unity的设置管理系统，它提供了一个统一的方式来处理游戏中的各种设置（如音频、图形等），并能够将这些设置保存到PlayerPrefs中。

![如何使用](images/image.png)
![设计思路](images/image1.png)
![泛型约束和依赖依赖关系](images/image2.png)
![AudioSettings示例](images/image3.png)

## 核心功能

1. 统一的设置管理接口
2. 自动序列化和持久化
3. 类型安全的设置访问
4. 事件驱动的设置更新
5. 支持默认值和重置功能
6. UI绑定支持

## 系统架构

### 1. 核心接口和基类

#### ISaveSettings 接口

```csharp
public interface ISaveSettings
{
    event EventHandler SettingsChanged;
    void Save();
    void Load();
    void ResetToDefault();
}
```

- 定义设置系统的基本操作：保存、加载、重置
- 提供设置变更事件通知机制

#### BaseSettings<TData, TSettingsSO>

- 所有具体设置类的抽象基类
- 实现通用的序列化和持久化逻辑
- 提供设置变更事件处理
- 类型参数：
  - TData: 设置数据类型（必须可序列化）
  - TSettingsSO: ScriptableObject设置类型

#### BaseSettingsManager<TSettings>

- 管理具体设置实例的抽象基类
- 实现单例模式
- 处理UI绑定和事件传递

### 2. 实现示例

#### 音频设置系统

##### AudioSettingsSO（数据容器）

```csharp
[CreateAssetMenu(fileName = "AudioVolumeSettingsSO", menuName = "Settings/Audio SettingsSO")]
public class AudioSettingsSO : ScriptableObject
{
    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
}
```

##### AudioSettings（设置逻辑）

- 继承自BaseSettings<AudioVolumeData, AudioSettingsSO>
- 实现音量控制逻辑
- 提供实际音量计算方法

##### AudioManager（管理器）

- 继承自BaseSettingsManager<AudioSettings>
- 管理音频源和音频剪辑
- 处理UI交互和音量更新

#### 图形设置系统

##### GraphicsSettingsSO（数据容器）

```csharp
[CreateAssetMenu(fileName = "GraphicsSettingsSO", menuName = "Settings/Graphics SettingsSO")]
public class GraphicsSettingsSO : ScriptableObject
{
    public bool fullscreenMode = true;
    public int resolutionIndex = 0;
    public int qualityLevel = 1;
    public int targetFrameRate = 60;
}
```

##### GraphicsSettings（设置逻辑）

- 继承自BaseSettings<GraphicsData, GraphicsSettingsSO>
- 实现图形设置逻辑
- 提供分辨率和质量设置方法

##### GraphicsManager（管理器）

- 继承自BaseSettingsManager<GraphicsSettings>
- 管理分辨率选项
- 处理UI交互和图形设置更新

### 场景特定设置支持

系统现在支持全局和场景特定的设置管理，这在 AudioManager 中特别有用：

#### 可配置的 DontDestroyOnLoad

```csharp
public class AudioManager : BaseSettingsManager<AudioSettings>
{
    [SerializeField] private bool dontDestroyOnLoad = false; // DontDestroyOnLoad 行为的开关
    
    // ...配置逻辑...
}
```

这个特性允许你：

- 通过禁用 DontDestroyOnLoad 来设置场景特定的音频设置
- 通过启用 DontDestroyOnLoad 在场景之间维护全局设置
- 避免不同场景之间的设置冲突
- 支持特定场景的专门音频配置

#### 场景特定设置最佳实践

1. **全局设置**
   - 对需要持久化的管理器在检视器中启用 dontDestroyOnLoad
   - 用于主菜单、全局背景音乐等

2. **场景特定设置**
   - 对场景特定的管理器禁用 dontDestroyOnLoad
   - 适用于关卡特定音频、专门配置
   - 离开场景时管理器会被销毁

3. **配置建议**
   - 根据场景需求决定是否持久化
   - 记录哪些场景使用全局或本地设置
   - 测试场景转换以确保行为正确

## 使用流程

### 1. 创建设置数据容器

```csharp
// 1. 创建 ScriptableObject 资产
[CreateAssetMenu(fileName = "YourSettingsSO", menuName = "Settings/Your Settings")]
public class YourSettingsSO : ScriptableObject
{
    public float someValue = 1f;
    // 添加你的设置字段
}
```

### 2. 实现设置类

```csharp
public class YourSettings : BaseSettings<YourData, YourSettingsSO>
{
    public YourSettings(YourSettingsSO settings) : base(settings, "YourSettings")
    {
    }
    
    // 实现必要的方法
    protected override YourData GetDataFromSettings() { ... }
    protected override void ApplyDataToSettings(YourData data) { ... }
    public override void ResetToDefault() { ... }
}
```

### 3. 创建管理器

```csharp
public class YourManager : BaseSettingsManager<YourSettings>
{
    [SerializeField] private YourSettingsSO settingsSO;
    
    protected override void InitializeSettings()
    {
        settings = new YourSettings(settingsSO);
        // 添加事件监听等初始化逻辑
    }
}
```

### 4.示例

Hierarchy结构：
├── AllSettingsManager (全局访问点)
│   ├── AudioManager (组件)
│   │   └── Audio Settings SO (引用)
│   ├── GraphicsManager (组件)
│   │   └── Graphics Settings SO (引用)
│   └── GameplayManager (组件)
│       └── Gameplay Settings SO (引用)

## 数据流向

1. **用户交互触发**
   - UI组件（如滑动条、开关等）触发值变更
   - 管理器接收这些变更并传递给对应的设置类

2. **设置更新**
   - 设置类验证并更新内部数据
   - 触发特定的变更事件（如OnVolumeChanged）
   - 触发通用的SettingsChanged事件

3. **数据持久化**
   - BaseSettings将数据序列化为JSON
   - 通过PlayerPrefs保存到本地存储
   - 保存成功后通知监听者

4. **事件通知链**
   - 设置类触发自身的变更事件
   - 管理器接收并处理这些事件
   - UI更新以反映新的设置值

## 实践想法

1. **类型安全**
   - 使用强类型的设置数据类
   - 确保数据类可以序列化

2. **事件处理**
   - 正确订阅和取消订阅事件
   - 在OnDestroy中清理事件监听

3. **值验证**
   - 在设置值之前进行范围检查
   - 使用Mathf.Clamp等方法确保值的有效性

4. **UI绑定**
   - 在Start中初始化UI组件
   - 使用UnityEvent简化UI绑定

5. **错误处理**
   - 优雅处理加载失败的情况
   - 提供合理的默认值
   - 使用try-catch捕获可能的异常
