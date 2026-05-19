using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EventsDB.Data;
using EventsDB.Services;

namespace EventsDB.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly DatabaseContext _db;
        private readonly AuthService _authService;

        private string _username = string.Empty;
        private string _errorMessage = string.Empty;
        private Visibility _errorVisibility = Visibility.Collapsed;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public Visibility ErrorVisibility
        {
            get => _errorVisibility;
            set => SetProperty(ref _errorVisibility, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(DatabaseContext db)
        {
            _db = db;
            _authService = new AuthService(db);
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private void ExecuteLogin(object? parameter)
        {
            // PasswordBox передається як параметр команди для безпеки
            if (parameter is PasswordBox passwordBox)
            {
                string password = passwordBox.Password;
                var parentWindow = Window.GetWindow(passwordBox);

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Будь ласка, введіть логін та пароль.");
                    return;
                }

                var user = _authService.Login(Username, password);

                if (user != null)
                {
                    SessionService.Login(user);
                    if (parentWindow != null)
                    {
                        parentWindow.DialogResult = true;
                        parentWindow.Close();
                    }
                }
                else
                {
                    ShowError("Невірний логін або пароль.");
                }
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = Visibility.Visible;
        }
    }
}