using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ValorantAccountManager.Core;
using ValorantAccountManager.MVVM.Model;
using ValorantAccountManager.MVVM.View;

namespace ValorantAccountManager.MVVM.ViewModel
{
    public class HomeViewModel : ObservableObject
    {
        // Храним ВСЕ аккаунты из БД
        private readonly List<Account> _allAccounts = new();

        public ObservableCollection<Account> Accounts { get; } = new();
        public ObservableCollection<AccountFolder> Folders { get; } = new();

        private AccountFolder _selectedFolder;
        public AccountFolder SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (_selectedFolder == value)
                    return;

                // Убираем выделение с предыдущей папки
                if (_selectedFolder != null)
                    _selectedFolder.IsSelected = false;

                _selectedFolder = value;

                // Выделяем новую папку
                if (_selectedFolder != null)
                    _selectedFolder.IsSelected = true;

                OnPropertyChanged();
                ApplyFolderFilter();
            }
        }

        public ICommand OpenTrackerCommand { get; }
        public ICommand AddAccountCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand ToggleRevealCommand { get; }
        public ICommand AddFolderCommand { get; }
        public ICommand SelectFolderCommand { get; }
        public ICommand MoveToFolderCommand { get; }
        public ICommand DeleteFolderCommand { get; }

        public HomeViewModel()
        {
            OpenTrackerCommand = new RelayCommand(p => OpenTracker(p as string));
            AddAccountCommand = new RelayCommand(_ => AddAccount());
            RefreshCommand = new RelayCommand(_ => Refresh());
            DeleteAccountCommand = new RelayCommand(p => DeleteAccount(p as Account));

            ToggleRevealCommand = new RelayCommand(p =>
            {
                if (p is Account acc)
                    acc.IsRevealed = !acc.IsRevealed;
            });

            AddFolderCommand = new RelayCommand(_ => AddFolder());
            SelectFolderCommand = new RelayCommand(p =>
            {
                if (p is AccountFolder folder)
                    SelectedFolder = folder;
            });

            MoveToFolderCommand = new RelayCommand(p => MoveToFolder(p as Account));
            DeleteFolderCommand = new RelayCommand(
    _ => DeleteSelectedFolder(),
    _ => SelectedFolder != null && !SelectedFolder.IsDefault
);

        }

        // ====== ЗАГРУЗКА ======

        public async Task LoadAsync()
        {
            Accounts.Clear();
            _allAccounts.Clear();
            Folders.Clear();

            var db = new DatabaseService();

            // --- ПАПКИ ---
            var folders = await db.GetFoldersAsync();
            foreach (var f in folders)
                Folders.Add(f);

            // гарантируем папку All
            var all = Folders.FirstOrDefault(f => f.IsDefault);
            if (all == null)
            {
                all = new AccountFolder { Name = "All", IsDefault = true };
                all.Id = await db.InsertFolderAsync(all);
                Folders.Add(all);
            }

            SelectedFolder = all;

            // --- АККАУНТЫ ---
            var baseAccounts = await db.GetAccountsAsync();
            foreach (var acc in baseAccounts)
            {
                _allAccounts.Add(acc);
                _ = LoadRankAsync(acc);
            }

            ApplyFolderFilter();
        }

        // ====== ФИЛЬТРАЦИЯ ======

        private void ApplyFolderFilter()
        {
            Accounts.Clear();

            if (SelectedFolder == null || SelectedFolder.IsDefault)
            {
                foreach (var acc in _allAccounts)
                    Accounts.Add(acc);
            }
            else
            {
                foreach (var acc in _allAccounts.Where(a => a.FolderId == SelectedFolder.Id))
                    Accounts.Add(acc);
            }
        }

        // ====== АККАУНТЫ ======

        private async void AddAccount()
        {
            var win = new AddAccountWindow(Folders.ToList());
            if (win.ShowDialog() != true)
                return;

            var acc = win.Result;

            acc.Rank = "Loading...";
            acc.Elo = 0;
            acc.RankIcon = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "rank_png",
                "Unranked_Rank.png"
            );

            var db = new DatabaseService();
            await db.InsertAccountAsync(acc);

            _allAccounts.Add(acc);
            ApplyFolderFilter();
            _ = LoadRankAsync(acc);
        }

        private async void DeleteAccount(Account acc)
        {
            if (acc == null)
                return;

            var db = new DatabaseService();
            await db.DeleteAccountAsync(acc.Id);

            _allAccounts.Remove(acc);
            ApplyFolderFilter();
        }

        private async void MoveToFolder(Account acc)
        {
            if (acc == null)
                return;

            // Используем готовое окно FolderSelectionWindow
            // Фильтруем папку "All" (IsDefault = true)
            var dialog = new FolderSelectionWindow(
                Folders.Where(f => !f.IsDefault).ToList(),
                acc.FolderId)
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() != true)
                return;

            // Обновляем папку аккаунта
            acc.FolderId = dialog.SelectedFolderId;
            var db = new DatabaseService();
            await db.UpdateAccountFolderAsync(acc.Id, dialog.SelectedFolderId);
            ApplyFolderFilter();
        }

        // ====== ПАПКИ ======

        private async void AddFolder()
        {
            var win = new AddFolderWindow
            {
                Owner = Application.Current.MainWindow
            };

            if (win.ShowDialog() != true)
                return;

            var folder = new AccountFolder
            {
                Name = win.FolderName,
                IsDefault = false
            };

            var db = new DatabaseService();
            folder.Id = await db.InsertFolderAsync(folder);

            Folders.Add(folder);
        }

        // ====== ПРОЧЕЕ ======

        private async Task LoadRankAsync(Account acc)
        {
            try
            {
                var response = await ValorantApiRequest.GetMmrAsync(
                    acc.Name, acc.Tag, acc.Region);

                var rank = response.Split(',')[0].Trim();
                var cleanRank = rank.Split('#')[0].Trim();
                var rankFileName = cleanRank.Replace(" ", "_") + "_Rank.png";

                var rrPart = response.Split("RR:")[1];
                var elo = int.Parse(rrPart.Split(' ')[1]);

                App.Current.Dispatcher.Invoke(() =>
                {
                    acc.Rank = rank;
                    acc.Elo = elo;
                    acc.RankIcon = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Data",
                        "rank_png",
                        rankFileName
                    );
                });
            }
            catch
            {
                // игнор
            }
        }

        private void OpenTracker(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private async void Refresh()
        {
            await LoadAsync();
        }
        private async void DeleteSelectedFolder()
        {
            if (SelectedFolder == null || SelectedFolder.IsDefault)
                return;

            var folder = SelectedFolder;

            SelectedFolder = Folders.First(f => f.IsDefault);

            var db = new DatabaseService();
            await db.DeleteFolderAsync(folder.Id);

            Folders.Remove(folder);
        }

    }
}