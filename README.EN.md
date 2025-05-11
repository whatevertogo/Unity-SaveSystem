# SaveSettingsSystem User Guide
[中文](README.CN.md)

SaveSettingsSystem is a settings management system for Unity that provides a unified way to handle various game settings (such as audio, graphics, etc.) and save them to PlayerPrefs.

![How to Use](images/image.png)
![Design Concept](images/image1.png)
![Generic Constraints and Dependencies](images/image2.png)
![AudioSettings Example](images/image3.png)



## Core Features

1. Unified settings management interface
2. Automatic serialization and persistence
3. Type-safe settings access
4. Event-driven settings updates
5. Support for default values and reset functionality
6. UI binding support

## System Architecture

### 1. Core Interfaces and Base Classes

#### ISaveSettings Interface

```csharp
public interface ISaveSettings
{
    event EventHandler SettingsChanged;
    void Save();
    void Load();
    void ResetToDefault();
}
```

- Defines basic settings operations: save, load, reset
- Provides settings change event notification mechanism

#### BaseSettings<TData, TSettingsSO>

- Abstract base class for all concrete settings classes
- Implements common serialization and persistence logic
- Provides settings change event handling
- Type parameters:
  - TData: Settings data type (must be serializable)
  - TSettingsSO: ScriptableObject settings type

#### BaseSettingsManager<TSettings>

- Abstract base class for managing specific settings instances
- Implements singleton pattern
- Handles UI binding and event propagation

### 2. Implementation Examples

#### Audio Settings System

##### AudioSettingsSO (Data Container)

```csharp
[CreateAssetMenu(fileName = "AudioVolumeSettingsSO", menuName = "Settings/Audio SettingsSO")]
public class AudioSettingsSO : ScriptableObject
{
    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
}
```

##### AudioSettings (Settings Logic)

- Inherits from BaseSettings<AudioVolumeData, AudioSettingsSO>
- Implements volume control logic
- Provides actual volume calculation methods

##### AudioManager (Manager)

- Inherits from BaseSettingsManager<AudioSettings>
- Manages audio sources and audio clips
- Handles UI interaction and volume updates

#### Graphics Settings System

##### GraphicsSettingsSO (Data Container)

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

##### GraphicsSettings (Settings Logic)

- Inherits from BaseSettings<GraphicsData, GraphicsSettingsSO>
- Implements graphics settings logic
- Provides resolution and quality setting methods

##### GraphicsManager (Manager)

- Inherits from BaseSettingsManager<GraphicsSettings>
- Manages resolution options
- Handles UI interaction and graphics settings updates

### Scene-Specific Settings Support

The system now supports both global and scene-specific settings management, particularly useful for the AudioManager:

#### Configurable DontDestroyOnLoad

```csharp
public class AudioManager : BaseSettingsManager<AudioSettings>
{
    [SerializeField] private bool dontDestroyOnLoad = false; // Toggle for DontDestroyOnLoad behavior
    
    // ...configuration logic...
}
```

This feature allows you to:

- Set up scene-specific audio settings by disabling DontDestroyOnLoad
- Maintain global settings across scenes by enabling DontDestroyOnLoad
- Avoid settings conflicts between different scenes
- Support specialized audio configurations for specific scenes

#### Best Practices for Scene-Specific Settings

1. **Global Settings**
   - Enable dontDestroyOnLoad in the inspector for managers that need to persist
   - Use this for main menu, global BGM, etc.

2. **Scene-Specific Settings**
   - Disable dontDestroyOnLoad for scene-specific managers
   - Useful for level-specific audio, specialized configurations
   - Managers will be destroyed when leaving the scene

3. **Configuration Tips**
   - Consider scene requirements when deciding persistence
   - Document which scenes use global vs local settings
   - Test scene transitions to ensure proper behavior

## Usage Flow

### 1. Create Settings Data Container

```csharp
// 1. Create ScriptableObject asset
[CreateAssetMenu(fileName = "YourSettingsSO", menuName = "Settings/Your Settings")]
public class YourSettingsSO : ScriptableObject
{
    public float someValue = 1f;
    // Add your settings fields
}
```

### 2. Implement Settings Class

```csharp
public class YourSettings : BaseSettings<YourData, YourSettingsSO>
{
    public YourSettings(YourSettingsSO settings) : base(settings, "YourSettings")
    {
    }
    
    // Implement required methods
    protected override YourData GetDataFromSettings() { ... }
    protected override void ApplyDataToSettings(YourData data) { ... }
    public override void ResetToDefault() { ... }
}
```

### 3. Create Manager

```csharp
public class YourManager : BaseSettingsManager<YourSettings>
{
    [SerializeField] private YourSettingsSO settingsSO;
    
    protected override void InitializeSettings()
    {
        settings = new YourSettings(settingsSO);
        // Add event listeners and initialization logic
    }
}
```

## Data Flow

1. **User Interaction Trigger**
   - UI components (sliders, toggles, etc.) trigger value changes
   - Manager receives these changes and passes them to corresponding settings class

2. **Settings Update**
   - Settings class validates and updates internal data
   - Triggers specific change events (like OnVolumeChanged)
   - Triggers general SettingsChanged event

3. **Data Persistence**
   - BaseSettings serializes data to JSON
   - Saves to local storage via PlayerPrefs
   - Notifies listeners after successful save

4. **Event Notification Chain**
   - Settings class triggers its own change events
   - Manager receives and processes these events
   - UI updates to reflect new settings values

## Best Practices

1. **Type Safety**
   - Use strongly typed settings data classes
   - Ensure data classes are serializable

2. **Event Handling**
   - Properly subscribe and unsubscribe from events
   - Clean up event listeners in OnDestroy

3. **Value Validation**
   - Perform range checks before setting values
   - Use Mathf.Clamp and similar methods to ensure valid values

4. **UI Binding**
   - Initialize UI components in Start
   - Use UnityEvent to simplify UI binding

5. **Error Handling**
   - Gracefully handle load failures
   - Provide sensible default values
   - Use try-catch to catch potential exceptions
