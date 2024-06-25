using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using Veldrid.Maui.Controls.Samples.Core.Headless;

namespace Veldrid.Maui.Controls.Samples.Pages;

public partial class HeadlessPage : ContentPage
{
    private SKCanvasView skView;
    public GraphicsDevice GraphicsDevice;
    IHeadless headless;
    public HeadlessPage()
    {
        InitializeComponent();
        this.Disappearing += OnDisappearing;
        this.Loaded += HeadlessPage_Loaded;
        this.Unloaded += HeadlessPage_Unloaded;
    }

    //when change page, android will load it, but when quit app from this page, not load it, so something will break.
    private void OnDisappearing(object sender, EventArgs e)
    {
        GraphicsDevice?.Dispose();
        GraphicsDevice = null;
        headless?.Dispose();
    }

    //when change page, windows will load it
    private void HeadlessPage_Unloaded(object sender, EventArgs e)
    {
        GraphicsDevice?.Dispose();
        GraphicsDevice = null;
        headless?.Dispose();
    }

    private void HeadlessPage_Loaded(object sender, EventArgs e)
    {
        
    }

    private void SkView_PaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        e.Surface.Canvas.Clear(SKColors.White);
        if (bitmap != null)
        {
            e.Surface.Canvas.DrawBitmap(bitmap, 0, 0);
        }
    }

    SKBitmap bitmap;

    private void Button_Clicked(object sender, EventArgs e)
    {
        if (skView == null)
        {
            skView = new SKCanvasView();
            skView.HorizontalOptions = LayoutOptions.Center;
            skView.VerticalOptions = LayoutOptions.Center;
            scrollView.Content = skView;
            skView.PaintSurface += SkView_PaintSurface;
        }

        if(GraphicsDevice == null)
        {
            GraphicsDevice = HeaderlessGraphicsDevice.Init();
            BackendChoose.Text = GraphicsDevice.BackendType.ToString();
        }

        if (headless != null)
        {
            headless.Dispose();
        }
        var density = DeviceDisplay.Current.MainDisplayInfo.Density;
        var w = 500;
        var h = 500;
        if (sender == Triangle)
            headless = new HeadlessHelloTriangle(GraphicsDevice, (int)(w * density), (int)(h * density));
        else if (sender == Texture)
            headless = new HeaderlessTextures(GraphicsDevice, (int)(w * density), (int)(h * density));
        headless.CreateResources();
        bitmap = headless.SaveRgba32ToSKBitmap(headless.Draw());
        headless.Dispose();
        skView.WidthRequest = w;
        skView.HeightRequest = h;
        skView.InvalidateSurface();
    }
}