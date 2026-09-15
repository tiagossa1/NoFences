using System;
using System.Runtime.InteropServices;

namespace NoFences.Win32
{
    /// <summary>
    /// Provides helpers for pinning a window to the desktop layer and
    /// preventing it from being minimized or maximized.
    /// </summary>
    public class DesktopUtil
    {
        private const int GWL_STYLE = -16;
        private const int GWL_HWNDPARENT = -8;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WS_MINIMIZEBOX = 0x00020000;

        // GetWindowLongPtr/SetWindowLongPtr must be used (instead of the 32-bit-only
        // GetWindowLong/SetWindowLong) so pointer-sized values are not truncated on x64.
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        /// <summary>
        /// Removes the minimize and maximize buttons/behavior from the window's style.
        /// </summary>
        /// <param name="handle">The handle of the window to modify.</param>
        public static void PreventMinimize(IntPtr handle)
        {
            var windowStyle = GetWindowLongPtr(handle, GWL_STYLE).ToInt64();
            var newStyle = windowStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX;
            SetWindowLongPtr(handle, GWL_STYLE, new IntPtr(newStyle));
        }

        /// <summary>
        /// Reparents the window to the desktop's Program Manager window so it
        /// behaves as if it were part of the desktop.
        /// </summary>
        /// <param name="handle">The handle of the window to reparent.</param>
        public static void GlueToDesktop(IntPtr handle)
        {
            var progman = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null);
            SetWindowLongPtr(handle, GWL_HWNDPARENT, progman);
        }
    }
}