using System;
using System.Windows;
using System.Windows.Interop;

namespace Calcpad.Wpf
{
    internal static class ScreenMetrics
    {
        internal static double GetWindowsScreenScalingFactor()
        {
            var source = PresentationSource.FromVisual(Application.Current.MainWindow);
            if (source?.CompositionTarget != null)
                return source.CompositionTarget.TransformToDevice.M11;

            return 1.0;
        }
    }
}