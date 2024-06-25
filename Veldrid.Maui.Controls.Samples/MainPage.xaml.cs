using Veldrid.Vk;

namespace Veldrid.Maui.Controls.Samples
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent(); 
#if __IOS__
            var vk = Veldrid.Maui.Controls.Platform.iOSMac.VulkanLoader.GetApi();
            SilkNETVk.Init(vk);
#endif
        }
    }
}
