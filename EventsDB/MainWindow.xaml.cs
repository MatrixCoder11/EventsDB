using System;
using System.Windows;
using EventsDB.Data;
using EventsDB.ViewModels;

namespace EventsDB
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseContext _dbContext;

        public MainWindow(DatabaseContext db)
        {
            InitializeComponent();

            _dbContext = db;

            DataContext = new MainViewModel(db);
        }

        protected override void OnClosed(EventArgs e)
        {
            _dbContext.Dispose();
            base.OnClosed(e);
        }
    }
}