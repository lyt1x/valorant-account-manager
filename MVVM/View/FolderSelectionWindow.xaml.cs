using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ValorantAccountManager.Core;
using ValorantAccountManager.MVVM.Model;

namespace ValorantAccountManager.MVVM.View
{
    public partial class FolderSelectionWindow : Window
    {
        public int? SelectedFolderId { get; private set; }
        public ICommand SelectFolderCommand { get; }

        public FolderSelectionWindow(List<AccountFolder> folders, int? currentFolderId)
        {
            InitializeComponent();

            SelectFolderCommand = new RelayCommand(p => SelectFolder(p as AccountFolder));

            FolderList.ItemsSource = folders;
            SelectedFolderId = currentFolderId;
        }

        private void SelectFolder(AccountFolder folder)
        {
            if (folder == null) return;

            SelectedFolderId = folder.IsDefault ? null : folder.Id;
            DialogResult = true;
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
