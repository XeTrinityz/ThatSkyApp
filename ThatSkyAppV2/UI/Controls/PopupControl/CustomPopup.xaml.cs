namespace ThatSkyAppV2;

public partial class CustomPopup : UserControl
{
    private DispatcherTimer? _timer;
    private const double UpdateInterval = 0.05;
    private double _remainingTime;

    public CustomPopup()
    {
        InitializeComponent();
        Loaded += CustomPopup_Loaded;
        Opacity = 0;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _remainingTime -= UpdateInterval;

        if (_remainingTime <= 0)
        {
            _timer?.Stop();
            StartFadeOutAnimation();
        }
    }

    public string Message
    {
        get => MessageText.Text;
        set => MessageText.Text = value;
    }

    public double DisplayTimeInSeconds { get; set; } = 5;

    private void CustomPopup_Loaded(object sender, RoutedEventArgs e)
    {
        StartFadeInAnimation();

        _remainingTime = DisplayTimeInSeconds;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(UpdateInterval)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        Width = 350; // Increased from 250
    }

    private void StartFadeInAnimation()
    {
        DoubleAnimation fadeInAnimation = new()
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromSeconds(0.5))
        };

        BeginAnimation(OpacityProperty, fadeInAnimation);
    }

    private void StartFadeOutAnimation()
    {
        DoubleAnimation fadeOutAnimation = new()
        {
            From = 1,
            To = 0,
            Duration = new Duration(TimeSpan.FromSeconds(0.5))
        };
        fadeOutAnimation.Completed += (s, e) => (Parent as Panel)?.Children.Remove(this);

        BeginAnimation(OpacityProperty, fadeOutAnimation);
    }
}