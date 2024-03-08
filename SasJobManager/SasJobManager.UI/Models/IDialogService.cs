using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SasJobManager.UI.Models
{
    public interface IDialogService
    {
        MessageBoxResult ShowMessageBox(string messageBoxText, string caption = null, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None);
    }
}
