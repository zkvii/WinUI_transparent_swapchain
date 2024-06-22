using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SharpGen.Runtime;
using System;
using System.Collections.ObjectModel;
using System.Numerics;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.WIC;
using FeatureLevel = Vortice.Direct3D.FeatureLevel;

namespace D3DWinUI3;

public struct DLinePoint(float x, float y, float pressure)
{
    public float X = x;
    public float Y = y;
    public float Pressure = pressure;
}

public sealed partial class MainWindow
{
    private DispatcherTimer timer;

    private ID3D11Device device;

    // private ID3D11DeviceContext deviceContext;
    private IDXGIDevice dxgiDevice;
    private IDXGISwapChain1 swapChain;
    private ID3D11Texture2D backBuffer;
    private ID3D11RenderTargetView renderTargetView;

    private IDXGISurface dxgiBackBuffer;
    private ID2D1Factory1 d2dFactory;
    private ID2D1Device d2dDevice;
    private ID2D1DeviceContext d2dContext;
    private ID2D1SolidColorBrush d2dbrush;
    private ID2D1Bitmap1 d2dTargetBitmap1;

    private Vortice.WinUI.ISwapChainPanelNative swapChainPanel;

    private IntPtr mHwnd;

    public MainWindow()
    {
        //easy way to remain system title bar
        ExtendsContentIntoTitleBar = true;

        this.InitializeComponent();

        mHwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);


        // hWndDesktopChildSiteBridge = FindWindowEx(hWndMain, IntPtr.Zero, "Microsoft.UI.Content.ContentWindowSiteBridge", null);

        //_presenter.IsResizable = true;

        SwapChainCanvas.Loaded += SwapChainCanvas_Loaded;

        SizeChanged += Window_SizeChanged;

        timer = new DispatcherTimer();
        timer.Tick += Timer_Tick;
        timer.Interval = TimeSpan.FromMilliseconds(16D);
        InitializeDirectX();

