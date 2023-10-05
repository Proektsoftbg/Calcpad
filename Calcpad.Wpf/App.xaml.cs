using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Calcpad.Wpf
{
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;
        }

        private void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException -= AppDomain_UnhandledException;
            var ex = (Exception)e.ExceptionObject;
            MainWindow main = (MainWindow)Current.MainWindow;
            var logFileName = Path.GetTempFileName();
            var message = GetMessage(ex);
            if (main.IsSaved)
            {
#if BG
                message += "\r\n\r\nНяма незаписани данни. Ако проблемът продължи, моля пишете на proektsoft.bg@gmail.com.";
#else
                message += "\r\n\r\nThere is no unsaved data. If the problem persists, please contact proektsoft.bg@gmail.com.";
#endif
            }
            else
            {
#if BG
                message += "\r\n\r\nCalcpad запази вашите данни и се опита да се възстанови автоматчно.";
#else
                message += "\r\n\r\nCalcpad saved your work and tried to recover automatically.";
#endif
                try
                {
                    main.SaveStateAndRestart();
                }
                catch (Exception ex2)
                {
#if BG
                    message += "\r\n\r\nИма незаписани данни. Възстановяването беше неуспешно. Ако проблемът продължи, моля пишете на proektsoft.bg@gmail.com.";
#else
                    message += "\r\n\r\nThere was unsaved data. Calcpad tried, but was unable to recover. If the problem persists, please contact proektsoft.bg@gmail.com.";
#endif
                }
            }
#if BG            
            message += $"\r\n\r\nИнформация за грешката:\r\n\r\n\"{ex.ToString()}\"";
#else
            message += $"\r\n\r\nException details:\r\n\r\n\"{ex.ToString()}\"";
#endif
            File.WriteAllText(logFileName, message);
            Process.Start(new ProcessStartInfo
            {
                FileName = logFileName,
                UseShellExecute = true
            });
        }
#if BG
        private static string GetMessage(Exception ex) => 
@$"В Calcpad възникна неочаквана грешка: ""{ex.Message}""

Източник: ""{ex.Source}""

";
#else
        private static string GetMessage(Exception ex) =>
@$"Unexpected error occured in Calcpad: ""{ex.Message}""

Source: ""{ex.Source}""";

#endif
    }
}