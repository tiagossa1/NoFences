using System.Runtime.InteropServices;

namespace NoFences.Win32
{
    /// <summary>
    /// Provides access to the undocumented <c>uxtheme.dll</c> API used to opt
    /// application windows and their context menus into dark mode.
    /// </summary>
    public static class DarkModeUtil
    {
        /// <summary>
        /// Sets the process-wide preferred app theme mode.
        /// </summary>
        /// <param name="preferredAppMode">
        /// <c>1</c> to allow dark mode, <c>2</c> to force dark mode.
        /// </param>
        /// <returns>The previous app mode value.</returns>
        [DllImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int SetPreferredAppMode(int preferredAppMode);
    }
}
