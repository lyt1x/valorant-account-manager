using System.Windows;
using System.Windows.Controls;
using ValorantAccountManager.MVVM.Model;

namespace ValorantAccountManager.MVVM.View
{
    public partial class AddAccountWindow : Window
    {
        public Account Result { get; private set; }

        public AddAccountWindow()
        {
            InitializeComponent();
            RegionBox.SelectedIndex = 3; // EU по умолчанию
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Result = new Account
            {
                Name = NameBox.Text,
                Tag = TagBox.Text,
                Region = (RegionBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Login = LoginBox.Text,
                Password = PasswordBox.Password
            };

            DialogResult = true;
        }
    }
}