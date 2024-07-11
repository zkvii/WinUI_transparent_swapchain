using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Numerics;
using Vortice.DXGI;
using Vortice.Mathematics;
using FluentCountDown.Helpers;
using Microsoft.Win32;
using WinUIEx;
using Windows.Graphics;

namespace FluentCountDown;

public sealed partial class MainWindow
{
    private DispatcherTimer timer;

    // private IntPtr mHwnd;
    // private Microsoft.UI.Windowing.OverlappedPresenter _presenter;

    public MainWindow()
    {
        InitializeComponent();

        SwapChainCanvas.Loaded += SwapChainCanvas_Loaded;

        SizeChanged += Window_SizeChanged;

        timer = new DispatcherTimer();
        timer.Tick += Timer_Tick;
        timer.Interval = TimeSpan.FromMilliseconds(16D);
        DirectXHelper.InitDirectX();
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(
            Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            () =>
            {
                Microsoft.UI.WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(WindowHelpers.MHwnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);
                appWindow.Resize(new SizeInt32(500, 800));

                //...
            });
        //if resize here, it will get the wrong size
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

    private bool isDesktop = false;

    private void SetTitle(object sender, RoutedEventArgs e)
    {
        if (!isDesktop)
        {
            ExtendsContentIntoTitleBar = !ExtendsContentIntoTitleBar;
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
        Win32Helpers.SetLayeredWindowAttributes(WindowHelpers.MHwnd, nColorBackground, 255, Win32Helpers.LWA_COLORKEY);
    }

    private void SetWallPaper(object sender, RoutedEventArgs e)
    {
        var hwnd = WindowHelpers.GetWindowHandle(this);
        WindowHelpers.SetWindowWorkerW(hwnd);
        isDesktop = true;
    }


    private void SetTransparent(object sender, RoutedEventArgs e)
    {
        if (!isDesktop)
        {
            var hwnd = WindowHelpers.GetWindowHandle(this);
            WindowHelpers.SetWindowTransparent(hwnd);
        }
    }

    private void RestoreNormal(object sender, RoutedEventArgs e)
    {
        WindowHelpers.SetWindowNormal(WindowHelpers.GetWindowHandle(this));
    }
}