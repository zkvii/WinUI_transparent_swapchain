using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentCountDown.Helpers;

static class Win32Helpers
{

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

    public const int RDW_INVALIDATE = 0x0001;
    public const int RDW_INTERNALPAINT = 0x0002;
    public const int RDW_ERASE = 0x0004;

    public const int RDW_VALIDATE = 0x0008;
    public const int RDW_NOINTERNALPAINT = 0x0010;
    public const int RDW_NOERASE = 0x0020;

    public const int RDW_NOCHILDREN = 0x0040;
    public const int RDW_ALLCHILDREN = 0x0080;

    public const int RDW_UPDATENOW = 0x0100;
    public const int RDW_ERASENOW = 0x0200;

    public const int RDW_FRAME = 0x0400;
    public const int RDW_NOFRAME = 0x0800;




    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);


    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern nint IntSetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern int IntSetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    private static int IntPtrToInt32(nint intPtr)
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
    public static extern long GetWindowLong32(nint hWnd, int nIndex);

    [DllImport("User32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
    public static extern long GetWindowLongPtr64(nint hWnd, int nIndex);

    public const int GWL_STYLE = -16;
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_LAYERED = 0x00080000;
    public const int WS_EX_TRANSPARENT = 0x00000020;

    public const uint LWA_COLORKEY = 0x00000001;
    public const uint LWA_ALPHA = 0x00000002;
    public static long GetWindowLong(nint hWnd, int nIndex)
    {
        if (nint.Size == 4)
        {
            return GetWindowLong32(hWnd, nIndex);
        }
        return GetWindowLongPtr64(hWnd, nIndex);
    }

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool SetLayeredWindowAttributes(nint hwnd, uint crKey, byte bAlpha, uint dwFlags);

    public static nint SetWindowLong(nint hWnd, int nIndex, nint dwNewLong)
    {
        int error = 0;
        nint result = nint.Zero;
        // Win32 SetWindowLong doesn't clear error on success
        SetLastError(0);

        if (nint.Size == 4)
        {
            // use SetWindowLong
            int tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
            error = Marshal.GetLastWin32Error();
            result = new nint(tempResult);
        }
        else
        {
            // use SetWindowLongPtr
            result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
            error = Marshal.GetLastWin32Error();
        }

        if (result == nint.Zero && error != 0)
        {
            throw new System.ComponentModel.Win32Exception(error);
        }

        return result;
    }
}