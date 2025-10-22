using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using ThatSkyAppV2.Constants;

namespace ThatSkyAppV2
{
    public partial class AboutWindow : UserControl
    {
        private const string DiscordUrl = "https://discord.com/invite/kjpGzTU9hH";
        private const string GitHubUrl = "https://github.com/XeTrinityz/ThatSkyApp";

        public AboutWindow()
        {
            InitializeComponent();
            var fmt = Application.Current.Resources["Str.About.Version"] as string ?? "Version {0}";
            VersionText.Text = string.Format(fmt, AppConstants.AppVersion);
        }

        // Original methods for backwards compatibility
        public void FadeIn()
        {
            Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(OpacityProperty, fadeIn);
        }

        public void FadeOut()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (s, e) => Visibility = Visibility.Collapsed;
            BeginAnimation(OpacityProperty, fadeOut);
        }

        private static void OpenUrl(string url)
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
                string errFmt = Application.Current.Resources["Str.Error.OpenUrlFailed"] as string ?? "Failed to open URL: {0}";
                string title = Application.Current.Resources["Str.Common.ErrorTitle"] as string ?? "Error";
                MessageBox.Show(
                    string.Format(errFmt, ex.Message),
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OnDiscordClick(object sender, RoutedEventArgs e)
        {
            OpenUrl(DiscordUrl);
        }

        private void OnGitHubClick(object sender, RoutedEventArgs e)
        {
            OpenUrl(GitHubUrl);
        }
    }
}