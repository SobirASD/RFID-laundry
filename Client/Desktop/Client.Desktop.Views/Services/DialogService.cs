﻿using System.Windows;
using Client.Desktop.ViewModels.Common.Services;
using Client.Desktop.ViewModels.Common.Windows;
using Microsoft.Win32;

namespace Client.Desktop.Views.Services
{
    public class DialogService : IDialogService
    {
        public bool ShowDialog(IWindowDialogViewModel windowDialogViewModel)
        {
            return true;
            //var window = windowDialogViewModel?.GetType().GetControl<Window>();
            //if (window == null)
            //    return false;

            //window.DataContext = windowDialogViewModel;

            //return window.ShowDialog() == true;
        }

        public bool ShowQuestionDialog(string message)
        {
            return ShowMessageDialog(message, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        public bool ShowErrorDialog(string message)
        {
            return ShowMessageDialog(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool ShowWarnigDialog(string message)
        {
            return ShowMessageDialog(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public bool ShowInfoDialog(string message)
        {
            return ShowMessageDialog(message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowBusy()
        {
            SetIsBusy(true);
        }

        public void HideBusy()
        {
            SetIsBusy(false);
        }

        public string ShowSaveFileDialog(string filter, string fileName)
        {
            var saveFileDialog = new SaveFileDialog { Filter = filter, FileName = fileName };

            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }

            return null;
        }

        private void SetIsBusy(bool isBusy)
        {
            //Helper.RunInMainThread(() => viewModel.IsBusy = isBusy);
        }

        private bool ShowMessageDialog(string questionMessage, string caption, MessageBoxButton buttons, MessageBoxImage image)
        {
            return true;
            //MessageBox.Show(Application.Current.MainWindow, "");
            //var dialogResult = MessageBox.Show(Application.Current.MainWindow, questionMessage, caption, buttons, image);

            //return dialogResult == MessageBoxResult.OK || dialogResult == MessageBoxResult.Yes;
        }
    }
}
