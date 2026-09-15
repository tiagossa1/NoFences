using System;
using System.Runtime.InteropServices;

namespace NoFences.Win32
{
    /// <summary>
    /// P/Invoke wrappers and helpers for repositioning windows and modifying
    /// their extended styles (e.g. hiding a window from the Alt+Tab switcher).
    /// </summary>
    public static class WindowStyleHelper
    {
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
           int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr DeferWindowPos(IntPtr hWinPosInfo, IntPtr hWnd,
           IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr BeginDeferWindowPos(int nNumWindows);

        [DllImport("user32.dll")]
        public static extern bool EndDeferWindowPos(IntPtr hWinPosInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// Extended window style flags, as passed to <c>GetWindowLong</c>/<c>SetWindowLong</c>
        /// with <see cref="GetWindowLongFields.GWL_EXSTYLE"/>.
        /// </summary>
        [Flags]
        public enum ExtendedWindowStyles
        {
            WS_EX_TOOLWINDOW = 0x00000080,
        }

        /// <summary>
        /// Indices accepted by <c>GetWindowLong</c>/<c>SetWindowLong</c>.
        /// </summary>
        public enum GetWindowLongFields
        {
            GWL_EXSTYLE = -20,
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Sets a window's extended style value, transparently using the correct
        /// 32-bit or 64-bit pointer-sized P/Invoke signature for the current process.
        /// </summary>
        /// <param name="hWnd">The handle of the window to modify.</param>
        /// <param name="nIndex">The style index to set, e.g. <see cref="GetWindowLongFields.GWL_EXSTYLE"/>.</param>
        /// <param name="dwNewLong">The new value for the style.</param>
        /// <returns>The previous value of the specified style.</returns>
        /// <exception cref="System.ComponentModel.Win32Exception">Thrown when the underlying Win32 call fails.</exception>
        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error;
            IntPtr result;

            // Win32 SetWindowLong doesn't clear error on success.
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                var tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if (result == IntPtr.Zero && error != 0)
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int IntSetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);

        /// <summary>
        /// Hides the given window from the Alt+Tab task switcher and sends it to
        /// the bottom of the Z order, without activating it.
        /// </summary>
        /// <param name="handle">The handle of the window to hide.</param>
        public static void HideFromAltTab(IntPtr handle)
        {
            SetWindowPos(handle, WindowMessages.HWND_BOTTOM, 0, 0, 0, 0,
                WindowMessages.SWP_NOSIZE | WindowMessages.SWP_NOMOVE | WindowMessages.SWP_NOACTIVATE);

            var exStyle = (int)GetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }
    }
}
