namespace Veldrid.Maui.Controls.Samples;

public partial class BackendChooseControl : ContentView
{
    public Action BackendChanged;
    public BackendChooseControl()
    {
        InitializeComponent();
        //Disable backend
        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan))
        {
            if (OperatingSystem.IsWindows())
                Vulkan.IsEnabled = false;
            else
                Vulkan.IsEnabled = true;
        }
        else
            Vulkan.IsEnabled = false;
        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Direct3D11))
            D3D11.IsEnabled = true;
        else
            D3D11.IsEnabled = false;
        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGLES))
        {
            if (OperatingSystem.IsWindows())
                OpenGLES.IsEnabled = false;
            else
                OpenGLES.IsEnabled = true;
        }
        else
            OpenGLES.IsEnabled = false;
        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal))
            Metal.IsEnabled = true;
        else
            Metal.IsEnabled = false;

        //Default
        if (OperatingSystem.IsWindows())
        {
            D3D11.IsChecked = true;
        }
        else if (OperatingSystem.IsIOS())
        {
            Metal.IsChecked = true;
        }
        else if (OperatingSystem.IsAndroid())
        {
            if (GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGLES))
                OpenGLES.IsChecked = true;
            else
                Vulkan.IsChecked = true;
        }
    }

    private void Backend_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value == true)
        {
            if (sender == Vulkan)
            {
                App.Backend = GraphicsBackend.Vulkan;
            }
            else if (sender == OpenGLES)
            {
                App.Backend = GraphicsBackend.OpenGLES;
            }
            else if (sender == Metal)
            {
                App.Backend = GraphicsBackend.Metal;
            }
            else if (sender == D3D11)
            {
                App.Backend = GraphicsBackend.Direct3D11;
            }
        }
        BackendChanged?.Invoke();
    }
}