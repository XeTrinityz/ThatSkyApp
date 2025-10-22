using System;
using System.Net.Http;
using System.Threading.Tasks;
using ThatSkyAppV2.Constants;
using ThatSkyAppV2.Utils;
using ThatSkyAppV2.Services;

public class UpdateService
{
    private readonly HttpClient _httpClient;
    private readonly Action<string> _showPopup;
    private readonly Action<string> _updateInfoLabel;
    private readonly LocalizationService _localizationService;

    public UpdateService(HttpClient httpClient, Action<string> showPopup, Action<string> updateInfoLabel, LocalizationService localizationService)
    {
        _httpClient = httpClient;
        _showPopup = showPopup;
        _updateInfoLabel = updateInfoLabel;
        _localizationService = localizationService;
    }

    public async Task<(bool hasUpdate, string updateMessage)> CheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            _updateInfoLabel(_localizationService.GetString("Str.Message.CheckingUpdates"));

            // Get app version from GitHub releases page
            var appResponse = await _httpClient.GetAsync(
                "https://github.com/XeTrinityz/ThatSkyApp/releases/latest",
                cancellationToken
            );
            appResponse.EnsureSuccessStatusCode();
            string appHtml = await appResponse.Content.ReadAsStringAsync(cancellationToken);
            string latestVersion = ExtractAppVersion(appHtml);

            // Get mod version
            var modResponse = await _httpClient.GetAsync(
                "https://github.com/XeTrinityz/ThatSkyMod/releases/latest",
                cancellationToken
            );
            modResponse.EnsureSuccessStatusCode();
            string modHtml = await modResponse.Content.ReadAsStringAsync(cancellationToken);
            string modVersion = ExtractModVersion(modHtml);

            bool hasUpdate = VersionUtils.IsNewerVersion(latestVersion, AppConstants.AppVersion);

            string updateMessage = hasUpdate
                ? string.Format(_localizationService.GetString("Str.Update.MessageWithUpdate"), modVersion, latestVersion)
                : string.Format(_localizationService.GetString("Str.Update.MessageNoUpdate"), modVersion, AppConstants.AppVersion);

            Debug.WriteLine($"Has update: {hasUpdate}");
            Debug.WriteLine($"Update message: {updateMessage}");

            return (hasUpdate, updateMessage);
        }
        catch (Exception ex)
        {
            _showPopup(string.Format(_localizationService.GetString("Str.Message.UpdateFailed"), ex.Message));
            throw;
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

    private string ExtractModVersion(string html)
    {
        int startIndex = html.IndexOf("That Sky Mod V");
        if (startIndex != -1)
        {
            startIndex += "That Sky Mod V".Length;
            string versionPattern = html[startIndex..].Split(new[] { "·", "<", " " }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            return string.IsNullOrEmpty(versionPattern) ? "Unknown" : versionPattern;
        }
        return "Unknown";
    }

    public async Task UpdateApplicationAsync()
    {
        const string updateUrl = "https://github.com/XeTrinityz/TSM-Installer/releases/latest/download/ThatSkyApp.exe";
        string tempPath = Path.Combine(Path.GetTempPath(), "ThatSkyApp.exe");

        try
        {
            if (!await DownloadFileAsync(updateUrl, tempPath)) return;

            string currentPath = Process.GetCurrentProcess().MainModule?.FileName
                ?? throw new InvalidOperationException(_localizationService.GetString("Str.Error.CannotDetermineExePath"));

            string backupPath = currentPath + ".bak";
            File.Delete(backupPath);
            File.Move(currentPath, backupPath);
            File.Move(tempPath, currentPath);

            Process.Start(currentPath);
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _showPopup(string.Format(_localizationService.GetString("Str.Message.UpdateFailed"), ex.Message));
            throw;
        }
    }

    private async Task<bool> DownloadFileAsync(string url, string destination)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using var fs = new FileStream(destination, FileMode.Create);
            await response.Content.CopyToAsync(fs);
            return true;
        }
        catch (Exception ex)
        {
            _showPopup(string.Format(_localizationService.GetString("Str.Message.DownloadFailed"), ex.Message));
            return false;
        }
    }
}