using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using ThatSkyAppV2.Constants;

namespace ThatSkyAppV2.Services;

public class GameLocationService
{
    private const string GameRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 2325290";

    public string? GetGameFolderFromRegistry(bool openFolderDialog)
    {
        // First try to get from registry
        string? registryGamePath = GetFromRegistry();

        if (!string.IsNullOrEmpty(registryGamePath) && IsCorrectGameFolder(registryGamePath))
        {
            return registryGamePath;
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

    private bool IsCorrectGameFolder(string path) =>
        !string.IsNullOrEmpty(path) &&
        Path.GetFileName(path).Equals(AppConstants.GameFolderName, StringComparison.OrdinalIgnoreCase) &&
        File.Exists(Path.Combine(path, "Sky.exe"));

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