        SwapChainCanvas.PointerMoved += SwapChainCanvas_PointerMoved;
        SwapChainCanvas.PointerPressed += SwapChainCanvas_PointerPressed;
        SwapChainCanvas.PointerReleased += SwapChainCanvas_PointerReleased;
        //remove border
        long nExStyle = Win32Helpers.GetWindowLong(mHwnd, Win32Helpers.GWL_EXSTYLE);
        if ((nExStyle & Win32Helpers.WS_EX_LAYERED) == 0)
        {
            Win32Helpers.SetWindowLong(mHwnd, Win32Helpers.GWL_EXSTYLE, (IntPtr)(nExStyle | Win32Helpers.WS_EX_LAYERED));
        }
        //
   
    }

    private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        SwapChainCanvas.Width = args.Size.Width;
        SwapChainCanvas.Height = args.Size.Height;
        ResizeSwapChain();
    }


    public void ResizeSwapChain()
    {
        d2dContext.Target = null;
        // renderTargetView.Dispose();
        backBuffer.Dispose();
        dxgiBackBuffer.Dispose();
        d2dTargetBitmap1.Dispose();

        swapChain.ResizeBuffers(2, (int)SwapChainCanvas.Width, (int)SwapChainCanvas.Height, Format.B8G8R8A8_UNorm,
            SwapChainFlags.None);
        backBuffer = swapChain.GetBuffer<ID3D11Texture2D>(0);
        // renderTargetView = device.CreateRenderTargetView(backBuffer);
        dxgiBackBuffer = backBuffer.QueryInterface<IDXGISurface>();
        var bitmapProperties = new BitmapProperties1();
        bitmapProperties.PixelFormat.Format = Format.B8G8R8A8_UNorm;
        bitmapProperties.PixelFormat.AlphaMode = Vortice.DCommon.AlphaMode.Premultiplied;
        bitmapProperties.BitmapOptions = BitmapOptions.Target | BitmapOptions.CannotDraw;
        bitmapProperties.DpiX = 144;
        bitmapProperties.DpiY = 144;
        d2dTargetBitmap1 = d2dContext.CreateBitmapFromDxgiSurface(dxgiBackBuffer, bitmapProperties);
        d2dContext.Target = d2dTargetBitmap1;
    }


    private bool isPointerPressed = false;

    private ObservableCollection<ObservableCollection<DLinePoint>> dLines = new();
    private ObservableCollection<DLinePoint> points = new();

    private void SwapChainCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        isPointerPressed = false;
        // DLineHelper.SimpleMovingAverageInPlace(points,4);
        DLineHelper.GaussianSmoothingInPlace(points, 10, 4.2);
        // DLineHelper.SplineInterpolationInPlace(points);

        dLines.Add(points);
        points = new ObservableCollection<DLinePoint>();
    }

    private void SwapChainCanvas_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        isPointerPressed = true;
        points ??= new ObservableCollection<DLinePoint>();
    }

    private void SwapChainCanvas_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (!isPointerPressed) return;
        var point = e.GetCurrentPoint(SwapChainCanvas);
        points.Add(new DLinePoint(point.Position._x, point.Position._y, point.Properties.Pressure));
    }

    private void SwapChainCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        SwapChainCanvas.Width= Width;
        SwapChainCanvas.Height = Height;

        CreateSwapChain();

        timer.Start();
    }

    public void InitializeDirectX()
    {
        FeatureLevel[] featureLevels = new FeatureLevel[]
        {
            FeatureLevel.Level_12_1,
            FeatureLevel.Level_12_0,
            FeatureLevel.Level_11_1,
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0,
            FeatureLevel.Level_9_3,
            FeatureLevel.Level_9_2,
            FeatureLevel.Level_9_1
        };

        ID3D11Device tempDevice;
        ID3D11DeviceContext tempContext;

        D3D11.D3D11CreateDevice(
            null,
            DriverType.Hardware,
            DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
            featureLevels,
            out tempDevice,
            out tempContext).CheckError();
        device = tempDevice;
        // deviceContext = tempContext;
        dxgiDevice = device.QueryInterface<IDXGIDevice>();
    }

    public void CreateSwapChain()
    {
        ComObject comObject = new ComObject(SwapChainCanvas);
        swapChainPanel = comObject.QueryInterfaceOrNull<Vortice.WinUI.ISwapChainPanelNative>();
        comObject.Dispose();

        SwapChainDescription1 swapChainDesc = new SwapChainDescription1()
        {
            Stereo = false,
            Width = (int)SwapChainCanvas.Width,
            Height = (int)SwapChainCanvas.Height,
            BufferCount = 2,
            BufferUsage = Usage.RenderTargetOutput,
            Format = Format.B8G8R8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            Scaling = Scaling.Stretch,
            AlphaMode = AlphaMode.Premultiplied,
            Flags = SwapChainFlags.None,
            SwapEffect = SwapEffect.FlipSequential
        };

        IDXGIAdapter1 dxgiAdapter = dxgiDevice.GetParent<IDXGIAdapter1>();
        IDXGIFactory2 dxgiFactory2 = dxgiAdapter.GetParent<IDXGIFactory2>();

        // resize window flick bug
        swapChain = dxgiFactory2.CreateSwapChainForComposition(device, swapChainDesc);

        backBuffer = swapChain.GetBuffer<ID3D11Texture2D>(0);
        // renderTargetView = device.CreateRenderTargetView(backBuffer);
        dxgiBackBuffer = backBuffer.QueryInterface<IDXGISurface>();
        swapChainPanel.SetSwapChain(swapChain);

        d2dFactory = D2D1.D2D1CreateFactory<ID2D1Factory1>(FactoryType.MultiThreaded);
        d2dDevice = d2dFactory.CreateDevice(dxgiDevice);
        d2dContext = d2dDevice.CreateDeviceContext(DeviceContextOptions.None);

        var bitmapProperties = new BitmapProperties1();
        bitmapProperties.PixelFormat.Format = Format.B8G8R8A8_UNorm;
        bitmapProperties.PixelFormat.AlphaMode = Vortice.DCommon.AlphaMode.Premultiplied;
        bitmapProperties.BitmapOptions = BitmapOptions.Target | BitmapOptions.CannotDraw;
        bitmapProperties.DpiX = 144;
        bitmapProperties.DpiY = 144;
        d2dTargetBitmap1 = d2dContext.CreateBitmapFromDxgiSurface(dxgiBackBuffer, bitmapProperties);
        d2dContext.Target = d2dTargetBitmap1;
        d2dbrush = d2dContext.CreateSolidColorBrush(new Color4(0.0f, 0.0f, 1.0f, 1.0f));

        dxgiDevice.Dispose();

        // d2dFactory.DesktopDpi.X, d2dFactory.DesktopDpi.Y,
        // BitmapOptions.Target | BitmapOptions.CannotDraw);
    }

    public void Draw()
    {
        d2dContext.BeginDraw();
        d2dContext.Clear(new Color4(1.0f, 1.0f, 1.0f, 0.0f));
        // d2dContext.FillRectangle(new Rect(100, 100, 200, 200), d2dbrush);
        foreach (var dLine in dLines)
        {
            if (dLine.Count > 1)
            {
                for (int i = 0; i < dLine.Count - 1; i++)
                {
                    var point1 = dLine[i];
                    var point2 = dLine[i + 1];
                    d2dContext.DrawLine(
                        new Vector2(point1.X, point1.Y),
                        new Vector2(point2.X, point2.Y),
                        d2dbrush, 2);
                }
            }
        }

        if (points.Count > 1)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                var point1 = points[i];
                var point2 = points[i + 1];
                d2dContext.DrawLine(
                    new Vector2(point1.X, point1.Y),
                    new Vector2(point2.X, point2.Y),
                    d2dbrush, 2);
            }
        }

        d2dContext.EndDraw();
        swapChain.Present(1, PresentFlags.None);
    }

    private void Timer_Tick(object sender, object e)
    {
        Draw();
    }
}