using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using ThatSkyAppV2.Constants;
using ThatSkyAppV2.Utils;

namespace ThatSkyAppV2.Services;

public class AutoUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly Action<string> _updateInfoLabel;
    private readonly Action<bool> _toggleLoading;
    private const string UPDATE_URL = "https://github.com/XeTrinityz/TSM-Installer/releases/latest/download/ThatSkyApp.exe";

    public AutoUpdateService(HttpClient httpClient, Action<string> updateInfoLabel, Action<bool> toggleLoading)
    {
        _httpClient = httpClient;
        _updateInfoLabel = updateInfoLabel;
        _toggleLoading = toggleLoading;
    }

    public async Task<bool> CheckAndApplyUpdateAsync()
    {
        try
        {
            _toggleLoading(true);
            _updateInfoLabel("Checking for mandatory updates...");

            var response = await _httpClient.GetAsync("https://github.com/XeTrinityz/TSM-Installer/releases/latest");
            response.EnsureSuccessStatusCode();
            string html = await response.Content.ReadAsStringAsync();

            string latestVersion = ExtractAppVersion(html);

            if (VersionUtils.IsNewerVersion(latestVersion, AppConstants.AppVersion))
            {
                _updateInfoLabel("Updating to latest version...");
                await ApplyUpdateAsync();
                return true;
            }
            _toggleLoading(false);
            return false;
        }
        catch (Exception)
        {
            _toggleLoading(false);
            return false;
        }
    }

    private string ExtractAppVersion(string html)
    {
        int startIndex = html.IndexOf("That Sky App V");
        if (startIndex != -1)
        {
            startIndex += "That Sky App V".Length;
            string versionPattern = html[startIndex..].Split(new[] { "·", "<", " " }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            return string.IsNullOrEmpty(versionPattern) ? "0.0.0" : versionPattern;
        }
        return "0.0.0";
    }

    private async Task ApplyUpdateAsync()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), "ThatSkyApp.exe");

        try
        {
            // Download new version
            using (var response = await _httpClient.GetAsync(UPDATE_URL))
            {
                response.EnsureSuccessStatusCode();
                using var fs = new FileStream(tempPath, FileMode.Create);
                await response.Content.CopyToAsync(fs);
            }

            string currentPath = Process.GetCurrentProcess().MainModule?.FileName
                ?? throw new InvalidOperationException("Cannot determine current executable path");

            string backupPath = currentPath + ".bak";

            // Backup current version
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
            File.Move(currentPath, backupPath);

            // Replace with new version
            File.Move(tempPath, currentPath);

            // Start new version and close current
            Process.Start(currentPath);
            Application.Current.Shutdown();
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
            throw;
        }
    }
}