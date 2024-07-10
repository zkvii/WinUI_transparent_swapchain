using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FluentCountDown.Common;
using FluentCountDown.Helpers;
using FluentCountDown.Interfaces;

namespace FluentCountDown.Utils;

public class SysTrayExtension
{
    private uint _trayIconId;

    public SysTrayExtension()
    {
        _trayIconId = Win32Helpers.RegisterWindowMessage("WM_TRAYICON");
        Win32Helpers.NOTIFYICONDATA nid = new Win32Helpers.NOTIFYICONDATA();
        nid.cbSize = (uint)Marshal.SizeOf(nid);

        nid.hWnd = WindowHelpers.MHwnd;
        nid.uID = 1;
        nid.uFlags = Win32Helpers.NIF_MESSAGE | Win32Helpers.NIF_ICON | Win32Helpers.NIF_TIP;
        nid.uCallbackMessage = _trayIconId;
        nid.hIcon = LoadIconFromFile("Assets/tray.ico");
        nid.szTip = "my tray";

        if (!Win32Helpers.Shell_NotifyIcon(Win32Helpers.NIM_ADD, ref nid))
        {
            Debug.WriteLine("Failed to add tray icon");
        }
    }

    private IntPtr LoadIconFromFile(string iconPath)
    {
        return Win32Helpers.LoadImage(IntPtr.Zero,
            iconPath,
            Win32Helpers.IMAGE_ICON,
            0, 0,
            Win32Helpers.LR_LOADFROMFILE);
    }
}