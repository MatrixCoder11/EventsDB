using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EventsDB.Data;
using EventsDB.Helpers;
using EventsDB.Models;
using EventsDB.Repositories;
using EventsDB.Services;

namespace EventsDB.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DatabaseContext _db;
        private readonly EventRepository _repo;

        // Поля для форми введення
        private string _time = string.Empty;
        private string _date = string.Empty;
        private string _location = string.Empty;
        private string _name = string.Empty;

        // Поля для пошуку та сортування
        private string _searchText = string.Empty;
        private int _selectedSearchType = 0;
        private int _selectedSortType = 0;

        // Виділений рядок у таблиці
        private Record? _selectedRecord;

        // Список записів для DataGrid
        private ObservableCollection<Record> _records = new();

        #region Властивості для зв'язування (Properties)

        public string Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        public string Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplySortingAndSearching();
            }
        }

        public int SelectedSearchType
        {
            get => _selectedSearchType;
            set
            {
                if (SetProperty(ref _selectedSearchType, value))
                    ApplySortingAndSearching();
            }
        }

        public int SelectedSortType
        {
            get => _selectedSortType;
            set
            {
                if (SetProperty(ref _selectedSortType, value))
                    ApplySortingAndSearching();
            }
        }

        public Record? SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (SetProperty(ref _selectedRecord, value))
                {
                    if (value != null)
                    {
                        Time = value.Time;
                        Date = value.Date;
                        Location = value.Location;
                        Name = value.Name;
                    }
                    else
                    {
                        ClearFormFields();
                    }
                }
            }
        }

        public ObservableCollection<Record> Records
        {
            get => _records;
            set => SetProperty(ref _records, value);
        }

        // Властивості прав доступу та профілю
        public Visibility AdminControlsVisibility => SessionService.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ViewerInfoVisibility => !SessionService.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        public string CurrentUsername => SessionService.CurrentUser?.Username ?? "Гість";

        #endregion

        #region Команди (Commands)

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand LogoutCommand { get; }

        #endregion

        public MainViewModel(DatabaseContext db)
        {
            _db = db;
            _repo = new EventRepository(_db);

            // Реєстрація команд
            AddCommand = new RelayCommand(ExecuteAdd);
            UpdateCommand = new RelayCommand(ExecuteUpdate);
            DeleteCommand = new RelayCommand(ExecuteDelete);
            ClearCommand = new RelayCommand(ExecuteClear);
            ClearSearchCommand = new RelayCommand(ExecuteClearSearch);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            // Перше завантаження таблиці
            ApplySortingAndSearching();
        }

        #region Методи команд (Execution)

        private void ExecuteAdd(object? parameter)
        {
            if (!ValidateInputs()) return;

            _repo.Add(Time, Date, Location, Name);
            MessageBox.Show("Запис успішно додано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

            ClearFormFields();
            ApplySortingAndSearching();
        }

        private void ExecuteUpdate(object? parameter)
        {
            if (SelectedRecord == null)
            {
                MessageBox.Show("Оберіть запис у таблиці для оновлення.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInputs()) return;

            SelectedRecord.Time = Time;
            SelectedRecord.Date = Date;
            SelectedRecord.Location = Location;
            SelectedRecord.Name = Name;

            if (_repo.Update(SelectedRecord))
            {
                MessageBox.Show("Запис оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFormFields();
                ApplySortingAndSearching();
            }
            else
            {
                MessageBox.Show("Помилка при оновленні запису.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDelete(object? parameter)
        {
            if (SelectedRecord == null)
            {
                MessageBox.Show("Оберіть запис у таблиці для видалення.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Ви дійсно хочете видалити подію '{SelectedRecord.Name}'?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _repo.Delete(SelectedRecord.Id);
                ClearFormFields();
                ApplySortingAndSearching();
            }
        }

        private void ExecuteClear(object? parameter)
        {
            ClearFormFields();
        }

        private void ExecuteClearSearch(object? parameter)
        {
            SearchText = string.Empty;
        }

        private void ExecuteLogout(object? parameter)
        {
            if (parameter is Window currentWindow)
            {
                var confirm = MessageBox.Show("Ви дійсно хочете вийти?", "Вихід",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                SessionService.Logout();
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                currentWindow.Close(); // Вікно закриється і спрацює Dispose() контексту бази

                var db = new DatabaseContext();
                var loginWindow = new LoginWindow(db);
                var loginResult = loginWindow.ShowDialog();

                if (loginResult != true)
                {
                    db.Dispose();
                    Application.Current.Shutdown();
                    return;
                }

                var newMain = new MainWindow(db);
                Application.Current.MainWindow = newMain;
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                newMain.Show();
            }
        }

        #endregion

        #region Допоміжні методи

        private void ApplySortingAndSearching()
        {
            string query = SearchText.Trim();
            List<Record> currentData;

            if (string.IsNullOrEmpty(query))
            {
                currentData = _repo.GetAll();
            }
            else
            {
                currentData = SelectedSearchType switch
                {
                    0 => _repo.SearchByName(query),
                    1 => _repo.SearchByTime(query),
                    2 => _repo.SearchByLocation(query),
                    3 => _repo.SearchByDate(query),
                    _ => _repo.GetAll()
                };
            }

            currentData = SelectedSortType switch
            {
                1 => currentData.OrderBy(r => r.TimeInMinutes()).ToList(),
                2 => currentData.OrderBy(r => r.Location).ToList(),
                3 => currentData.OrderBy(r => r.Name).ToList(),
                4 => currentData.OrderBy(r => r.DateAsNumber()).ToList(),
                5 => currentData.OrderByDescending(r => r.DateAsNumber()).ToList(),
                _ => currentData.OrderBy(r => r.Id).ToList()
            };

            Records = new ObservableCollection<Record>(currentData);
        }

        private bool ValidateInputs()
        {
            if (!Validator.IsValidTime(Time))
            {
                MessageBox.Show("Невірний формат часу. Використовуйте ГГ:ХХ (наприклад 09:30).", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!Validator.IsValidDate(Date))
            {
                MessageBox.Show("Невірний формат дати. Використовуйте ММ.ДД (наприклад 12.25).", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!Validator.IsNotEmpty(Location) || !Validator.IsNotEmpty(Name))
            {
                MessageBox.Show("Місце та Назва події не можуть бути порожніми.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void ClearFormFields()
        {
            Time = string.Empty;
            Date = string.Empty;
            Location = string.Empty;
            Name = string.Empty;
            _selectedRecord = null;
            OnPropertyChanged(nameof(SelectedRecord));
        }

        #endregion
    }
}