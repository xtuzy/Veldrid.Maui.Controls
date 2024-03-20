using Veldrid.Maui.Controls.Samples.Core;
using Veldrid.Maui.Controls.Samples.Core.LearnOpenGL;

namespace Veldrid.Maui.Controls.Samples.Pages;

public partial class LearnOpenGLPage : ContentPage
{
    VeldridView VeldridView { get; set; }
    public LearnOpenGLPage()
    {
        InitializeComponent();
        Backend.Text = App.Backend.ToString();
        BackendChoose.BackendChanged += () =>
        {
            Backend.Text = App.Backend.ToString();
        };
        RenderDocCapture.Init();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        if (VeldridView == null)
        {
            VeldridView = new VeldridView();
            VeldridView.Backend = App.Backend;
            VeldridView.AutoReDraw = true;
            scrollView.RemoveAt(0);
            scrollView.IsClippedToBounds = true;
            scrollView.Add(VeldridView);
        }

        if (sender == HelloTriangle)
        {
            VeldridView.Drawable = new HelloTriangle();
        }
        else if (sender == Shaders_Uniform)
        {
            VeldridView.Drawable = new Shaders_Uniform();
        }
        else if (sender == Textures)
        {
            VeldridView.Drawable = new Textures();
        }
        else if (sender == Cube)
        {
            VeldridView.Drawable = new CoordinateSystems_More3D();
        }
        else if (sender == MoreCube)
        {
            VeldridView.Drawable = new CoordinateSystems_MoreCubes();
        }
    }

    private void RenderDocButton_Clicked(object sender, EventArgs e)
    {
        if (RenderDocButton.Text == "StartCapture")
        {
            RenderDocCapture.StartCapture();
            RenderDocButton.Text = "EndCapture";
        }
        else
        {
            RenderDocCapture.EndCapture();
            RenderDocButton.Text = "StartCapture";
        }
    }
}