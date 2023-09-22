using SkiaSharp;

namespace Veldrid.Maui.Controls.Samples.Core.Headless
{
    public interface IHeadless : IDisposable
    {
        void CreateResources();
        byte[] Draw();
        SKBitmap SaveRgba32ToSKBitmap(byte[] bytes);
    }
}