using System;
using System.Globalization;
using System.Windows.Data;
using ValorantAccountManager.MVVM.Model;

namespace ValorantAccountManager.Core
{
    public class FolderComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AccountFolder selectedFolder && parameter is AccountFolder currentFolder)
            {
                return selectedFolder != null && currentFolder != null && selectedFolder.Id == currentFolder.Id;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
