using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using ValorantAccountManager.Core;
using ValorantAccountManager.MVVM.Model;
using ValorantAccountManager.MVVM.View;

namespace ValorantAccountManager.MVVM.ViewModel
{
    public class HomeViewModel
    {
        public ICommand OpenTrackerCommand { get; }
        public ICommand AddAccountCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand ToggleRevealCommand { get; }
        public HomeViewModel()
        {
            OpenTrackerCommand = new RelayCommand(
                p => OpenTracker(p as string)
            );
            AddAccountCommand = new RelayCommand(
            _ => AddAccount()
            );
            RefreshCommand = new RelayCommand(_ => Refresh());
            DeleteAccountCommand = new RelayCommand(p => DeleteAccount(p as Account));
            ToggleRevealCommand = new RelayCommand(p =>
            {
                if (p is Account acc)
                    acc.IsRevealed = !acc.IsRevealed;
            });
        }
        public ObservableCollection<Account> Accounts { get; }
        = new ObservableCollection<Account>();

        public async Task LoadAsync()
        {
            Accounts.Clear();

            var db = new DatabaseService();
            var baseAccounts = await db.GetAccountsAsync();

            foreach (var acc in baseAccounts)
            {
                Accounts.Add(acc);
                _ = LoadRankAsync(acc);
            }
        }
        private async Task LoadRankAsync(Account acc)
        {
            try
            {
                var json = await ValorantApiRequest.GetMmrAsync(
                    acc.Name, acc.Tag, acc.Region);

                var response = json;

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
            }
        }
        private void OpenTracker(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        private async void AddAccount()
        {
            var win = new AddAccountWindow();
            if (win.ShowDialog() != true) return;

            var acc = win.Result;

            acc.Rank = "Loading...";
            acc.Elo = 0;
            acc.RankIcon = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", "rank_png", "Unranked_Rank.png"
            );

            var db = new DatabaseService();
            await db.InsertAccountAsync(acc);

            Accounts.Add(acc);
            _ = LoadRankAsync(acc);
        }
        private async void Refresh()
        {
            await LoadAsync();
        }
        private async void DeleteAccount(Account acc)
        {
            if (acc == null) return;

            var db = new DatabaseService();
            await db.DeleteAccountAsync(acc.Id);

            Accounts.Remove(acc);
        }

    }
}