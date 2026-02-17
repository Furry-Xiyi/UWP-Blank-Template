using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Windows.Storage;

namespace UWP.Services
{
    /// <summary>
    /// AOT 安全的设置管理器
    /// 使用 JSON 文件而非 LocalSettings，避免 .NET Native 的装箱/拆箱问题
    /// </summary>
    public class SettingsManager
    {
        private static SettingsManager? _instance;
        public static SettingsManager Instance => _instance ??= new SettingsManager();

        private readonly string _settingsFilePath;
        private AppSettings _settings;

        private SettingsManager()
        {
            _settingsFilePath = Path.Combine(
                ApplicationData.Current.LocalFolder.Path,
                "app_settings.json"
            );
            
            _settings = LoadSettingsFromFile();
        }

        // 设置属性 - 直接访问，无需装箱/拆箱
        public string AppTheme
        {
            get => _settings.AppTheme;
            set
            {
                _settings.AppTheme = value;
                SaveSettings();
            }
        }

        public string AppMaterial
        {
            get => _settings.AppMaterial;
            set
            {
                _settings.AppMaterial = value;
                SaveSettings();
            }
        }

        public string PanePosition
        {
            get => _settings.PanePosition;
            set
            {
                _settings.PanePosition = value;
                SaveSettings();
            }
        }

        public bool EnableSound
        {
            get => _settings.EnableSound;
            set
            {
                _settings.EnableSound = value;
                SaveSettings();
            }
        }

        // 保存到文件
        private void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                File.WriteAllText(_settingsFilePath, json);
                Debug.WriteLine($"[SettingsManager] 设置已保存到: {_settingsFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsManager] 保存失败: {ex.Message}");
            }
        }

        // 从文件加载
        private AppSettings LoadSettingsFromFile()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    
                    if (settings != null)
                    {
                        Debug.WriteLine("[SettingsManager] 设置已从文件加载");
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsManager] 加载失败: {ex.Message}");
            }

            // 返回默认设置
            Debug.WriteLine("[SettingsManager] 使用默认设置");
            return new AppSettings();
        }
    }

    // 设置数据类 - 使用强类型，无需反射
    public class AppSettings
    {
        public string AppTheme { get; set; } = "System";
        public string AppMaterial { get; set; } = "Mica";
        public string PanePosition { get; set; } = "Left";
        public bool EnableSound { get; set; } = true;
    }
}
