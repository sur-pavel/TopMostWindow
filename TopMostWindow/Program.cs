using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
 
namespace TopMostWindow
{
    public class SysTrayApp : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd,
            int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private const int HWND_TOPMOST = -1;
        private const int HWND_NOTOPMOST = -2;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOPMOST = 0x0008;
        private List<Process> processesList = new List<Process>();
        KeyboardHook hook = new KeyboardHook();

        [STAThread]
        public static void Main()
        {
            Application.Run(new SysTrayApp());
        }
 
        private NotifyIcon  trayIcon;
        private ContextMenu trayMenu;
 
        public SysTrayApp()
        {
            hook.KeyPressed += hook_KeyPressed;
            // register the control + alt + F12 combination as hot key.
            hook.RegisterHotKey(TopMostWindow.ModifierKeys.Control,
                Keys.F8);
            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);
 
            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon      = new NotifyIcon();
            trayIcon.Text = "TopMostWindow";
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
 
            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible     = true;
        }
 
        protected override void OnLoad(EventArgs e)
        {
            Visible       = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
 
            base.OnLoad(e);
        }
 
        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }
 
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }
 
            base.Dispose(isDisposing);
        }
        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            LoadProcesses();
            foreach (Process process in processesList)
            {
                if (process.MainWindowTitle.Equals(GetActiveWindowTitle()))
                {
                    if (!IsWindowTopMost(GetForegroundWindow()))
                    {
                        SetWindowPos(process.MainWindowHandle,
                            HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                    }
                    else
                    {
                        SetWindowPos(process.MainWindowHandle,
                            HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                    }
                }
            }
        }

        private void LoadProcesses()
        {
            processesList.Clear();
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                //Adding only process which has got Window Title.
                if (process.MainWindowTitle.Length >= 1)
                {
                    processesList.Add(process);
                }
            }
        }

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        public static bool IsWindowTopMost(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            return (exStyle & WS_EX_TOPMOST) == WS_EX_TOPMOST;
        }

    }
}