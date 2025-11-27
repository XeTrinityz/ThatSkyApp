using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using ThatSkyAppV2.Services;
using System.Windows.Threading;

namespace ThatSkyAppV2.UI.Windows
{
    public partial class SettingsWindow : MetroWindow
    {
        private readonly ConfigurationService _configService;
        public event Action? SettingsChanged;

        public SettingsWindow(ConfigurationService configService)
        {
            InitializeComponent();
            _configService = configService;

            // Populate ComboBox
            LanguageComboBox.Items.Clear();
            string langEn = (Application.Current.Resources["Str.Language.English"] as string) ?? "English";
            string langZh = (Application.Current.Resources["Str.Language.Chinese"] as string) ?? "Chinese";
            string langRu = (Application.Current.Resources["Str.Language.Russian"] as string) ?? "Russian";
            var englishItem = new ComboBoxItem { Content = langEn };
            var chineseItem = new ComboBoxItem { Content = langZh };
            var russianItem = new ComboBoxItem { Content = langRu };
            LanguageComboBox.Items.Add(englishItem);
            LanguageComboBox.Items.Add(chineseItem);
            LanguageComboBox.Items.Add(russianItem);

            // Select current language and initialize other settings
            var config = _configService.GetConfig();
            LanguageComboBox.SelectedItem = config.Language switch
            {
                "Chinese" => chineseItem,
                "Russian" => russianItem, 
                _ => englishItem
            };

            // Set initial mod install path
            if (!string.IsNullOrEmpty(config.ModInstallPath))
            {
                _gamePath = config.ModInstallPath;
                GamePathTextBlock.Text = config.ModInstallPath;
            }
            else
            {
                GamePathTextBlock.Text = (Application.Current.Resources["Str.Settings.ModInstall.NoPath"] as string) ?? "No mod path set";
            }

            // Initialize download latest toggle
            DownloadLatestCheckBox.IsChecked = config.AlwaysDownloadLatestOnInject;

            // Initialize auto-launch game toggle
            AutoLaunchGameCheckBox.IsChecked = config.AutoLaunchGame;

            // Initialize inject delay (ms)
            InjectDelayTextBox.Text = Math.Max(0, config.InjectDelayMs).ToString();

            // Initialize injection method
            InjectionMethodTSMRadio.IsChecked = true;
        }

        private string _gamePath = string.Empty;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedLanguage = ((ComboBoxItem)LanguageComboBox.SelectedItem).Content.ToString()!;
            
            _configService.UpdateConfig(config =>
            {
                config.Language = selectedLanguage;
                config.ModInstallPath = string.IsNullOrWhiteSpace(_gamePath) ? null : _gamePath;
                config.AlwaysDownloadLatestOnInject = DownloadLatestCheckBox.IsChecked == true;
                config.AutoLaunchGame = AutoLaunchGameCheckBox.IsChecked == true;

                // Persist inject delay (ms)
                if (int.TryParse(InjectDelayTextBox.Text, out int delayMs) && delayMs >= 0)
                {
                    config.InjectDelayMs = delayMs;
                }
                else
                {
                    config.InjectDelayMs = 0;
                }
            });

            SettingsChanged?.Invoke();
            DialogResult = true;
            Close();
        }

        private void UpdateGamePath(string path)
        {
            _gamePath = path;
            GamePathTextBlock.Text = string.IsNullOrEmpty(path)
                ? (Application.Current.Resources["Str.Settings.ModInstall.NoPath"] as string) ?? "No mod path set"
                : path;
        }

        private void BrowseGamePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = (Application.Current.Resources["Str.Dialog.SelectSkyInstallFolder"] as string)
                        ?? "Select Sky: Children of the Light Installation Folder",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.FolderName;
                // Accept any selected folder without validation
                UpdateGamePath(selectedPath);
            }
        }

        private void InjectDelayTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                // Move focus away from textbox
                Keyboard.ClearFocus();
                e.Handled = true;
            }
        }
    }
}
