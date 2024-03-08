using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SasJobManager.Scheduler.Models
{

    public interface IDialogService
    {
        MessageBoxResult ShowMessageBox(string messageBoxText, string caption = null, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None);
    }

    public class DialogService : IDialogService
    {
        public virtual MessageBoxResult ShowMessageBox(string messageBoxText, string caption = null, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None)
        {
            return MessageBox.Show(messageBoxText, caption, buttons, icon, defaultResult);
        }
    }

}
