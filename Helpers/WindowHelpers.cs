using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using Microsoft.UI.Xaml;
using Window = Microsoft.UI.Xaml.Window;

namespace FluentCountDown.Helpers
{
    public class WindowHelpers
    {
        public static List<IntPtr> HwndList = new();

        public static IntPtr MHwnd;

        public static Window CreateMainWindow()
        {
            var window = new MainWindow();
            MHwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            HwndList.Add(MHwnd);

            return window;
        }

        public static IntPtr GetWindowHandle(Window window)
        {
            return WinRT.Interop.WindowNative.GetWindowHandle(window);
        }

        public static void SetWindowWorkerW(IntPtr hwnd)
        {
            var hShellViewWin = IntPtr.Zero;
            var hWorkerW = IntPtr.Zero;

            var hProgman = Win32Helpers.FindWindow("Progman", "Program Manager");
            var hDesktopWnd = Win32Helpers.GetDesktopWindow();


            if (hProgman == IntPtr.Zero) return;
            // Get and load the main List view window containing the icons.
            hShellViewWin = Win32Helpers.FindWindowEx(hProgman, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (hShellViewWin == IntPtr.Zero)
            {
                // When this fails (picture rotation is turned ON), then look for the WorkerW windows list to get the
                // correct desktop list handle.
                // As there can be multiple WorkerW windows, iterate through all to get the correct one
                do
                {
                    hWorkerW = Win32Helpers.FindWindowEx(hDesktopWnd, hWorkerW, "WorkerW", null);
                    hShellViewWin = Win32Helpers.FindWindowEx(hWorkerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                } while (hShellViewWin == IntPtr.Zero && hWorkerW != IntPtr.Zero);
            }

            Win32Helpers.SetParent(hwnd, hShellViewWin);
        }

        public static void RestoreNormal(IntPtr hwnd)
        {
            Win32Helpers.SetParent(hwnd, IntPtr.Zero);
        }


        // var winId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        // var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(winId);
        //
        // appWindow.TitleBar.ExtendsContentIntoTitleBar= true;
        public static void SetWindowTransparent(IntPtr hwnd)
        {

            // var winId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            // var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(winId);
            //

            // var presenter= appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
            long nExStyle = Win32Helpers.GetWindowLong(hwnd, Win32Helpers.GWL_EXSTYLE);
            //
            //
            Win32Helpers.SetWindowLong(hwnd, Win32Helpers.GWL_STYLE,
                (IntPtr)(0x00200000L));
            // Win32Helpers.SetWindowLong(mHwnd, Win32Helpers.GWL_EXSTYLE, 0);

            // Win32Helpers.SetLayeredWindowAttributes(hwnd, 0, 128, Win32Helpers.LWA_ALPHA);
            Win32Helpers.SetWindowPos(hwnd, IntPtr.Zero, 0, 0,
                0, 0, Win32Helpers.SWP_FRAMECHANGED
                      | Win32Helpers.SWP_NOMOVE
                      | Win32Helpers.SWP_NOSIZE 
                      | Win32Helpers.SWP_NOZORDER 
                      | Win32Helpers.SWP_SHOWWINDOW);
            // presenter.SetBorderAndTitleBar(false,false);
            // Win32Helpers.ShowWindow(hwnd, Win32Helpers.SW_SHOW);
        }

        public static void SetWindowNormal(IntPtr hwnd)
        {
            var winId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(winId);

            appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

            long nExStyle = Win32Helpers.GetWindowLong(hwnd, Win32Helpers.GWL_EXSTYLE);
            //
            //
            // Win32Helpers.SetWindowLong(hwnd, Win32Helpers.GWL_EXSTYLE, (IntPtr)(nExStyle | Win32Helpers.WS_EX_LAYERED));
            Win32Helpers.SetWindowLong(hwnd, Win32Helpers.GWL_EXSTYLE, 0);

            // Win32Helpers.SetLayeredWindowAttributes(hwnd, 0, 128, Win32Helpers.LWA_ALPHA);
            Win32Helpers.SetWindowPos(hwnd, IntPtr.Zero, 0, 0,
                0, 0,
                Win32Helpers.SWP_FRAMECHANGED | Win32Helpers.SWP_NOMOVE | Win32Helpers.SWP_NOSIZE |
                Win32Helpers.SWP_NOZORDER | Win32Helpers.SWP_SHOWWINDOW);
        }
    }
}