namespace ThatSkyAppV2.Core;

public partial class App : Application
{
    [STAThread]
    public static void Main()
    {
        var application = new App();
        application.InitializeComponent();
        application.Run();
    }
}