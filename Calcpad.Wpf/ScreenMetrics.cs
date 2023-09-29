using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Calcpad.Wpf
{
    internal static class ScreenMetrics
    {
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        private enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117
        }

        internal static double GetWindowsScreenScalingFactor()
        {
            using var g = Graphics.FromHwnd(IntPtr.Zero);
            var factor = (g.DpiX + g.DpiY) / 192.0;
            return factor;
        }
    }
}
