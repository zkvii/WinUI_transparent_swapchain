using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SharpGen.Runtime;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using Windows.Graphics;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.WIC;
using FeatureLevel = Vortice.Direct3D.FeatureLevel;
using FluentCountDown.Helpers;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using MathNet.Numerics;
using static FluentCountDown.Helpers.Win32Helpers;

namespace FluentCountDown;

public sealed partial class MainWindow
{
    private DispatcherTimer timer;

    private IntPtr mHwnd;
    private Microsoft.UI.Windowing.OverlappedPresenter _presenter;

    public MainWindow()
    {
        InitializeComponent();

        //easy way to remain system title bar

        mHwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

        // AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        // Width = 600;
        // Height = 400;
        var winId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(mHwnd);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(winId);

        var hWndDesktopChildSiteBridge =
            Win32Helpers.FindWindowEx(mHwnd, IntPtr.Zero, "Microsoft.UI.Content.ContentWindowSiteBridge", null);

        _presenter = appWindow.Presenter as OverlappedPresenter;
        _presenter.SetBorderAndTitleBar(false, false);
        _presenter.IsResizable = false;

        int nValue = (int)Win32Helpers.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DEFAULT;
        Win32Helpers.DwmSetWindowAttribute(mHwnd, (int)Win32Helpers.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
            ref nValue, Marshal.SizeOf(typeof(int)));

        SwapChainCanvas.Loaded += SwapChainCanvas_Loaded;

        SizeChanged += Window_SizeChanged;

        timer = new DispatcherTimer();
        timer.Tick += Timer_Tick;
        timer.Interval = TimeSpan.FromMilliseconds(16D);
        DirectXHelper.InitDirectX();
        long nExStyle = Win32Helpers.GetWindowLong(mHwnd, Win32Helpers.GWL_EXSTYLE);


        Win32Helpers.SetWindowLong(mHwnd, Win32Helpers.GWL_EXSTYLE, (IntPtr)(nExStyle | Win32Helpers.WS_EX_LAYERED));
        // Win32Helpers.SetWindowLong(mHwnd, Win32Helpers.GWL_EXSTYLE, 0);

        // SetClickThrough();
    }

    private void SetWorkerWParent()
    {
        var hShellViewWin = IntPtr.Zero;
        var hWorkerW = IntPtr.Zero;

        var hProgman = Win32Helpers.FindWindow("Progman", "Program Manager");
        var hDesktopWnd = Win32Helpers.GetDesktopWindow();



        if (hProgman != IntPtr.Zero)
        {
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

            Win32Helpers.SetParent(mHwnd, hShellViewWin);
        }

        // var progman = Win32Helpers.FindWindow("Progman", null);
        //
        // IntPtr shell = Win32Helpers.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
        //
        // IntPtr workerw = IntPtr.Zero;
        // workerw = Win32Helpers.FindWindowEx(IntPtr.Zero, workerw, "WorkerW", null);
        // if (shell != IntPtr.Zero)
        // {
        //     // Win32Helpers.SetParent(mHwnd, shell);
        // }
    }

    private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        SwapChainCanvas.Width = args.Size.Width;
        SwapChainCanvas.Height = args.Size.Height;
        Debug.WriteLine($"Window_SizeChanged {args.Size.Width} {args.Size.Height}");
        DirectXHelper.ResizeSwapChain((int)args.Size.Width, (int)args.Size.Height);
    }


    private void SwapChainCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        SwapChainCanvas.Width = AppWindow.Size.Width;
        SwapChainCanvas.Height = AppWindow.Size.Height;

        DirectXHelper.CreateSwapChain(SwapChainCanvas);

        timer.Start();
    }


    private void Timer_Tick(object sender, object e)
    {
        Draw();
    }

    public void Draw()
    {
        DirectXHelper.D2dContext.BeginDraw();

        DirectXHelper.D2dContext.Clear(Colors.Transparent);

        //draw a line
        DirectXHelper.D2dContext.DrawLine(new Vector2(0, 0), new Vector2(100, 100), DirectXHelper.D2dbrush, 2);

        DirectXHelper.D2dContext.EndDraw();

        DirectXHelper.SwapChain.Present(1, PresentFlags.None);
    }

    private void ToggleTitle(object sender, RoutedEventArgs e)
    {
        if (ExtendsContentIntoTitleBar != true)
        {
            ExtendsContentIntoTitleBar = true;
            _presenter.SetBorderAndTitleBar(true, true);
            // SetClickThrough();
        }
        else
        {
            ExtendsContentIntoTitleBar = false;
            _presenter.SetBorderAndTitleBar(false, false);
            _presenter.IsResizable = false;
        }
    }

    private void SetClickThrough()
    {
        int nAppsUseLightTheme = 0;
        int nSystemUsesLightTheme = 0;
        string sPathKey = @"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";
        using (RegistryKey rkLocal = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
        {
            using (RegistryKey rk = rkLocal.OpenSubKey(sPathKey, false))
            {
                nAppsUseLightTheme = (int)rk.GetValue("AppsUseLightTheme", 0);
                nSystemUsesLightTheme = (int)rk.GetValue("SystemUsesLightTheme", 0);
            }
        }

        uint nColorBackground = (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.Black);
        //if (nAppsUseLightTheme == 1 || nSystemUsesLightTheme == 1)
        if (nAppsUseLightTheme == 1)
        {
            nColorBackground = (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.White);
            // not refreshed when mouse over...
            // myButton.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
        }

        // set clickthrough
        Win32Helpers.SetLayeredWindowAttributes(mHwnd, nColorBackground, 255, Win32Helpers.LWA_COLORKEY);
    }

    private void SetWallPaper(object sender, RoutedEventArgs e)
    {
        SetWorkerWParent();
    }
}