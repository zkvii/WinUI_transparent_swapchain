using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Numerics;
using Vortice.DXGI;
using Vortice.Mathematics;
using FluentCountDown.Helpers;
using Microsoft.Win32;

namespace FluentCountDown;

public sealed partial class MainWindow
{
    private DispatcherTimer timer;

    // private IntPtr mHwnd;
    // private Microsoft.UI.Windowing.OverlappedPresenter _presenter;

    public MainWindow()
    {
        InitializeComponent();
        // ExtendsContentIntoTitleBar= true;

        //easy way to remain system title bar

        // mHwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

        // AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        // Width = 600;
        // Height = 400;
        // var winId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(WindowHelpers.MHwnd);
        // var appWindow = AppWindow.GetFromWindowId(winId);

        // var hWndDesktopChildSiteBridge =
        //     Win32Helpers.FindWindowEx(WindowHelpers.MHwnd, IntPtr.Zero, "Microsoft.UI.Content.ContentWindowSiteBridge", null);

        // _presenter = appWindow.Presenter as OverlappedPresenter;
        // _presenter.SetBorderAndTitleBar(false, false);
        // _presenter.IsResizable = false;

        // int nValue = (int)Win32Helpers.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DEFAULT;
        // Win32Helpers.DwmSetWindowAttribute(WindowHelpers.MHwnd, (int)Win32Helpers.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
        //     ref nValue, Marshal.SizeOf(typeof(int)));

        SwapChainCanvas.Loaded += SwapChainCanvas_Loaded;

        SizeChanged += Window_SizeChanged;

        timer = new DispatcherTimer();
        timer.Tick += Timer_Tick;
        timer.Interval = TimeSpan.FromMilliseconds(16D);
        DirectXHelper.InitDirectX();
        // long nExStyle = Win32Helpers.GetWindowLong(WindowHelpers.MHwnd, Win32Helpers.GWL_EXSTYLE);
        //
        //
        // Win32Helpers.SetWindowLong(WindowHelpers.MHwnd, Win32Helpers.GWL_EXSTYLE, (IntPtr)(nExStyle | Win32Helpers.WS_EX_LAYERED));
        // Win32Helpers.SetWindowLong(WindowHelpers.GetWindowHandle(this), Win32Helpers.GWL_EXSTYLE, (IntPtr)(nExStyle | Win32Helpers.WS_EX_LAYERED));

        // SetClickThrough();
        // CreateTrayIcon();
    }

   

 

 

    private bool isDesktop = false;

    private void SetWorkerWParent()
    {
        var hwnd = WindowHelpers.GetWindowHandle(this);

        if (!isDesktop)
        {
            WindowHelpers.SetWindowWorkerW(hwnd);
            isDesktop = true;
        }
        else
        {
            WindowHelpers.RestoreNormal(hwnd);
            isDesktop = false;
        }
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
        //set normal
        WindowHelpers.RestoreNormal(WindowHelpers.GetWindowHandle(this));
        ExtendsContentIntoTitleBar = ExtendsContentIntoTitleBar != true;
        // _presenter.SetBorderAndTitleBar(true, true);
        // SetClickThrough();

        // _presenter.SetBorderAndTitleBar(false, false);
        // _presenter.IsResizable = false;
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
        Win32Helpers.SetLayeredWindowAttributes(WindowHelpers.MHwnd, nColorBackground, 255, Win32Helpers.LWA_COLORKEY);
    }

    private void SetWallPaper(object sender, RoutedEventArgs e)
    {
        SetWorkerWParent();
    }


    private void ToggleTransparent(object sender, RoutedEventArgs e)
    {
        var hwnd= WindowHelpers.GetWindowHandle(this);
        WindowHelpers.SetWindowTransparent(hwnd);
        
    }
}