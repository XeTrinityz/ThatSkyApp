using System.Text.Json;
using System.Text.Json.Serialization;
using ThatSkyAppV2.Constants;
using ThatSkyAppV2.Models;
using ThatSkyAppV2.Utils;

namespace ThatSkyAppV2.Services;

public class ConfigurationService
{
    private readonly string _configPath;
    private AppConfig _currentConfig;
    
    public event Action? SettingsChanged;

    public ConfigurationService()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appFolder = Path.Combine(appData, "ThatSkyApp");
        Directory.CreateDirectory(appFolder);
        _configPath = Path.Combine(appFolder, "config.json");
        _currentConfig = LoadConfiguration();
    }

    public AppConfig GetConfig() => _currentConfig;

    public void UpdateConfig(Action<AppConfig> updateAction)
    {
        updateAction(_currentConfig);
        SaveConfiguration();
        SettingsChanged?.Invoke();
    }

    private AppConfig LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                string jsonContent = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<AppConfig>(jsonContent);
                return config ?? new AppConfig();
            }
        }
        catch (Exception)
        {
            // If there's any error reading the config, return default
        }
        return new AppConfig();
    }

    private void SaveConfiguration()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string jsonContent = JsonSerializer.Serialize(_currentConfig, options);
            File.WriteAllText(_configPath, jsonContent);
        }
        catch (Exception)
        {
            //
        }
    }
}