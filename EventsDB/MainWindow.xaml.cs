using System.Windows;
using System.Windows.Controls;
using EventsDB.Data;
using EventsDB.Helpers;
using EventsDB.Models;
using EventsDB.Repositories;
using EventsDB.Services;

namespace EventsDB
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseContext _dbContext;
        private readonly EventRepository _repo;



        public MainWindow(DatabaseContext db)
        {
            InitializeComponent();

            _dbContext = db;
            _repo = new EventRepository(_dbContext);

            RefreshTable();
            cmbSortType.SelectedIndex = 0;
            ApplyRolePermissions(); 
        }



        private void RefreshTable(List<Record>? records = null)
        {
            dgEvents.ItemsSource = records ?? _repo.GetAll();
        }

        private void ClearForm()
        {
            txtTime.Clear();
            txtDate.Clear();
            txtLocation.Clear();
            txtName.Clear();
            dgEvents.SelectedItem = null;
        }


        private bool ValidateInputs()
        {
            if (!Validator.IsValidTime(txtTime.Text))
            {
                MessageBox.Show("Невірний формат часу. Використовуйте ГГ:ХХ (наприклад 09:30).", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!Validator.IsValidDate(txtDate.Text))
            {
                MessageBox.Show("Невірний формат дати. Використовуйте ММ.ДД (наприклад 12.25).", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!Validator.IsNotEmpty(txtLocation.Text) || !Validator.IsNotEmpty(txtName.Text))
            {
                MessageBox.Show("Місце та Назва події не можуть бути порожніми.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }


        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            _repo.Add(txtTime.Text, txtDate.Text, txtLocation.Text, txtName.Text);

            MessageBox.Show("Запис успішно додано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm();
            ApplySortingAndSearching(); 
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgEvents.SelectedItem is Record selectedRecord)
            {
                if (!ValidateInputs()) return;

                selectedRecord.Time = txtTime.Text;
                selectedRecord.Date = txtDate.Text;
                selectedRecord.Location = txtLocation.Text;
                selectedRecord.Name = txtName.Text;

                if (_repo.Update(selectedRecord))
                {
                    MessageBox.Show("Запис оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    ApplySortingAndSearching();
                }
                else
                {
                    MessageBox.Show("Помилка при оновленні запису.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Оберіть запис у таблиці для оновлення.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgEvents.SelectedItem is Record selectedRecord)
            {
                var result = MessageBox.Show($"Ви дійсно хочете видалити подію '{selectedRecord.Name}'?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _repo.Delete(selectedRecord.Id);
                    ClearForm();
                    ApplySortingAndSearching();
                }
            }
            else
            {
                MessageBox.Show("Оберіть запис у таблиці для видалення.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }


        private void DgEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgEvents.SelectedItem is Record selectedRecord)
            {
                txtTime.Text = selectedRecord.Time;
                txtDate.Text = selectedRecord.Date;
                txtLocation.Text = selectedRecord.Location;
                txtName.Text = selectedRecord.Name;
            }
        }


        private void ApplySortingAndSearching()
        {
            string query = txtSearch.Text.Trim();
            List<Record> currentData;

            if (string.IsNullOrEmpty(query))
            {
                currentData = _repo.GetAll();
            }
            else
            {
                currentData = cmbSearchType.SelectedIndex switch
                {
                    0 => _repo.SearchByName(query),
                    1 => _repo.SearchByTime(query),
                    2 => _repo.SearchByLocation(query),
                    3 => _repo.SearchByDate(query),
                    _ => _repo.GetAll()
                };
            }


            if (cmbSortType != null)
            {
                currentData = cmbSortType.SelectedIndex switch
                {
                    1 => currentData.OrderBy(r => r.TimeInMinutes()).ToList(),
                    2 => currentData.OrderBy(r => r.Location).ToList(),
                    3 => currentData.OrderBy(r => r.Name).ToList(),
                    4 => currentData.OrderBy(r => r.DateAsNumber()).ToList(),
                    5 => currentData.OrderByDescending(r => r.DateAsNumber()).ToList(),
                    _ => currentData.OrderBy(r => r.Id).ToList() 
                };
            }

            RefreshTable(currentData);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySortingAndSearching();
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
        }

        private void CmbSortType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySortingAndSearching();
        }

        protected override void OnClosed(EventArgs e)
        {
            _dbContext.Dispose();
            base.OnClosed(e);
        }

        private void ApplyRolePermissions()
        {
            if (!SessionService.IsAdmin)
            {
                pnlAdminControls.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show("Ви дійсно хочете вийти?", "Вихід",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            SessionService.Logout();

            // 1. Перемикаємо режим, щоб закриття поточного вікна не завершило роботу програми
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // 2. Закриваємо поточне головне вікно відразу (воно зникне з екрану та звільнить ресурси)
            this.Close();

            // 3. Створюємо новий контекст та відкриваємо вікно авторизації
            var db = new DatabaseContext();
            var loginWindow = new LoginWindow(db);
            var loginResult = loginWindow.ShowDialog();

            if (loginResult != true)
            {
                db.Dispose();
                Application.Current.Shutdown();
                return;
            }

            // 4. Якщо вхід успішний, створюємо та показуємо нове головне вікно
            var newMain = new MainWindow(db);
            Application.Current.MainWindow = newMain;
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            newMain.Show();
        }
    }
}