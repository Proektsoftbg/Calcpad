using System;
using System.Windows;
using System.Windows.Threading;

namespace Calcpad.Wpf
{
    public partial class App : Application
    {
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            MainWindow main = (MainWindow)Current.MainWindow;
            if (main.IsSaved)
            {
#if BG
                var message =
                @$"Неочаквана грешка:
""{GetMessage(ex)}""
Няма незаписани данни. Calcpad ще се затвори след това съобщение.";
#else
                var message =
                @$"Unexpected error:
""{GetMessage(ex)}""
There is no unsaved data. Calcpad will shut down after this message.";
#endif
                MessageBox.Show(message, "Calcpad", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
#if BG
                var message1 =
                @$"Неочаквана грешка:
""{GetMessage(ex)}""
Calcpad ще запази вашите данни и ще се опита да се възстанови автоматчно.";
#else
            var message1 =
            @$"Unexpected error:
""{GetMessage(ex)}""
Calcpad will save your work and will try to recover automatically.";
#endif
            e.Handled = true;
            try
            {
                MessageBox.Show(message1, "Calcpad", MessageBoxButton.OK, MessageBoxImage.Error);
                main.SaveStateAndRestart();
                main.Close();
                Current.Shutdown();
            }
            catch (Exception ex2)
            {
#if BG
                var message2 =
                @$"Неочаквана грешка:
""{GetMessage(ex2)}""
Възстановяването е неуспешно. Ако проблемът продължи, моля пишете на proektsoft.bg@gmail.com.";
#else
                var message2 =
                @$"Unexpected error:
""{GetMessage(ex2)}""
Unable to recover. If the problem persists, please contact us on proektsoft.bg@gmail.com.";
#endif
                MessageBox.Show(message2, "Calcpad", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GetMessage(Exception e) => @$"Unexpected error:
""{e.Message}""
""Source: {e.Source}""
""Data: {e.Data}""
""Stack Trace: {e.StackTrace}""";
    }
}