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
                message += AppMessages.ReportUnhandledExceptionAndClose_NoUnsavedData;
            }
            else
            {
                message += AppMessages.ReportUnhandledExceptionAndClose_NoUnsavedData_RecoveryAttempted;
                try
                {
                    main.SaveStateAndRestart();
                }
                catch
                {
                    message += AppMessages.ReportUnhandledExceptionAndClose_UnsavedData_RecoveryFailed;
                }
            }
            message += string.Format(AppMessages.ExceptionDetails, e);
            File.WriteAllText(logFileName, message);
            Process.Start(new ProcessStartInfo
            {
                FileName = logFileName,
                UseShellExecute = true
            });
            Environment.Exit(System.Runtime.InteropServices.Marshal.GetHRForException((Exception)e));
        }

        private static string GetMessage(Exception e) =>
            string.Format(AppMessages.UnexpectedErrorOccurred, e.Message, e.Source);
    }
}