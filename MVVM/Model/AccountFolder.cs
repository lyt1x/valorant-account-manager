using System.Collections.ObjectModel;
using ValorantAccountManager.Core;

namespace ValorantAccountManager.MVVM.Model
{
    public class AccountFolder : ObservableObject
    {
        public int Id { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public bool IsDefault { get; set; } // Папка "All" - дефолтная

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        private int _accountCount;
        public int AccountCount
        {
            get => _accountCount;
            set
            {
                _accountCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CountDisplay));
            }
        }

        public string CountDisplay => $"({AccountCount})";

        public ObservableCollection<Account> Accounts { get; set; }
            = new ObservableCollection<Account>();
    }
}