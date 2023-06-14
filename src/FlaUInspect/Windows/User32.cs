namespace FlaUInspect.Windows
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <seealso href="https://www.codeproject.com/articles/794407/utility-to-make-important-windows-remain-always-on"/>
    /// <seealso href="https://github.com/oazabir/AlwaysOnTop/blob/master/Source/User32.cs"/>
    public class User32
    {
        /// <summary>
        /// filter function
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);


        #region Windows

        /// <summary>
        /// check if windows visible
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// return windows text
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpWindowText"></param>
        /// <param name="nMaxCount"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        /// <summary>
        /// enumarator on all desktop windows
        /// </summary>
        /// <param name="hDesktop"></param>
        /// <param name="lpEnumCallbackFunction"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("User32.dll")]
        private static extern int FindWindow(String ClassName, String WindowName);

        const int HWND_TOPMOST = -1;
        const int HWND_NOTOPMOST = -2;
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        const UInt32 SW_HIDE = 0;
        const UInt32 SW_SHOWNORMAL = 1;
        const UInt32 SW_NORMAL = 1;
        const UInt32 SW_SHOWMINIMIZED = 2;
        const UInt32 SW_SHOWMAXIMIZED = 3;
        const UInt32 SW_MAXIMIZE = 3;
        const UInt32 SW_SHOWNOACTIVATE = 4;
        const UInt32 SW_SHOW = 5;
        const UInt32 SW_MINIMIZE = 6;
        const UInt32 SW_SHOWMINNOACTIVE = 7;
        const UInt32 SW_SHOWNA = 8;
        const UInt32 SW_RESTORE = 9;

        private static WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hwnd, ref placement);

            return placement;
        }

        public static void AlwaysOnTop(string titlePart, bool isEnabled)
        {
            User32.EnumDelegate filter = delegate (IntPtr hWnd, int lParam)
            {
                StringBuilder strbTitle = new StringBuilder(255);
                int nLength = User32.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                string strTitle = strbTitle.ToString();

                if (User32.IsWindowVisible(hWnd) && string.IsNullOrEmpty(strTitle) == false)
                {
                    if (strTitle.Contains(titlePart))
                    {
                        int insertionIndex = HWND_NOTOPMOST;

                        if (isEnabled)
                        {
                            insertionIndex = HWND_TOPMOST;
                        }

                        SetWindowPos(hWnd, insertionIndex, 0, 0, 0, 0, (int)(SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize));
                    }
                }
                return true;
            };

            User32.EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero);
        }

        public static void RemoveAlwaysOnTop()
        {
            User32.EnumDelegate filter = delegate (IntPtr hWnd, int lParam)
            {
                StringBuilder strbTitle = new StringBuilder(255);
                int nLength = User32.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                string strTitle = strbTitle.ToString();

                if (!string.IsNullOrEmpty(strTitle))
                {
                    WINDOWPLACEMENT placement = GetPlacement(hWnd);
                    if (placement.showCmd == SW_SHOWNORMAL) // Normal
                        SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, (int)(SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize));
                }
                return true;
            };

            User32.EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero);
        }
        #endregion
    }
}
