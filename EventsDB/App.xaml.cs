using EventsDB.Data;
using EventsDB.Models;
using EventsDB.Services;
using System.Windows;

namespace EventsDB;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var db = new DatabaseContext();

        if (!db.HasAnyUsers())
        {
            var auth = new AuthService(db);
            auth.Register("admin", "admin123", UserRole.Admin);
        }

        var loginWindow = new LoginWindow(db);
        var result = loginWindow.ShowDialog();

        if (result != true)
        {
            Shutdown();
            return;
        }

        var mainWindow = new MainWindow(db);
        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
    }
}