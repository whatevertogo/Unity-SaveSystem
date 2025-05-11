## 示例用法

### 1. 基础设置管理

```csharp
// 在游戏启动时加载设置
void Start()
{
    // 确保在使用设置之前加载
    settingsManager.Load();
    
    // 订阅设置变更事件
    if (settingsManager.Settings is ISaveSettings settings)
    {
        settings.SettingsChanged += OnSettingsChanged;
    }
}

void OnDestroy()
{
    // 清理事件订阅
    if (settingsManager.Settings is ISaveSettings settings)
    {
        settings.SettingsChanged -= OnSettingsChanged;
    }
}

// 当设置发生变化时的处理
private void OnSettingsChanged(object sender, EventArgs e)
{
    // 更新UI或其他相关逻辑
    UpdateUI();
    // 自动保存设置
    settingsManager.Save();
}

// 手动触发设置保存
public void OnSettingChanged()
{
    settingsManager.Save();
}

// 重置为默认设置
public void ResetSettings()
{
    settingsManager.ResetToDefault();
    UpdateUI(); // 确保UI反映新的设置
}
```

### 2. UI绑定示例

```csharp
public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    
    private AudioManager audioManager;
    private GraphicsManager graphicsManager;
    
    void Start()
    {
        InitializeManagers();
        SetupUIListeners();
        LoadAndApplySettings();
    }
    
    private void InitializeManagers()
    {
        audioManager = AudioManager.Instance;
        graphicsManager = GraphicsManager.Instance;
    }
    
    private void SetupUIListeners()
    {
        // 音量滑块
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        
        // 全屏切换
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        
        // 质量设置
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }
    
    private void LoadAndApplySettings()
    {
        // 加载设置并更新UI
        audioManager.Load();
        graphicsManager.Load();
        
        // 更新UI显示
        UpdateUIValues();
    }
    
    private void UpdateUIValues()
    {
        // 使用当前设置更新UI控件
        masterVolumeSlider.value = audioManager.Settings.MasterVolume;
        fullscreenToggle.isOn = graphicsManager.Settings.FullscreenMode;
        qualityDropdown.value = graphicsManager.Settings.QualityLevel;
    }
    
    // UI事件处理
    private void OnMasterVolumeChanged(float value)
    {
        audioManager.Settings.MasterVolume = value;
    }
    
    private void OnFullscreenChanged(bool isFullscreen)
    {
        graphicsManager.Settings.FullscreenMode = isFullscreen;
    }
    
    private void OnQualityChanged(int qualityLevel)
    {
        graphicsManager.Settings.QualityLevel = qualityLevel;
    }
}
```

### 3. 自定义设置示例

```csharp
// 自定义设置数据
[System.Serializable]
public class GameplayData
{
    public float gameDifficulty = 1f;
    public bool tutorialEnabled = true;
    public string lastSelectedCharacter = "Default";
}

// 自定义设置SO
[CreateAssetMenu(fileName = "GameplaySettingsSO", menuName = "Settings/Gameplay Settings")]
public class GameplaySettingsSO : ScriptableObject
{
    public float gameDifficulty = 1f;
    public bool tutorialEnabled = true;
    public string lastSelectedCharacter = "Default";
}

// 自定义设置类
public class GameplaySettings : BaseSettings<GameplayData, GameplaySettingsSO>
{
    public event Action<float> DifficultyChanged;
    
    public GameplaySettings(GameplaySettingsSO settings) : base(settings, "GameplaySettings")
    {
    }
    
    public float GameDifficulty
    {
        get => settingsSO.gameDifficulty;
        set
        {
            float clampedValue = Mathf.Clamp(value, 0.5f, 2f);
            if (!Mathf.Approximately(settingsSO.gameDifficulty, clampedValue))
            {
                settingsSO.gameDifficulty = clampedValue;
                DifficultyChanged?.Invoke(clampedValue);
                NotifySettingsChanged();
            }
        }
    }
    
    protected override GameplayData GetDataFromSettings()
    {
        return new GameplayData
        {
            gameDifficulty = settingsSO.gameDifficulty,
            tutorialEnabled = settingsSO.tutorialEnabled,
            lastSelectedCharacter = settingsSO.lastSelectedCharacter
        };
    }
    
    protected override void ApplyDataToSettings(GameplayData data)
    {
        settingsSO.gameDifficulty = data.gameDifficulty;
        settingsSO.tutorialEnabled = data.tutorialEnabled;
        settingsSO.lastSelectedCharacter = data.lastSelectedCharacter;
    }
    
    public override void ResetToDefault()
    {
        settingsSO.gameDifficulty = 1f;
        settingsSO.tutorialEnabled = true;
        settingsSO.lastSelectedCharacter = "Default";
        NotifySettingsChanged();
    }
}
```

