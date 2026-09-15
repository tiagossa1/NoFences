using System;

namespace NoFences.Win32
{
    /// <summary>
    /// Win32 window message identifiers, hit-test results, and <c>SetWindowPos</c>
    /// flag constants used when handling the custom <see cref="System.Windows.Forms.Form.WndProc"/>
    /// logic of borderless, draggable/resizable fence windows.
    /// </summary>
    public static class WindowMessages
    {
        // Hit-test results, used when handling WM_NCHITTEST to support dragging/resizing a borderless form.
        public const int WM_NCHITTEST = 0x84;
        public const int HTCLIENT = 0x1;
        public const int HTCAPTION = 0x2;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;

        public const int WM_SYSCOMMAND = 274;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_MINIMIZE = 0xF020;

        // The low-order 4 bits of a WM_SYSCOMMAND wParam are reserved by Windows for
        // extra signaling (e.g. whether the command was triggered by mouse or keyboard),
        // so callers should mask them off before comparing against an SC_* value.
        public const int SC_COMMAND_MASK = 0xFFF0;

        public const int WM_MOUSELEAVE = 0x02A2;

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_NOZORDER = 0x0004;

        public const int WM_ACTIVATEAPP = 0x001C;
        public const int WM_ACTIVATE = 0x0006;
        public const int WM_SETFOCUS = 0x0007;
        public const int WM_WINDOWPOSCHANGING = 0x0046;

        /// <summary>
        /// A window handle sentinel used with <c>SetWindowPos</c> to place a window
        /// at the bottom of the Z order.
        /// </summary>
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
    }
}
