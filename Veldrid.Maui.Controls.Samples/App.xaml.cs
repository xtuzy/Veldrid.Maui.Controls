namespace Veldrid.Maui.Controls.Samples
{
    public partial class App : Application
    {
        public static GraphicsBackend Backend { get; set; }
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
