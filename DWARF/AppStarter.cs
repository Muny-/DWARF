using System;
using DWARF;
public static class AppStarter
{
    [STAThread]
    public static void Main()
    {
        var app = new App();
        app.StartupUri = new Uri("DWARFLoader.xaml", UriKind.Relative);
        app.Run();
    }
}