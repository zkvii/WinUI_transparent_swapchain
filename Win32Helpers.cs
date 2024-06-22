using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace D3DWinUI3;

static class Win32Helpers
{


    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

    private static int IntPtrToInt32(IntPtr intPtr)
    {
        return unchecked((int)intPtr.ToInt64());
    }

    [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
    public static extern void SetLastError(int dwErrorCode);

    [Flags]
    public enum ExtendedWindowStyles
    {
        // ...
        WS_EX_TOOLWINDOW = 0x00000080,
        WS_EX_TRANSPARENT = 32
        // ...
    }
    [DllImport("User32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
    public static extern long GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("User32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
    public static extern long GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    public const int GWL_STYLE = (-16);
    public const int GWL_EXSTYLE = (-20);
    public const int WS_EX_LAYERED = 0x00080000;
    public const int WS_EX_TRANSPARENT = 0x00000020;

    public const uint LWA_COLORKEY = 0x00000001;
    public const uint LWA_ALPHA = 0x00000002;
    public static long GetWindowLong(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 4)
        {
            return GetWindowLong32(hWnd, nIndex);
        }
        return GetWindowLongPtr64(hWnd, nIndex);
    }

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        int error = 0;
        IntPtr result = IntPtr.Zero;
        // Win32 SetWindowLong doesn't clear error on success
        SetLastError(0);

        if (IntPtr.Size == 4)
        {
            // use SetWindowLong
            Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
            error = Marshal.GetLastWin32Error();
            result = new IntPtr(tempResult);
        }
        else
        {
            // use SetWindowLongPtr
            result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
            error = Marshal.GetLastWin32Error();
        }

        if ((result == IntPtr.Zero) && (error != 0))
        {
            throw new System.ComponentModel.Win32Exception(error);
        }

        return result;
    }
}