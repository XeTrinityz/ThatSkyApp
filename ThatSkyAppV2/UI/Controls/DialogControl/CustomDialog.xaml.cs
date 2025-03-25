using ThatSkyAppV2.Services;

namespace ThatSkyAppV2;

public partial class CustomDialog : UserControl
{
    private TaskCompletionSource<bool?> _tcs;
    private readonly LocalizationService _localizationService;

    public CustomDialog(string message, LocalizationService localizationService)
    {
        InitializeComponent();
        _localizationService = localizationService;
        MessageText.Text = message;
        _tcs = new TaskCompletionSource<bool?>();

        // Localize button texts
        YesButton.Content = _localizationService.GetString("Str.Button.Yes");
        NoButton.Content = _localizationService.GetString("Str.Button.No");
    }

    public Task<bool?> ShowAsync()
    {
        Visibility = Visibility.Visible;
        return _tcs.Task;
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        _tcs.SetResult(true);
        FadeOut();
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        _tcs.SetResult(false);
        FadeOut();
    }

    private void FadeOut()
    {
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.2));
        fadeOut.Completed += (s, e) =>
        {
            Visibility = Visibility.Collapsed;
            if (Parent is Panel panel)
            {
                panel.Children.Remove(this);
            }
        };
        BeginAnimation(OpacityProperty, fadeOut);
    }
}