using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using MahApps.Metro.Controls;
using ThatSkyAppV2.Services;
using ThatSkyAppV2.Models;
using ThatSkyAppV2.Constants;
using ThatSkyAppV2.Utils;
using Microsoft.Win32;

namespace ThatSkyAppV2.UI.Windows;

public partial class InstallerWindow : MetroWindow
{

    private readonly HttpClient _httpClient;
    private readonly AboutWindow _aboutWindow;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly InstallationService _installationService;
    private readonly UpdateService _updateService;
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

        _installationService = new InstallationService(_httpClient, _configService, _localizationService, ShowPopup, UpdateInfoLabel);

        _updateService = new UpdateService(_httpClient, ShowPopup, UpdateInfoLabel, _localizationService);
        _autoUpdateService = new AutoUpdateService(_httpClient, content => InfoLabel.Content = content, isLoading => ToggleLoading(isLoading));

        UpdateUIStrings();
        _ = CleanupAsync();
        _ = _autoUpdateService.CheckAndApplyUpdateAsync();

        _localizationService.LanguageChanged += UpdateUIStrings;
    }


    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _localizationService.LanguageChanged -= UpdateUIStrings;
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
            ShowPopup(string.Format(_localizationService.GetString("Str.Message.CleanupFailed"), ex.Message));
        }
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
        InstallButtonText.Text = _localizationService.GetString("Str.Button.Inject");

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
        if (FindTextBlock(InstallVCRedistButton) is TextBlock vcRedistText)
            vcRedistText.Text = _localizationService.GetString("Str.Menu.InstallVCRedist");

        if (FindTextBlock(VerifyFilesButton) is TextBlock verifyFilesText)
            verifyFilesText.Text = _localizationService.GetString("Str.Menu.VerifyFiles");

    }


    // GameLocationService is no longer used; install/verify flows do not require selecting the game path.

    private void UpdateInfoLabel(string text)
    {
        InfoLabel.Content = text;
    }

    private async void InstallButton_Click(object sender, RoutedEventArgs e)
    {
        // Prevent injection while the game is already running
        try
        {
            bool gameRunning = false;
            foreach (var p in Process.GetProcessesByName("Sky"))
            {
                try { if (!p.HasExited) { gameRunning = true; break; } }
                catch { gameRunning = true; break; }
            }

            if (gameRunning)
            {
                ShowPopup(_localizationService.GetString("Str.Message.CloseGameBeforeInject"));
                return;
            }
        }
        catch { /* best-effort check; if it fails, continue */ }

        // Ensure a ModInstallPath is configured
        var config = _configService.GetConfig();
        if (string.IsNullOrWhiteSpace(config.ModInstallPath))
        {
            var dialog = new OpenFolderDialog
            {
                Title = _localizationService.GetString("Str.Dialog.SelectBaseFolder"),
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                string basePath = dialog.FolderName;
                string target = System.IO.Path.Combine(basePath, "That Sky Mod");
                try
                {
                    if (!Directory.Exists(target)) Directory.CreateDirectory(target);
                    _configService.UpdateConfig(c => c.ModInstallPath = target);
                }
                catch (Exception ex)
                {
                    ShowPopup(string.Format(_localizationService.GetString("Str.Error.FailedSetModPath"), ex.Message));
                    return;
                }
            }
            else
            {
                return;
            }
        }

        ToggleLoading(true);
        try
        {
            Debug.WriteLine("[InstallButton_Click] Starting injection workflow");
            await _installationService.InjectLatestAsync(_cancellationTokenSource.Token);
            Debug.WriteLine("[InstallButton_Click] Injection completed successfully");
        }
        catch (Exception ex)
        {
            ShowPopup(string.Format(_localizationService.GetString("Str.Message.InjectionFailed"), ex.Message));
        }
        finally
        {
            UpdateInfoLabel(string.Empty);
            ToggleLoading(false);
        }
    }

    // Uninstall functionality removed.

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_configService);
        settingsWindow.Owner = this;
        settingsWindow.SettingsChanged += () => {
            _localizationService.UpdateResources();
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

    // Repair functionality removed.

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
            ShowPopup(string.Format(_localizationService.GetString("Str.Error.StartVerificationFailed"), ex.Message));
        }
    }

    private async void InstallVCRedistButton_Click(object sender, RoutedEventArgs e)
    {
        MaintenanceMenu.IsOpen = false;
        ToggleLoading(true);
        try
        {
            await _installationService.InstallVCRedistAsync(_cancellationTokenSource.Token);
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
        var config = _configService.GetConfig();
        if (string.IsNullOrWhiteSpace(config.ModInstallPath))
        {
            ShowPopup(_localizationService.GetString("Str.Message.PleaseSetModLocation"));
            return;
        }

        string tsmFolder = Path.Combine(config.ModInstallPath!, "TSM Resources");
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
            ShowPopup(string.Format(_localizationService.GetString("Str.Error.OpenUrlFailed"), ex.Message));
        }
    }
}