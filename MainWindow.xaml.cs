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

namespace FluentCountDown;

public struct DLinePoint(float x, float y, float pressure)
{
    public float X = x;
    public float Y = y;
    public float Pressure = pressure;
}

public sealed partial class MainWindow
{
    private DispatcherTimer timer;

    private IntPtr mHwnd;

    public MainWindow()
    {
        //easy way to remain system title bar
        ExtendsContentIntoTitleBar = true;

        InitializeComponent();
        

        mHwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

        AppWindow.MoveAndResize(new RectInt32(100,100,400,600));

        // Width = 600;
        // Height = 400;
        // var winId= Microsoft.UI.Win32Interop.GetWindowIdFromWindow(mHwnd);
        // var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(winId);

        // var hWndDesktopChildSiteBridge = Win32Helpers.FindWindowEx(mHwnd, IntPtr.Zero, "Microsoft.UI.Content.ContentWindowSiteBridge", null);

       // var presenter=appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;

       // if (presenter != null)
       // {
       //     // presenter.IsResizable = true;
       // }

       SwapChainCanvas.Loaded += SwapChainCanvas_Loaded;

        SizeChanged += Window_SizeChanged;

        timer = new DispatcherTimer();
        timer.Tick += Timer_Tick;
        timer.Interval = TimeSpan.FromMilliseconds(16D);
        DirectXHelper.InitDirectX();


        //
   
    }

    private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        SwapChainCanvas.Width= args.Size.Width;
        SwapChainCanvas.Height = args.Size.Height;
        Debug.WriteLine($"Window_SizeChanged {args.Size.Width} {args.Size.Height}");
        DirectXHelper.ResizeSwapChain((int)args.Size.Width,(int)args.Size.Height);
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
        //remove border
        long nExStyle = Win32Helpers.GetWindowLong(mHwnd, Win32Helpers.GWL_EXSTYLE);
        // if ((nExStyle & Win32Helpers.WS_EX_LAYERED) == 0)
        // {
        //     Win32Helpers.SetWindowLong(mHwnd, Win32Helpers.GWL_EXSTYLE, (IntPtr)(nExStyle | Win32Helpers.WS_EX_LAYERED));
        // }
       Win32Helpers.SetWindowLong(mHwnd, Win32Helpers.GWL_EXSTYLE, (IntPtr)(nExStyle & ~Win32Helpers.WS_EX_LAYERED));
       Win32Helpers.RedrawWindow(mHwnd, IntPtr.Zero, IntPtr.Zero, Win32Helpers.RDW_ERASE | Win32Helpers.RDW_INVALIDATE | Win32Helpers.RDW_FRAME | Win32Helpers.RDW_ALLCHILDREN);

        nExStyle = Win32Helpers.GetWindowLong(mHwnd, Win32Helpers.GWL_EXSTYLE);

        Win32Helpers.SetWindowLong(mHwnd, Win32Helpers.GWL_EXSTYLE, (IntPtr)(nExStyle | Win32Helpers.WS_EX_LAYERED));
    }
}