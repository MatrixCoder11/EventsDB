using System.Windows;
using EventsDB.Data;
using EventsDB.ViewModels;

namespace EventsDB
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(DatabaseContext db)
        {
            InitializeComponent();
            DataContext = new LoginViewModel(db);
        }
    }
}