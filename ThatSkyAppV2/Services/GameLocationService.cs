using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using ThatSkyAppV2.Constants;

namespace ThatSkyAppV2.Services;

public class GameLocationService
{
    private const string GameRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 2325290";
    private readonly ConfigurationService _configService;

    public GameLocationService(ConfigurationService configService)
    {
        _configService = configService;
    }

    public string? GetGameFolderFromRegistry(bool openFolderDialog)
    {
        Debug.WriteLine($"[GameLocationService] Getting game folder (openDialog: {openFolderDialog})");
        
        // First check custom path from config
        var config = _configService.GetConfig();
        Debug.WriteLine($"[GameLocationService] Custom game path in config: {config.CustomGamePath ?? "(null)"}");
        
        if (!string.IsNullOrEmpty(config.CustomGamePath) && IsCorrectGameFolder(config.CustomGamePath, isCustomPath: true))
        {
            Debug.WriteLine("[GameLocationService] Using custom game path from config");
            return config.CustomGamePath;
        }
        else if (!string.IsNullOrEmpty(config.CustomGamePath))
        {
            Debug.WriteLine($"[GameLocationService] Custom game path is set but not valid: {config.CustomGamePath}");
        }

        // Then try to get from registry
        string? registryGamePath = GetFromRegistry();
        Debug.WriteLine($"[GameLocationService] Registry game path: {registryGamePath ?? "(null)"}");
        
        if (!string.IsNullOrEmpty(registryGamePath) && IsCorrectGameFolder(registryGamePath))
        {
            Debug.WriteLine("[GameLocationService] Using game path from registry");
            return registryGamePath;
        }
        else if (!string.IsNullOrEmpty(registryGamePath))
        {
            Debug.WriteLine("[GameLocationService] Registry path is not valid");
        }

        // If not found in registry and dialog requested, try manual selection
        if (openFolderDialog)
        {
            string? selectedPath = GetGameFolderFromDialog();
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // Store the valid path in registry for future use
                StoreInRegistry(selectedPath);
                return selectedPath;
            }
        }

        return null;
    }

    private string? GetFromRegistry()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(GameRegistryPath);
            return key?.GetValue("InstallLocation") as string;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void StoreInRegistry(string gamePath)
    {
        try
        {
            using var key = Registry.LocalMachine.CreateSubKey(GameRegistryPath);
            key?.SetValue("InstallLocation", gamePath, RegistryValueKind.String);
        }
        catch (UnauthorizedAccessException)
        {
            string userRegistryPath = $@"SOFTWARE\ThatSkyApp\GameLocation";
            using var userKey = Registry.CurrentUser.CreateSubKey(userRegistryPath);
            userKey?.SetValue("InstallLocation", gamePath, RegistryValueKind.String);
        }
        catch (Exception)
        {
            // Silently fail if we can't write to registry
        }
    }

    private string? GetGameFolderFromDialog()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog();
        if (dialog.ShowDialog() != true) return null;

        string selectedPath = dialog.FolderName;
        if (IsCorrectGameFolder(selectedPath)) return selectedPath;

        return ScanForGameFolder(selectedPath);
    }

    private bool IsCorrectGameFolder(string path, bool isCustomPath = false)
    {
        if (string.IsNullOrEmpty(path))
            return false;
            
        // If it's a custom path, only check if the path exists
        if (isCustomPath)
            return Directory.Exists(path);
            
        // For non-custom paths, check folder name and Sky.exe existence
        return Path.GetFileName(path).Equals(AppConstants.GameFolderName, StringComparison.OrdinalIgnoreCase) &&
               File.Exists(Path.Combine(path, "Sky.exe"));
    }

    private string? ScanForGameFolder(string directory)
    {
        try
        {
            // Check current directory
            if (Directory.GetDirectories(directory)
                        .FirstOrDefault(dir => IsCorrectGameFolder(dir)) is string foundDir)
            {
                return foundDir;
            }

            // Check parent directory
            string? parentDir = Path.GetDirectoryName(directory);
            if (parentDir != null)
            {
                return Directory.GetDirectories(parentDir)
                              .FirstOrDefault(dir => IsCorrectGameFolder(dir));
            }
        }
        catch (Exception)
        {
            // Handle or log error as needed
        }
        return null;
    }
}