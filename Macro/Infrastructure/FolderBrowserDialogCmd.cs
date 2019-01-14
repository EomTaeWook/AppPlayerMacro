using System;
using System.Windows.Controls;
using System.Windows.Input;


namespace Macro.Infrastructure
{
    public class FolderBrowserDialogCmd : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if(parameter is TextBox)
                    {
                        (parameter as TextBox).Text = dlg.SelectedPath;
                    }
                }
            }
        }
    }
}
