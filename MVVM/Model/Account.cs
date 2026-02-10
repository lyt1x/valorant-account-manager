using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValorantAccountManager.Core;

namespace ValorantAccountManager.MVVM.Model
{
    public class Account : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Region { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        private string _rank;
        public string Rank
        {
            get => _rank;
            set
            {
                _rank = value;
                OnPropertyChanged();
            }
        }

        private int _elo;
        public int Elo
        {
            get => _elo;
            set
            {
                _elo = value;
                OnPropertyChanged();
            }
        }

        private string _rankIcon;
        public string RankIcon
        {
            get => _rankIcon;
            set
            {
                _rankIcon = value;
                OnPropertyChanged();
            }
        }
        public string TrnLink => $"https://tracker.gg/valorant/profile/riot/{Uri.EscapeDataString(Name)}%23{Tag}/overview?platform=pc&playlist=competitive";
        public string TrackerIcon => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "trn.png");
        public string VtlLink => $"https://vtl.lol/id/{Name.Replace(" ", "+")}_{Tag}";

        public string VtlIcon => Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data","vtl.png");
        private bool _isRevealed;
        public bool IsRevealed
        {
            get => _isRevealed;
            set
            {
                _isRevealed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoginDisplay));
                OnPropertyChanged(nameof(PasswordDisplay));
            }
        }

        public string LoginDisplay =>
            IsRevealed ? Login : "****************";

        public string PasswordDisplay =>
            IsRevealed ? Password : "****************";
        private int? _folderId;
        public int? FolderId
        {
            get => _folderId;
            set
            {
                _folderId = value;
                OnPropertyChanged();
            }
        }
    }
}
