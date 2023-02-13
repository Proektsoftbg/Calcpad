using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Calcpad.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App : Application
    {
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            var message = 
@$"Unexpected error:
{ex.Message}.
Calcpad will save your work and will try to recover.";
            e.Handled = true;            
            MainWindow main = (MainWindow)Current.MainWindow;
            try
            {
                MessageBox.Show(message, main.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                main.SaveState();
                MainWindow.Close();
                Current.Shutdown();
            }
            catch (Exception ex2)
            {
                var message2 =
@$"Unexpected error:
{ex2.Message}.
Calcpad will save your work and will try to recover.";
                MessageBox.Show(message2, main.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
