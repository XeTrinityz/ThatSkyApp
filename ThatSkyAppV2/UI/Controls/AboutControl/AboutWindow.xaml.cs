using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace ThatSkyAppV2
{
    public partial class AboutWindow : UserControl
    {
        private const string DiscordUrl = "https://discord.com/invite/z5Ub9a3QhU";
        private const string GitHubUrl = "https://github.com/XeTrinityz/ThatSkyApp";

        public event EventHandler? CheckForUpdatesRequested;

        public AboutWindow()
        {
            InitializeComponent();
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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            OpenUrl(e.Uri.AbsoluteUri);
            e.Handled = true;
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
                MessageBox.Show(
                    $"Failed to open URL: {ex.Message}",
                    "Error",
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