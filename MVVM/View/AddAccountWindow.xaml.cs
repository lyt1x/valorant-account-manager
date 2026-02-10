using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ValorantAccountManager.MVVM.Model;

namespace ValorantAccountManager.MVVM.View
{
    public partial class AddAccountWindow : Window
    {
        public Account Result { get; private set; }
        private List<AccountFolder> _folders;

        public AddAccountWindow(List<AccountFolder> folders)
        {
            InitializeComponent();
            _folders = folders;
            RegionBox.SelectedIndex = 3; // EU по умолчанию
            
            // Заполняем ComboBox папками (кроме All)
            var nonDefaultFolders = folders.Where(f => !f.IsDefault).ToList();
            FolderBox.ItemsSource = nonDefaultFolders;
            if (nonDefaultFolders.Count > 0)
            {
                FolderBox.SelectedItem = null;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Result = new Account
            {
                Name = NameBox.Text,
                Tag = TagBox.Text,
                Region = (RegionBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Login = LoginBox.Text,
                Password = PasswordBox.Password,
                FolderId = (FolderBox.SelectedItem as AccountFolder)?.Id
            };

            DialogResult = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
