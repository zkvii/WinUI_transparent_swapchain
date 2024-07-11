using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using MathNet.Numerics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using static FluentCountDown.Helpers.Win32Helpers;
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

        public static void UnSetWindowWorkerW(IntPtr hwnd)
        {
            Win32Helpers.SetParent(hwnd, IntPtr.Zero);
        }


        // var winId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        // var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(winId);
        //
        // appWindow.TitleBar.ExtendsContentIntoTitleBar= true;
        public static void SetWindowTransparent(IntPtr hwnd)
        {
            Microsoft.UI.WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);

            appWindow.Resize(new SizeInt32(500,800));
            // appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

            // appWindow.ResizeClient(new SizeInt32(600,800));

            long nExStyle = GetWindowLong(hwnd, Win32Helpers.GWL_EXSTYLE);

            SetWindowLong(hwnd, GWL_EXSTYLE, (IntPtr)(nExStyle & ~WS_EX_LAYERED));
            RedrawWindow(hwnd, IntPtr.Zero, IntPtr.Zero, RDW_ERASE | RDW_INVALIDATE | RDW_FRAME | RDW_ALLCHILDREN);
            SetWindowLong(hwnd, GWL_EXSTYLE, (IntPtr)(nExStyle|WS_EX_LAYERED));


            if (!SetPictureToLayeredWindow(hwnd)) return;
            GetWindowRect(hwnd, out var rectWnd);
            SetWindowPos(hwnd, IntPtr.Zero, rectWnd.left, rectWnd.top - 1, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW | SWP_FRAMECHANGED);
            SetWindowPos(hwnd, IntPtr.Zero, rectWnd.left, rectWnd.top, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW | SWP_FRAMECHANGED);
        }


        // public static IntPtr LoadLayerPicture()
        // {
        //
        // }

        public static IntPtr CreateTransparentBitmap(int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics gfx = Graphics.FromImage(bitmap))
            {
                // 创建一个全透明的矩形
                gfx.Clear(Color.FromArgb(0, 0, 0, 0));

                // 在 bitmap 上绘制一些图形
                using (var brush = new SolidBrush(Color.FromArgb(128, 255, 0, 0))) // 半透明红色
                {
                    // gfx.FillEllipse(brush, 10, 10, width - 20, height - 20);
                }
            }

            return bitmap.GetHbitmap(Color.FromArgb(0));
        }

        public static bool SetPictureToLayeredWindow(IntPtr hWnd)
        {
            Win32Helpers.RECT rectWnd;
            Win32Helpers.GetWindowRect(hWnd, out rectWnd);
            IntPtr hBitmap = CreateTransparentBitmap(rectWnd.right-rectWnd.left, rectWnd.bottom-rectWnd.top);
            Win32Helpers.BITMAP bm;
            Win32Helpers.GetObject(hBitmap, Marshal.SizeOf(typeof(Win32Helpers.BITMAP)), out bm);
            System.Drawing.Size sizeBitmap = new System.Drawing.Size(bm.bmWidth, bm.bmHeight);

            IntPtr hDCScreen = Win32Helpers.GetDC(IntPtr.Zero);
            IntPtr hDCMem = Win32Helpers.CreateCompatibleDC(hDCScreen);
            IntPtr hBitmapOld = Win32Helpers.SelectObject(hDCMem, hBitmap);

            Win32Helpers.BLENDFUNCTION bf = new Win32Helpers.BLENDFUNCTION();
            bf.BlendOp = Win32Helpers.AC_SRC_OVER;
            bf.SourceConstantAlpha = 255;
            bf.AlphaFormat = Win32Helpers.AC_SRC_ALPHA;

      

            Point ptSrc = new Point();
            var ptDest = new Point(rectWnd.left, rectWnd.top);

            IntPtr pptSrc = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Point)));
            Marshal.StructureToPtr(ptSrc, pptSrc, false);

            IntPtr pptDest = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Point)));
            Marshal.StructureToPtr(ptDest, pptDest, false);

            IntPtr psizeBitmap = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Size)));
            Marshal.StructureToPtr(sizeBitmap, psizeBitmap, false);

            bool bRet = Win32Helpers.UpdateLayeredWindow(hWnd, hDCScreen, pptDest, psizeBitmap, hDCMem, pptSrc, 0, ref bf, Win32Helpers.ULW_ALPHA);
            //int nErr = Marshal.GetLastWin32Error();

            Marshal.FreeHGlobal(pptSrc);
            Marshal.FreeHGlobal(pptDest);
            Marshal.FreeHGlobal(psizeBitmap);

            Win32Helpers.SelectObject(hDCMem, hBitmapOld);
            Win32Helpers.DeleteDC(hDCMem);
            Win32Helpers.ReleaseDC(IntPtr.Zero, hDCScreen);

            return bRet;
        }
        public static void SetWindowNormal(IntPtr hwnd)
        {
            UnSetWindowWorkerW(hwnd);
            SetWindowLong(hwnd, GWL_EXSTYLE, (IntPtr)(256));
            RedrawWindow(hwnd, IntPtr.Zero, IntPtr.Zero, RDW_ERASE | RDW_INVALIDATE | RDW_FRAME | RDW_ALLCHILDREN);
            GetWindowRect(hwnd, out var rectWnd);
            SetWindowPos(hwnd, IntPtr.Zero, rectWnd.left, rectWnd.top - 1, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW | SWP_FRAMECHANGED);

        }

    }
}