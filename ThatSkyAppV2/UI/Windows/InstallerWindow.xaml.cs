using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using MahApps.Metro.Controls;
using ThatSkyAppV2.Services;
using ThatSkyAppV2.Models;
using ThatSkyAppV2.Constants;
using ThatSkyAppV2.Utils;

namespace ThatSkyAppV2.UI.Windows;

public partial class InstallerWindow : MetroWindow
{
    // Will be initialized based on the configuration
    private ModInstallInfo[] _modInstallations;

    private readonly HttpClient _httpClient;
    private readonly AboutWindow _aboutWindow;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly InstallationService _installationService;
    private readonly UpdateService _updateService;
    private readonly GameLocationService _gameLocationService;
    private readonly AutoUpdateService _autoUpdateService;
    private readonly ConfigurationService _configService;
    private readonly LocalizationService _localizationService;

    public InstallerWindow()
    {
        InitializeComponent();
        _httpClient = CreateHttpClient();
        _cancellationTokenSource = new CancellationTokenSource();
        _aboutWindow = InitializeAboutWindow();

        _configService = new ConfigurationService();
        _localizationService = new LocalizationService(_configService);

        _gameLocationService = new GameLocationService();
        _installationService = new InstallationService(_httpClient, _configService, _localizationService, ShowPopup, UpdateInfoLabel);

        _updateService = new UpdateService(_httpClient, ShowPopup, UpdateInfoLabel);
        _autoUpdateService = new AutoUpdateService(_httpClient, content => InfoLabel.Content = content, isLoading => ToggleLoading(isLoading));

        // Initialize mod installations based on current config
        UpdateModInstallations();

        UpdateStatus(GetGameFolderFromRegistry(false));
        UpdateUIStrings();
        _ = CleanupAsync();
        _ = _autoUpdateService.CheckAndApplyUpdateAsync();

        _localizationService.LanguageChanged += UpdateUIStrings;
        
        // Listen for settings changes to update mod installations
        _configService.SettingsChanged += UpdateModInstallations;
    }


    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _localizationService.LanguageChanged -= UpdateUIStrings;
        _configService.SettingsChanged -= UpdateModInstallations;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _httpClient.Dispose();
    }

    private HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("ThatSkyApp/" + AppConstants.AppVersion);
        client.Timeout = TimeSpan.FromMinutes(5);
        return client;
    }

    private AboutWindow InitializeAboutWindow()
    {
        var aboutWindow = new AboutWindow
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = Visibility.Collapsed
        };
        _ = PopupContainer.Children.Add(aboutWindow);
        PopupContainer.Visibility = Visibility.Visible;
        return aboutWindow;
    }

    private async Task CleanupAsync()
    {
        try
        {
            string backupFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ThatSkyApp.exe.bak");
            if (File.Exists(backupFilePath))
            {
                await Task.Delay(3000, _cancellationTokenSource.Token);
                File.Delete(backupFilePath);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ShowPopup($"Cleanup failed: {ex.Message}");
        }
    }

    private void UpdateModInstallations()
    {
        var config = _configService.GetConfig();
        string smlFilename = config.UseNewModLoader ? "TSML.zip" : "sml-pc.zip";
        
        _modInstallations = new ModInstallInfo[] {
            new("TSM", "TSM.zip", true),
            new("SML", smlFilename, false)
        };
    }

    private bool IsComponentInstalled(string component)
    {
        string? gameFolder = GetGameFolderFromRegistry(false);
        if (string.IsNullOrEmpty(gameFolder)) return false;

        switch (component)
        {
            case "TSM":
                return File.Exists(Path.Combine(gameFolder, "mods", "TSM.dll"));
            case "SML":
                return File.Exists(Path.Combine(gameFolder, "powrprof.dll"));
            case "VCRedist":
                return IsRuntimeInstalled(@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64");
            default:
                return false;
        }
    }

    private bool IsInstalled()
    {
        string? gameFolder = GetGameFolderFromRegistry(false);
        return !string.IsNullOrEmpty(gameFolder) && File.Exists(Path.Combine(gameFolder, "mods", "TSM.dll"));
    }

    private void UpdateUIStrings()
    {
        // Helper function to find TextBlock in a Button
        TextBlock FindTextBlock(Button button)
        {
            var grid = button.Content as Grid;
            return grid?.Children.OfType<TextBlock>().FirstOrDefault();
        }

        // Main buttons
        InstallButtonText.Text = _localizationService.GetString(
            IsInstalled() ? "Str.Button.Update" : "Str.Button.Install");

        if (FindTextBlock(CheckForUpdatesButton) is TextBlock checkUpdatesText)
            checkUpdatesText.Text = _localizationService.GetString("Str.Button.CheckForUpdates");

        if (FindTextBlock(ChangelogButton) is TextBlock changelogText)
            changelogText.Text = _localizationService.GetString("Str.Button.Changelog");

        if (FindTextBlock(OpenTSMFolder) is TextBlock tsmResourcesText)
            tsmResourcesText.Text = _localizationService.GetString("Str.Button.TSMResources");

        if (FindTextBlock(LaunchSky) is TextBlock launchSkyText)
            launchSkyText.Text = _localizationService.GetString("Str.Button.LaunchSky");

        if (FindTextBlock(MaintenanceButton) is TextBlock maintenanceText)
            maintenanceText.Text = _localizationService.GetString("Str.Button.Maintenance");

        // Maintenance menu items
        if (FindTextBlock(RepairButton) is TextBlock repairText)
            repairText.Text = _localizationService.GetString("Str.Menu.RepairInstallation");

        if (FindTextBlock(InstallVCRedistButton) is TextBlock vcRedistText)
            vcRedistText.Text = _localizationService.GetString("Str.Menu.InstallVCRedist");

        if (FindTextBlock(UninstallButton) is TextBlock uninstallText)
            uninstallText.Text = _localizationService.GetString("Str.Menu.UninstallTSM");

        if (FindTextBlock(VerifyFilesButton) is TextBlock verifyFilesText)
            verifyFilesText.Text = _localizationService.GetString("Str.Menu.VerifyFiles");

        // Status labels
        UpdateStatusLabels();
    }


    private void UpdateInstallButton(bool isInstalled)
    {
        InstallButtonText.Text = _localizationService.GetString(
            isInstalled ? "Str.Button.Update" : "Str.Button.Install");
    }

    private void UpdateStatusLabel(Label label, bool isInstalled)
    {
        label.Content = _localizationService.GetString(
            isInstalled ? "Str.Status.Installed" : "Str.Status.NotInstalled");
        label.Foreground = isInstalled ? Brushes.Green : Brushes.Red;
    }

    private void UpdateStatusLabels()
    {
        // Status labels
        var labels = new[] {
        ("TSM", TSMStatusLabel),
        ("SML", SMLStatusLabel),
        ("VCRedist", VCRedistStatusLabel)
    };

        foreach (var (prefix, label) in labels)
        {
            UpdateStatusLabel(label, IsComponentInstalled(prefix));
        }
    }

    private void UpdateStatus(string? gameFolder)
    {
        try
        {
            UpdateVCRedistStatus();
            if (string.IsNullOrEmpty(gameFolder))
            {
                ShowPopup("Game folder not found. Please select it manually.");
                UpdateInstallButton(false);
                return;
            }
            UpdateModStatus(gameFolder);
        }
        catch (Exception ex)
        {
            ShowPopup($"Status update failed: {ex.Message}");
            UpdateInstallButton(false);
        }
    }

    private void UpdateVCRedistStatus()
    {
        string[] registryPaths = {
            @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x86",
            @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64"
        };

        bool isVCRedistInstalled = registryPaths.Any(IsRuntimeInstalled);
        UpdateStatusLabel(VCRedistStatusLabel, isVCRedistInstalled);
    }

    private bool IsRuntimeInstalled(string registryPath)
    {
        using var key = Registry.LocalMachine.OpenSubKey(registryPath);
        return key?.GetValue("Installed") is int installedValue && installedValue == 1;
    }

    private void UpdateModStatus(string gameFolder)
    {
        bool isTSMInstalled = File.Exists(Path.Combine(gameFolder, "mods", "TSM.dll"));
        bool isSMLInstalled = File.Exists(Path.Combine(gameFolder, "powrprof.dll"));

        UpdateStatusLabel(TSMStatusLabel, isTSMInstalled);
        UpdateStatusLabel(SMLStatusLabel, isSMLInstalled);
        UpdateInstallButton(isTSMInstalled);
    }

    private string? GetGameFolderFromRegistry(bool openFolderDialog) =>
        _gameLocationService.GetGameFolderFromRegistry(openFolderDialog);

    private void UpdateInfoLabel(string text)
    {
        InfoLabel.Content = text;
    }

    private async void InstallButton_Click(object sender, RoutedEventArgs e)
    {
        string? gameFolder = GetGameFolderFromRegistry(true);
        if (string.IsNullOrEmpty(gameFolder)) return;

        ToggleLoading(true);
        try
        {
            await _installationService.InstallModsAsync(gameFolder, _modInstallations);
            UpdateStatus(gameFolder);
        }
        finally
        {
            UpdateInfoLabel(string.Empty);
            ToggleLoading(false);
        }
    }

    private async void UninstallButton_Click(object sender, RoutedEventArgs e)
    {
        MaintenanceMenu.IsOpen = false;

        var dialog = new CustomDialog(
            _localizationService.GetString("Str.Message.UninstallConfirm"),
            _localizationService);

        PopupContainer.Children.Add(dialog);
        PopupContainer.Visibility = Visibility.Visible;

        bool? result = await dialog.ShowAsync();

        if (result == true)
        {
            string? gameFolder = GetGameFolderFromRegistry(true);
            if (string.IsNullOrEmpty(gameFolder)) return;

            ToggleLoading(true);
            try
            {
                FileUtils.RemoveNonProtectedFiles(gameFolder);
                ShowPopup(_localizationService.GetString("Str.Message.UninstallSuccess"));
                UpdateStatus(gameFolder);
            }
            catch (Exception ex)
            {
                ShowPopup($"Uninstall failed: {ex.Message}");
            }
            finally
            {
                UpdateInfoLabel(string.Empty);
                ToggleLoading(false);
            }
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_configService);
        settingsWindow.Owner = this;
        settingsWindow.SettingsChanged += () => {
            _localizationService.UpdateResources();
            UpdateStatus(GetGameFolderFromRegistry(false));
        };
        settingsWindow.ShowDialog();
    }

    private void MaintenanceButton_Click(object sender, RoutedEventArgs e)
    {
        MaintenanceMenu.IsOpen = true;
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);
        MaintenanceMenu.IsOpen = false;
    }

    private async void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleLoading(true);
        try
        {
            var (hasUpdate, updateMessage) = await _updateService.CheckForUpdatesAsync(_cancellationTokenSource.Token);
            ShowPopup(updateMessage);

            if (hasUpdate)
            {
                ToggleLoading(false);
                var dialog = new CustomDialog(
                    _localizationService.GetString("Str.Message.UpdateAvailable"),
                    _localizationService);

                PopupContainer.Children.Add(dialog);
                PopupContainer.Visibility = Visibility.Visible;

                bool? result = await dialog.ShowAsync();

                if (result == true)
                {
                    ToggleLoading(true);
                    await _updateService.UpdateApplicationAsync();
                }
            }
        }
        finally
        {
            UpdateInfoLabel(string.Empty);
            ToggleLoading(false);
        }
    }

    private async void RepairButton_Click(object sender, RoutedEventArgs e)
    {
        MaintenanceMenu.IsOpen = false;
        string? gameFolder = GetGameFolderFromRegistry(true);
        if (string.IsNullOrEmpty(gameFolder)) return;

        var dialog = new CustomDialog(
            _localizationService.GetString("Would you like to verify game files through Steam first?"),
            _localizationService);

        PopupContainer.Children.Add(dialog);
        PopupContainer.Visibility = Visibility.Visible;

        bool? result = await dialog.ShowAsync();

        if (result == true)
        {
            // Launch Steam verification
            Process.Start(new ProcessStartInfo
            {
                FileName = "steam://validate/2325290",
                UseShellExecute = true
            });

            // Show message about continuing after Steam verification
            var continueDialog = new CustomDialog(
                _localizationService.GetString("Please wait for Steam to finish verifying files, then click Yes to continue with TSM repair."),
                _localizationService);

            PopupContainer.Children.Add(continueDialog);
            bool? shouldContinue = await continueDialog.ShowAsync();

            if (shouldContinue != true) return;
        }

        ToggleLoading(true);
        try
        {
            FileUtils.RemoveNonProtectedFiles(gameFolder);
            await _installationService.InstallVCRedistAsync(_cancellationTokenSource.Token);
            await _installationService.InstallModsAsync(gameFolder, _modInstallations);

            ShowPopup("Repair completed successfully");
            UpdateStatus(gameFolder);
        }
        catch (OperationCanceledException ex)
        {
            ShowPopup(ex.Message);
        }
        catch (Exception ex)
        {
            ShowPopup($"Repair failed: {ex.Message}");
        }
        finally
        {
            UpdateInfoLabel(string.Empty);
            ToggleLoading(false);
        }
    }

    private void VerifyFilesButton_Click(object sender, RoutedEventArgs e)
    {
        MaintenanceMenu.IsOpen = false;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "steam://validate/2325290",
                UseShellExecute = true
            });

            ShowPopup(_localizationService.GetString("Str.Message.VerificationStarted"));
        }
        catch (Exception ex)
        {
            ShowPopup($"Failed to start verification: {ex.Message}");
        }
    }

    private async void InstallVCRedistButton_Click(object sender, RoutedEventArgs e)
    {
        MaintenanceMenu.IsOpen = false;
        ToggleLoading(true);
        try
        {
            await _installationService.InstallVCRedistAsync(_cancellationTokenSource.Token);
            UpdateVCRedistStatus();
        }
        catch (Exception ex)  
        {
            ShowPopup(ex.Message);
        }
        finally
        {
            UpdateInfoLabel(string.Empty);
            ToggleLoading(false);
        }
    }

    private void ChangelogButton_Click(object sender, RoutedEventArgs e) =>
        OpenUrl("https://github.com/XeTrinityz/ThatSkyMod/releases/latest");

    private void OpenTSMFolder_Click(object sender, RoutedEventArgs e)
    {
        string? gameFolder = GetGameFolderFromRegistry(true);
        if (string.IsNullOrEmpty(gameFolder)) return;

        string tsmFolder = Path.Combine(gameFolder, "mods", "TSM Resources");
        Process.Start("explorer.exe", tsmFolder);
    }

    private void LaunchSky_Click(object sender, RoutedEventArgs e) =>
        OpenUrl("steam://rungameid/2325290");

    private void AboutButton_Click(object sender, RoutedEventArgs e) =>
        _aboutWindow.FadeIn();

    private void MainContent_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_aboutWindow.Visibility == Visibility.Visible)
        {
            _aboutWindow.FadeOut();
        }
    }

    private void ShowPopup(string message)
    {
        if (message.Length > AppConstants.MaxPopupMessageLength)
        {
            message = message[..AppConstants.MaxPopupMessageLength] + "...";
        }

        var popup = new CustomPopup
        {
            Message = message,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 10, 40)
        };
        _ = PopupContainer.Children.Add(popup);
    }

    private void ToggleLoading(bool isLoading)
    {
        LoadingRing.Visibility = isLoading ? Visibility.Visible : Visibility.Hidden;
        MainContent.IsEnabled = !isLoading;
    }

    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowPopup($"Failed to open URL: {ex.Message}");
        }
    }
}