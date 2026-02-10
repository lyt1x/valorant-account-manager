using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ValorantAccountManager.MVVM.Model;
using ValorantAccountManager.MVVM.ViewModel;

namespace ValorantAccountManager.MVVM.View
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid grid) return;
            if (grid.CurrentCell.Item is not Account acc) return;

            var column = grid.CurrentCell.Column?.Header?.ToString();
            if (column == "LOGIN")
                Clipboard.SetText(acc.Login);
            else if (column == "PASS")
                Clipboard.SetText(acc.Password);
        }
    }
}
