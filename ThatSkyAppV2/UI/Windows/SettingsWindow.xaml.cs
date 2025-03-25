using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using ThatSkyAppV2.Services;

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

            // Select current language
            var config = _configService.GetConfig();
            LanguageComboBox.SelectedItem = config.Language switch
            {
                "Chinese" => chineseItem,
                "Russian" => russianItem, 
                _ => englishItem
            };
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedLanguage = ((ComboBoxItem)LanguageComboBox.SelectedItem).Content.ToString()!;

            _configService.UpdateConfig(config =>
            {
                config.Language = selectedLanguage;
            });

            SettingsChanged?.Invoke();
            DialogResult = true;
            Close();
        }
    }
}