### 4. 错误处理示例

```csharp
public class RobustSettingsManager : BaseSettingsManager<GameplaySettings>
{
    [SerializeField] private GameplaySettingsSO settingsSO;
    
    protected override void InitializeSettings()
    {
        try
        {
            settings = new GameplaySettings(settingsSO);
            LoadSettings();
        }
        catch (Exception e)
        {
            Debug.LogError($"设置初始化失败: {e.Message}");
            HandleInitializationError();
        }
    }
    
    private void LoadSettings()
    {
        try
        {
            settings.Load();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"加载设置失败，使用默认值: {e.Message}");
            settings.ResetToDefault();
        }
    }
    
    private void HandleInitializationError()
    {
        // 创建应急设置
        var emergencySettings = ScriptableObject.CreateInstance<GameplaySettingsSO>();
        settings = new GameplaySettings(emergencySettings);
        settings.ResetToDefault();
        
        // 通知用户
        Debug.LogWarning("使用应急设置配置");
    }
    
    public void SaveWithBackup()
    {
        try
        {
            // 在保存前创建备份
            var currentData = settings.GetDataFromSettings();
            string backupKey = $"{settings.SettingsKey}_backup";
            Save_Load_SettingsSystem_Functions.SaveByPlayerPrefs(backupKey, currentData);
            
            // 执行实际保存
            settings.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"保存设置失败: {e.Message}");
            TryRestoreFromBackup();
        }
    }
    
    private void TryRestoreFromBackup()
    {
        try
        {
            string backupKey = $"{settings.SettingsKey}_backup";
            var backupData = Save_Load_SettingsSystem_Functions.LoadByPlayerPrefs<GameplayData>(backupKey);
            if (backupData != null)
            {
                settings.ApplyDataToSettings(backupData);
                Debug.Log("已从备份恢复设置");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"从备份恢复失败: {e.Message}");
            settings.ResetToDefault();
        }
    }
}
```

### 5. 设置迁移示例

```csharp
public class SettingsMigrationManager
{
    private const string VERSION_KEY = "SettingsVersion";
    private const int CURRENT_VERSION = 2;
    
    public static void CheckAndMigrateSettings(BaseSettingsManager<GameplaySettings> manager)
    {
        int savedVersion = PlayerPrefs.GetInt(VERSION_KEY, 1);
        if (savedVersion < CURRENT_VERSION)
        {
            MigrateSettings(savedVersion, manager);
            PlayerPrefs.SetInt(VERSION_KEY, CURRENT_VERSION);
            PlayerPrefs.Save();
        }
    }
    
    private static void MigrateSettings(int oldVersion, BaseSettingsManager<GameplaySettings> manager)
    {
        try
        {
            switch (oldVersion)
            {
                case 1:
                    MigrateFromV1ToV2(manager);
                    break;
                default:
                    Debug.LogWarning($"未知的设置版本: {oldVersion}，重置为默认值");
                    manager.Settings.ResetToDefault();
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"设置迁移失败: {e.Message}");
            manager.Settings.ResetToDefault();
        }
    }
    
    private static void MigrateFromV1ToV2(BaseSettingsManager<GameplaySettings> manager)
    {
        // 迁移逻辑示例
        var settings = manager.Settings;
        // 执行迁移操作...
        Debug.Log("设置已成功迁移到V2");
    }
}
```

## 使用建议

1. **性能优化**
   - 避免频繁保存，考虑使用防抖动
   - 大型设置更改时批量处理
   - 仅在必要时序列化数据

2. **安全性**
   - 对加载的数据进行验证
   - 实现设置备份机制
   - 处理版本迁移
   - 每个场景都明确指定是使用全局还是本地设置

3. **可维护性**
   - 使用常量管理设置键
   - 实现详细的日志记录
   - 保持设置类职责单一
   - 对场景特定设置进行清晰文档记录

