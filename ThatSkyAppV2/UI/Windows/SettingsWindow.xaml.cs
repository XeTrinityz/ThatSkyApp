using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            var englishItem = new ComboBoxItem { Content = "English" };
            var chineseItem = new ComboBoxItem { Content = "Chinese" };
            var russianItem = new ComboBoxItem { Content = "Russian" };
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
            
            // Set mod loader radio button state
            TsmlRadioButton.IsChecked = config.UseNewModLoader;
            SmlRadioButton.IsChecked = !config.UseNewModLoader;

            // Set initial game path
            if (!string.IsNullOrEmpty(config.CustomGamePath))
            {
                _gamePath = config.CustomGamePath;
                GamePathTextBlock.Text = config.CustomGamePath;
            }
            else
            {
                // Try to detect the game path if not set
                try
                {
                    var gameLocationService = new GameLocationService(_configService);
                    var detectedPath = gameLocationService.GetGameFolderFromRegistry(false);
                    if (!string.IsNullOrEmpty(detectedPath))
                    {
                        GamePathTextBlock.Text = $"Using default location: {detectedPath}";
                    }
                }
                catch
                {
                    // Ignore errors in detection
                }
            }
        }

        private string _gamePath = string.Empty;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedLanguage = ((ComboBoxItem)LanguageComboBox.SelectedItem).Content.ToString()!;
            
            _configService.UpdateConfig(config =>
            {
                config.Language = selectedLanguage;
                config.UseNewModLoader = TsmlRadioButton.IsChecked ?? true;
                config.CustomGamePath = string.IsNullOrWhiteSpace(_gamePath) ? null : _gamePath;
            });

            SettingsChanged?.Invoke();
            DialogResult = true;
            Close();
        }

        private void UpdateGamePath(string path)
        {
            _gamePath = path;
            GamePathTextBlock.Text = string.IsNullOrEmpty(path) ? "Using default game location" : path;
        }

        private void BrowseGamePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Sky: Children of the Light Installation Folder",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.FolderName;
                
                // Check if the selected path contains the game
                if (File.Exists(Path.Combine(selectedPath, "Sky.exe")) ||
                    Directory.GetDirectories(selectedPath)
                        .Any(dir => File.Exists(Path.Combine(dir, "Sky.exe"))))
                {
                    UpdateGamePath(selectedPath);
                }
                else
                {
                    if (MessageBox.Show("The selected folder doesn't appear to contain Sky: Children of the Light.\n\nWould you like to use this location anyway?", 
                        "Game Not Found", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        UpdateGamePath(selectedPath);
                    }
                }
            }
        }
    }
}
