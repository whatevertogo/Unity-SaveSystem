using UnityEngine; 
using SaveSettingsSystem;


[CreateAssetMenu(fileName = "AudioSettingsSO", menuName = "Settings/Audio SettingsSO")]
public class AudioSettingsSO : ScriptableObject
{
    [Header("音量数据")] [SerializeField, Range(0, 1), Tooltip("主音量")]
    public float masterVolume = 1f;

    [SerializeField, Range(0, 1), Tooltip("背景音乐音量")]
    public float bgmVolume = 1f;

    [SerializeField, Range(0, 1), Tooltip("音效音量")]
    public float sfxVolume = 1f;
}