4. **用户体验**
   - 提供设置预览功能
   - 实现撤销/重做功能
   - 确保场景转换时设置平滑过渡
   - 添加设置导入/导出功能



## Example Usage

### 1. Basic Settings Management

```csharp
// Load settings at game startup
void Start()
{
    // Ensure settings are loaded before use
    settingsManager.Load();
    
    // Subscribe to settings change event
    if (settingsManager.Settings is ISaveSettings settings)
    {
        settings.SettingsChanged += OnSettingsChanged;
    }
}

void OnDestroy()
{
    // Clean up event subscriptions
    if (settingsManager.Settings is ISaveSettings settings)
    {
        settings.SettingsChanged -= OnSettingsChanged;
    }
}

// Handle settings changes
private void OnSettingsChanged(object sender, EventArgs e)
{
    // Update UI or other related logic
    UpdateUI();
    // Automatically save settings
    settingsManager.Save();
}

// Manually trigger settings save
public void OnSettingChanged()
{
    settingsManager.Save();
}

// Reset to default settings
public void ResetSettings()
{
    settingsManager.ResetToDefault();
    UpdateUI(); // Ensure UI reflects new settings
}
```

### 2. UI Binding Example

```csharp
public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    
    private AudioManager audioManager;
    private GraphicsManager graphicsManager;
    
    void Start()
    {
        InitializeManagers();
        SetupUIListeners();
        LoadAndApplySettings();
    }
    
    private void InitializeManagers()
    {
        audioManager = AudioManager.Instance;
        graphicsManager = GraphicsManager.Instance;
    }
    
    private void SetupUIListeners()
    {
        // Volume slider
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        
        // Fullscreen toggle
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        
        // Quality settings
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }
    
    private void LoadAndApplySettings()
    {
        // Load settings and update UI
        audioManager.Load();
        graphicsManager.Load();
        
        // Update UI display
        UpdateUIValues();
    }
    
    private void UpdateUIValues()
    {
        // Update UI controls with current settings
        masterVolumeSlider.value = audioManager.Settings.MasterVolume;
        fullscreenToggle.isOn = graphicsManager.Settings.FullscreenMode;
        qualityDropdown.value = graphicsManager.Settings.QualityLevel;
    }
    
    // UI event handling
    private void OnMasterVolumeChanged(float value)
    {
        audioManager.Settings.MasterVolume = value;
    }
    
    private void OnFullscreenChanged(bool isFullscreen)
    {
        graphicsManager.Settings.FullscreenMode = isFullscreen;
    }
    
    private void OnQualityChanged(int qualityLevel)
    {
        graphicsManager.Settings.QualityLevel = qualityLevel;
    }
}
```

### 3. Custom Settings Example

```csharp
// Custom settings data
[System.Serializable]
public class GameplayData
{
    public float gameDifficulty = 1f;
    public bool tutorialEnabled = true;
    public string lastSelectedCharacter = "Default";
}

// Custom settings SO
[CreateAssetMenu(fileName = "GameplaySettingsSO", menuName = "Settings/Gameplay Settings")]
public class GameplaySettingsSO : ScriptableObject
{
    public float gameDifficulty = 1f;
    public bool tutorialEnabled = true;
    public string lastSelectedCharacter = "Default";
}

// Custom settings class
public class GameplaySettings : BaseSettings<GameplayData, GameplaySettingsSO>
{
    public event Action<float> DifficultyChanged;
    
    public GameplaySettings(GameplaySettingsSO settings) : base(settings, "GameplaySettings")
    {
    }
    
    public float GameDifficulty
    {
        get => settingsSO.gameDifficulty;
        set
        {
            float clampedValue = Mathf.Clamp(value, 0.5f, 2f);
            if (!Mathf.Approximately(settingsSO.gameDifficulty, clampedValue))
            {
                settingsSO.gameDifficulty = clampedValue;
                DifficultyChanged?.Invoke(clampedValue);
                NotifySettingsChanged();
            }
        }
    }
    
    protected override GameplayData GetDataFromSettings()
    {
        return new GameplayData
        {
            gameDifficulty = settingsSO.gameDifficulty,
            tutorialEnabled = settingsSO.tutorialEnabled,
            lastSelectedCharacter = settingsSO.lastSelectedCharacter
        };
    }
    
    protected override void ApplyDataToSettings(GameplayData data)
    {
        settingsSO.gameDifficulty = data.gameDifficulty;
        settingsSO.tutorialEnabled = data.tutorialEnabled;
        settingsSO.lastSelectedCharacter = data.lastSelectedCharacter;
    }
    
    public override void ResetToDefault()
    {
        settingsSO.gameDifficulty = 1f;
        settingsSO.tutorialEnabled = true;
        settingsSO.lastSelectedCharacter = "Default";
        NotifySettingsChanged();
    }
}
```

