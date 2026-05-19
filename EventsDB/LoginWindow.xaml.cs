using System.Windows;
using EventsDB.Data;
using EventsDB.Services;

namespace EventsDB;

public partial class LoginWindow : Window
{
    private readonly AuthService _auth;

    public LoginWindow(DatabaseContext db)
    {
        InitializeComponent();
        _auth = new AuthService(db);
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        var username = txtUsername.Text.Trim();
        var password = txtPassword.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Введіть логін і пароль.");
            return;
        }

        var user = _auth.Login(username, password);

        if (user is null)
        {
            ShowError("Невірний логін або пароль.");
            return;
        }

        SessionService.Login(user);
        DialogResult = true;
    }

    private void ShowError(string message)
    {
        txtError.Text = message;
        txtError.Visibility = Visibility.Visible;
    }
}