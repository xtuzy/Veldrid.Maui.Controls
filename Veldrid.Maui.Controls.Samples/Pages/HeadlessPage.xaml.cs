using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using Veldrid.Maui.Controls.Samples.Core.Headless;

namespace Veldrid.Maui.Controls.Samples.Pages;

public partial class HeadlessPage : ContentPage
{
    private SKCanvasView skView;

    public HeadlessPage()
	{
		InitializeComponent();
	}

    private void SkView_PaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        if(bitmap != null)
        {
            e.Surface.Canvas.DrawBitmap(bitmap, 0, 0);
        }
    }

    SKBitmap bitmap;
    private void Texture_Clicked(object sender, EventArgs e)
    {
        if(skView == null)
        {
            skView = new SKCanvasView();
            scrollView.Content = skView;
            skView.PaintSurface += SkView_PaintSurface;
        }

        var headless = new HeaderlessTextures();
        bitmap = headless.SaveRgba32ToSKBitmap(headless.Draw());
        headless.Dispose();
        skView.WidthRequest = bitmap.Width;
        skView.HeightRequest = bitmap.Height;
        skView.InvalidateSurface();
    }

    private void Triangle_Clicked(object sender, EventArgs e)
    {
        if (skView == null)
        {
            skView = new SKCanvasView();
            scrollView.Content = skView;
            skView.PaintSurface += SkView_PaintSurface;
        }

        var headless = new HeadlessHelloTriangle();
        bitmap = headless.SaveRgba32ToSKBitmap(headless.Draw());
        headless.Dispose();
        skView.WidthRequest = bitmap.Width;
        skView.HeightRequest = bitmap.Height;
        skView.InvalidateSurface();
    }
}