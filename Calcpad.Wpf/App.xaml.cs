using System;
using System.Diagnostics;
using System.IO;
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
            ReportUnhandledExceptionAndClose((Exception)e.ExceptionObject);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            DispatcherUnhandledException -= Application_DispatcherUnhandledException;
            e.Handled = true;
            ReportUnhandledExceptionAndClose(e.Exception);
        }

        private static void ReportUnhandledExceptionAndClose(Exception e) 
        {

            MainWindow main = (MainWindow)Current.MainWindow;
            var logFileName = Path.GetTempFileName();
            var message = GetMessage(e);
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
                catch
                {
#if BG
                    message += "\r\n\r\nИма незаписани данни. Възстановяването беше неуспешно. Ако проблемът продължи, моля пишете на proektsoft.bg@gmail.com.";
#else
                    message += "\r\n\r\nThere was unsaved data. Calcpad tried, but was unable to recover. If the problem persists, please contact proektsoft.bg@gmail.com.";
#endif
                }
            }
#if BG            
            message += $"\r\n\r\nИнформация за грешката:\r\n\r\n\"{e.ToString()}\"";
#else
            message += $"\r\n\r\nException details:\r\n\r\n\"{e}\"";
#endif
            File.WriteAllText(logFileName, message);
            Process.Start(new ProcessStartInfo
            {
                FileName = logFileName,
                UseShellExecute = true
            });
            Environment.Exit(System.Runtime.InteropServices.Marshal.GetHRForException((Exception)e));
        }
#if BG
        private static string GetMessage(Exception e) => 
@$"В Calcpad възникна неочаквана грешка: ""{e.Message}""

Източник: ""{e.Source}""

";
#else
        private static string GetMessage(Exception e) =>
@$"Unexpected error occured in Calcpad: ""{e.Message}""

Source: ""{e.Source}""";

#endif
    }
}