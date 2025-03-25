using System;
using System.Linq;
using System.Windows;

namespace ThatSkyAppV2.Services
{
    public class LocalizationService
    {
        private readonly ConfigurationService _configService;

        public event Action? LanguageChanged;

        public LocalizationService(ConfigurationService configService)
        {
            _configService = configService;
            UpdateResources();
        }

        public void UpdateResources()
        {
            var config = _configService.GetConfig();
            string languageCode = GetLanguageCode(config.Language);

            // Clear existing merged dictionaries
            Application.Current.Resources.MergedDictionaries
                .Where(x => x.Source?.OriginalString?.Contains("LocalizationResources") ?? false)
                .ToList()
                .ForEach(dict => Application.Current.Resources.MergedDictionaries.Remove(dict));

            // Add the base resources
            var baseDict = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Resources/Localization/LocalizationResources.xaml", UriKind.Absolute)
            };
            Application.Current.Resources.MergedDictionaries.Add(baseDict);

            // Add language-specific resources if needed
            if (!string.IsNullOrEmpty(languageCode))
            {
                var langDict = new ResourceDictionary
                {
                    Source = new Uri($"pack://application:,,,/Resources/Localization/LocalizationResources.{languageCode}.xaml", UriKind.Absolute)
                };
                Application.Current.Resources.MergedDictionaries.Add(langDict);
            }

            LanguageChanged?.Invoke();
        }

        public string GetString(string key)
        {
            return Application.Current.Resources[key] as string ?? key;
        }

        private string GetLanguageCode(string language)
        {
            return language switch
            {
                "Chinese" => "zh",
                "Russian" => "ru",
                _ => ""
            };
        }
    }
}