### 4. Error Handling Example

```csharp
public class RobustSettingsManager : BaseSettingsManager<GameplaySettings>
{
    [SerializeField] private GameplaySettingsSO settingsSO;
    
    protected override void InitializeSettings()
    {
        try
        {
            settings = new GameplaySettings(settingsSO);
            LoadSettings();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize settings: {e.Message}");
            HandleInitializationError();
        }
    }
    
    private void LoadSettings()
    {
        try
        {
            settings.Load();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load settings, using default values: {e.Message}");
            settings.ResetToDefault();
        }
    }
    
    private void HandleInitializationError()
    {
        // Create emergency settings
        var emergencySettings = ScriptableObject.CreateInstance<GameplaySettingsSO>();
        settings = new GameplaySettings(emergencySettings);
        settings.ResetToDefault();
        
        // Notify user
        Debug.LogWarning("Using emergency settings configuration");
    }
    
    public void SaveWithBackup()
    {
        try
        {
            // Create backup before saving
            var currentData = settings.GetDataFromSettings();
            string backupKey = $"{settings.SettingsKey}_backup";
            Save_Load_SettingsSystem_Functions.SaveByPlayerPrefs(backupKey, currentData);
            
            // Perform actual save
            settings.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save settings: {e.Message}");
            TryRestoreFromBackup();
        }
    }
    
    private void TryRestoreFromBackup()
    {
        try
        {
            string backupKey = $"{settings.SettingsKey}_backup";
            var backupData = Save_Load_SettingsSystem_Functions.LoadByPlayerPrefs<GameplayData>(backupKey);
            if (backupData != null)
            {
                settings.ApplyDataToSettings(backupData);
                Debug.Log("Settings restored from backup");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to restore from backup: {e.Message}");
            settings.ResetToDefault();
        }
    }
}
```

### 5. Settings Migration Example

```csharp
public class SettingsMigrationManager
{
    private const string VERSION_KEY = "SettingsVersion";
    private const int CURRENT_VERSION = 2;
    
    public static void CheckAndMigrateSettings(BaseSettingsManager<GameplaySettings> manager)
    {
        int savedVersion = PlayerPrefs.GetInt(VERSION_KEY, 1);
        if (savedVersion < CURRENT_VERSION)
        {
            MigrateSettings(savedVersion, manager);
            PlayerPrefs.SetInt(VERSION_KEY, CURRENT_VERSION);
            PlayerPrefs.Save();
        }
    }
    
    private static void MigrateSettings(int oldVersion, BaseSettingsManager<GameplaySettings> manager)
    {
        try
        {
            switch (oldVersion)
            {
                case 1:
                    MigrateFromV1ToV2(manager);
                    break;
                default:
                    Debug.LogWarning($"Unknown settings version: {oldVersion}, resetting to default values");
                    manager.Settings.ResetToDefault();
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to migrate settings: {e.Message}");
            manager.Settings.ResetToDefault();
        }
    }
    
    private static void MigrateFromV1ToV2(BaseSettingsManager<GameplaySettings> manager)
    {
        // Example migration logic
        var settings = manager.Settings;
        // Perform migration operations...
        Debug.Log("Settings successfully migrated to V2");
    }
}
```

### Usage Recommendations

1. **Performance Optimization**
   - Avoid frequent saves, consider using debouncing
   - Batch process large settings changes
   - Serialize data only when necessary

2. **Security**
   - Validate loaded data
   - Implement settings backup mechanism
   - Handle version migrations

3. **Maintainability**
   - Use constants to manage settings keys
   - Implement detailed logging
   - Keep settings classes single-responsibility

4. **User Experience**
   - Provide settings preview functionality
   - Implement undo/redo functionality
   - Add settings import/export functionality