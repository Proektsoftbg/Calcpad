using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Calcpad.Wpf
{
    internal static class ScreenMetrics
    {
        internal static double GetWindowsScreenScalingFactor()
        {
            using var g = Graphics.FromHwnd(IntPtr.Zero);
            var factor = (g.DpiX + g.DpiY) / 192.0;
            return factor;
        }
    }
}
