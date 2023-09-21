﻿using SkiaSharp;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid.SPIRV;

namespace Veldrid.Maui.Controls.Samples.Core.Headless
{
    public class HeadlessHelloTriangle : //BaseGpuDrawable, 
        IDisposable
    {
        public HeadlessHelloTriangle()
        {
            GraphicsDevice = HeaderlessGraphicsDevice.Init();
            ResourceFactory = GraphicsDevice.ResourceFactory;
            CreateResources(ResourceFactory);
        }

        #region  Add for headless
        GraphicsDevice GraphicsDevice;
        ResourceFactory ResourceFactory;
        int Width = 500;
        int Height = 500;
        Texture _offscreenReadOut;
        Texture _offscreenColor;
        Framebuffer _offscreenFB;
        #endregion

        private DeviceBuffer _vertexBuffer;
        private Pipeline _pipeline;
        private CommandList _commandList;
        private Shader[] _shaders;
        private DeviceBuffer _indexBuffer;
        ushort[] triangleIndices;
        protected unsafe void CreateResources(ResourceFactory factory)
        {
            //vertices data of a triangle
            Vector3[] vertices = new Vector3[]
            {
                new Vector3( -0.5f, -0.5f, 0.0f),
                new Vector3(0.5f, -0.5f, 0.0f),
                new Vector3(0.0f, 0.5f, 0.0f),
            };

            // create Buffer for vertices data
            BufferDescription vbDescription = new BufferDescription(
                (uint)(vertices.Length * sizeof(Vector3)),
                BufferUsage.VertexBuffer);
            _vertexBuffer = factory.CreateBuffer(vbDescription);
            GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertices);// update data to Buffer

            // Index data
            triangleIndices = new ushort[] { 0, 1, 2 };// CounterClockwise
            // create IndexBuffer
            BufferDescription ibDescription = new BufferDescription(
                (uint)(triangleIndices.Length * sizeof(ushort)),
                BufferUsage.IndexBuffer);
            _indexBuffer = factory.CreateBuffer(ibDescription);
            GraphicsDevice.UpdateBuffer(_indexBuffer, 0, triangleIndices);// update data to Buffer

            string vertexCode = @"
#version 460

layout(location = 0) in vec3 aPos;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
}";

            string fragmentCode = @"
#version 460

layout(location = 0) out vec4 FragColor;

void main()
{
    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
}";
            var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertexCode), "main");
            var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragmentCode), "main");

            if (factory.BackendType == GraphicsBackend.OpenGL)
            {
                var vertexShader = factory.CreateShader(vertexShaderDesc);
                var fragmentShader = factory.CreateShader(fragmentShaderDesc);
                _shaders = new Shader[] { vertexShader, fragmentShader };
            }
            if (factory.BackendType == GraphicsBackend.Vulkan)
            {
                vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, spirvVertexCode, "main");
                fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, spirvFragmentCode, "main");
                _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
            }
            else
                _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);// we use glsl, when not use opengl we need convert it to other 


            // VertexLayout tell Veldrid we store what in Vertex Buffer, it need match with vertex.glsl
            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
               new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));

            #region Add for Headless 
            _offscreenReadOut = factory.CreateTexture(TextureDescription.Texture2D((uint)Width, (uint)Height, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Staging));
            _offscreenColor = factory.CreateTexture(TextureDescription.Texture2D((uint)Width, (uint)Height, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget));
            _offscreenFB = factory.CreateFramebuffer(new FramebufferDescription(null, _offscreenColor));
            #endregion

            // create GraphicsPipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.Disabled;
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,//draw outline or fill
                frontFace: FrontFace.CounterClockwise,//order of drawing point, see Indices array.
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;//basis graphics is point,line,or triangle
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: _shaders);
            pipelineDescription.Outputs = _offscreenFB.OutputDescription;

            _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
            // create CommandList
            _commandList = factory.CreateCommandList();
        }

        public byte[] Draw()
        {

            // Begin() must be called before commands can be issued.
            _commandList.Begin();

            _commandList.SetFramebuffer(_offscreenFB);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _commandList.SetPipeline(_pipeline);
            _commandList.DrawIndexed(
                indexCount: (uint)triangleIndices.Length,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);

            #region Add for Headless
            //transfer GPU drawing to CPU readable one
            _commandList.CopyTexture(_offscreenColor, _offscreenReadOut);
            #endregion

            // End() must be called before commands can be submitted for execution.
            _commandList.End();
            GraphicsDevice?.SubmitCommands(_commandList);
            // Once commands have been submitted, the rendered image can be presented to the application window.
            //GraphicsDevice?.SwapBuffers(MainSwapchain);
            GraphicsDevice?.WaitForIdle();
            
            #region Add for Headless
            MappedResourceView<byte> view = GraphicsDevice.Map<byte>(_offscreenReadOut, MapMode.Read);
            byte[] tmp = new byte[view.SizeInBytes];
            Marshal.Copy(view.MappedResource.Data, tmp, 0, (int)view.SizeInBytes);
            GraphicsDevice.Unmap(_offscreenReadOut);
            #endregion

            return tmp;
        }

        #region Add for Headless
        public SKBitmap SaveRgba32ToSKBitmap(byte[] bytes)
        {
            var flipVertical = GraphicsDevice.BackendType == GraphicsBackend.Vulkan;
            using SKImage img = SKImage.FromPixelCopy(new SKImageInfo(bytes.Length / 16 / Height, Height, SKColorType.RgbaF32), bytes);
            SKBitmap bmp = new SKBitmap(img.Width, img.Height);
            using SKCanvas surface = new SKCanvas(bmp);
            surface.Scale(1, flipVertical ? -1 : 1, 0, flipVertical ? Height / 2f : 0);
            surface.DrawImage(img, 0, 0);
            return bmp;
        }
        #endregion

        public void Dispose()
        {
            _indexBuffer?.Dispose();
            _vertexBuffer?.Dispose();
            _pipeline?.Dispose();
            _commandList?.Dispose();
            foreach (var shader in _shaders)
                shader?.Dispose();
        }

        /// <summary>
        /// convert glsl to spirv use <see href="https://shader-playground.timjones.io/#"/>
        /// </summary>
        static byte[] spirvVertexCode =
{
    0x03, 0x02, 0x23, 0x07, 0x00, 0x06, 0x01, 0x00, 0x0a, 0x00, 0x08, 0x00, 0x20, 0x00, 0x00, 0x00,  // ..#......... ...
    0x00, 0x00, 0x00, 0x00, 0x11, 0x00, 0x02, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x06, 0x00,  // ................
    0x01, 0x00, 0x00, 0x00, 0x47, 0x4c, 0x53, 0x4c, 0x2e, 0x73, 0x74, 0x64, 0x2e, 0x34, 0x35, 0x30,  // ....GLSL.std.450
    0x00, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,  // ................
    0x0f, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x6d, 0x61, 0x69, 0x6e,  // ............main
    0x00, 0x00, 0x00, 0x00, 0x0d, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00, 0x03, 0x00, 0x03, 0x00,  // ................
    0x02, 0x00, 0x00, 0x00, 0xcc, 0x01, 0x00, 0x00, 0x05, 0x00, 0x04, 0x00, 0x04, 0x00, 0x00, 0x00,  // ................
    0x6d, 0x61, 0x69, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x05, 0x00, 0x06, 0x00, 0x0b, 0x00, 0x00, 0x00,  // main............
    0x67, 0x6c, 0x5f, 0x50, 0x65, 0x72, 0x56, 0x65, 0x72, 0x74, 0x65, 0x78, 0x00, 0x00, 0x00, 0x00,  // gl_PerVertex....
    0x06, 0x00, 0x06, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x67, 0x6c, 0x5f, 0x50,  // ............gl_P
    0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x00, 0x06, 0x00, 0x07, 0x00, 0x0b, 0x00, 0x00, 0x00,  // osition.........
    0x01, 0x00, 0x00, 0x00, 0x67, 0x6c, 0x5f, 0x50, 0x6f, 0x69, 0x6e, 0x74, 0x53, 0x69, 0x7a, 0x65,  // ....gl_PointSize
    0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x07, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,  // ................
    0x67, 0x6c, 0x5f, 0x43, 0x6c, 0x69, 0x70, 0x44, 0x69, 0x73, 0x74, 0x61, 0x6e, 0x63, 0x65, 0x00,  // gl_ClipDistance.
    0x06, 0x00, 0x07, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x67, 0x6c, 0x5f, 0x43,  // ............gl_C
    0x75, 0x6c, 0x6c, 0x44, 0x69, 0x73, 0x74, 0x61, 0x6e, 0x63, 0x65, 0x00, 0x05, 0x00, 0x03, 0x00,  // ullDistance.....
    0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x00, 0x04, 0x00, 0x12, 0x00, 0x00, 0x00,  // ................
    0x61, 0x50, 0x6f, 0x73, 0x00, 0x00, 0x00, 0x00, 0x48, 0x00, 0x05, 0x00, 0x0b, 0x00, 0x00, 0x00,  // aPos....H.......
    0x00, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x00, 0x05, 0x00,  // ............H...
    0x0b, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,  // ................
    0x48, 0x00, 0x05, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00,  // H...............
    0x03, 0x00, 0x00, 0x00, 0x48, 0x00, 0x05, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,  // ....H...........
    0x0b, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x47, 0x00, 0x03, 0x00, 0x0b, 0x00, 0x00, 0x00,  // ........G.......
    0x02, 0x00, 0x00, 0x00, 0x47, 0x00, 0x04, 0x00, 0x12, 0x00, 0x00, 0x00, 0x1e, 0x00, 0x00, 0x00,  // ....G...........
    0x00, 0x00, 0x00, 0x00, 0x13, 0x00, 0x02, 0x00, 0x02, 0x00, 0x00, 0x00, 0x21, 0x00, 0x03, 0x00,  // ............!...
    0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x16, 0x00, 0x03, 0x00, 0x06, 0x00, 0x00, 0x00,  // ................
    0x20, 0x00, 0x00, 0x00, 0x17, 0x00, 0x04, 0x00, 0x07, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00,  //  ...............
    0x04, 0x00, 0x00, 0x00, 0x15, 0x00, 0x04, 0x00, 0x08, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,  // ............ ...
    0x00, 0x00, 0x00, 0x00, 0x2b, 0x00, 0x04, 0x00, 0x08, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00,  // ....+...........
    0x01, 0x00, 0x00, 0x00, 0x1c, 0x00, 0x04, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00,  // ................
    0x09, 0x00, 0x00, 0x00, 0x1e, 0x00, 0x06, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00,  // ................
    0x06, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x20, 0x00, 0x04, 0x00,  // ............ ...
    0x0c, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x3b, 0x00, 0x04, 0x00,  // ............;...
    0x0c, 0x00, 0x00, 0x00, 0x0d, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x15, 0x00, 0x04, 0x00,  // ................
    0x0e, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x2b, 0x00, 0x04, 0x00,  // .... .......+...
    0x0e, 0x00, 0x00, 0x00, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x17, 0x00, 0x04, 0x00,  // ................
    0x10, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x20, 0x00, 0x04, 0x00,  // ............ ...
    0x11, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x3b, 0x00, 0x04, 0x00,  // ............;...
    0x11, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x2b, 0x00, 0x04, 0x00,  // ............+...
    0x08, 0x00, 0x00, 0x00, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x04, 0x00,  // ............ ...
    0x14, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x2b, 0x00, 0x04, 0x00,  // ............+...
    0x08, 0x00, 0x00, 0x00, 0x19, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x2b, 0x00, 0x04, 0x00,  // ............+...
    0x06, 0x00, 0x00, 0x00, 0x1c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f, 0x20, 0x00, 0x04, 0x00,  // ...........? ...
    0x1e, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x36, 0x00, 0x05, 0x00,  // ............6...
    0x02, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,  // ................
    0xf8, 0x00, 0x02, 0x00, 0x05, 0x00, 0x00, 0x00, 0x41, 0x00, 0x05, 0x00, 0x14, 0x00, 0x00, 0x00,  // ........A.......
    0x15, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00, 0x13, 0x00, 0x00, 0x00, 0x3d, 0x00, 0x04, 0x00,  // ............=...
    0x06, 0x00, 0x00, 0x00, 0x16, 0x00, 0x00, 0x00, 0x15, 0x00, 0x00, 0x00, 0x41, 0x00, 0x05, 0x00,  // ............A...
    0x14, 0x00, 0x00, 0x00, 0x17, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00,  // ................
    0x3d, 0x00, 0x04, 0x00, 0x06, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x17, 0x00, 0x00, 0x00,  // =...............
    0x41, 0x00, 0x05, 0x00, 0x14, 0x00, 0x00, 0x00, 0x1a, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00,  // A...............
    0x19, 0x00, 0x00, 0x00, 0x3d, 0x00, 0x04, 0x00, 0x06, 0x00, 0x00, 0x00, 0x1b, 0x00, 0x00, 0x00,  // ....=...........
    0x1a, 0x00, 0x00, 0x00, 0x50, 0x00, 0x07, 0x00, 0x07, 0x00, 0x00, 0x00, 0x1d, 0x00, 0x00, 0x00,  // ....P...........
    0x16, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x1b, 0x00, 0x00, 0x00, 0x1c, 0x00, 0x00, 0x00,  // ................
    0x41, 0x00, 0x05, 0x00, 0x1e, 0x00, 0x00, 0x00, 0x1f, 0x00, 0x00, 0x00, 0x0d, 0x00, 0x00, 0x00,  // A...............
    0x0f, 0x00, 0x00, 0x00, 0x3e, 0x00, 0x03, 0x00, 0x1f, 0x00, 0x00, 0x00, 0x1d, 0x00, 0x00, 0x00,  // ....>...........
    0xfd, 0x00, 0x01, 0x00, 0x38, 0x00, 0x01, 0x00 // ....8...
};
        static byte[] spirvFragmentCode =
{
    0x03, 0x02, 0x23, 0x07, 0x00, 0x06, 0x01, 0x00, 0x0a, 0x00, 0x08, 0x00, 0x0e, 0x00, 0x00, 0x00,  // ..#.............
    0x00, 0x00, 0x00, 0x00, 0x11, 0x00, 0x02, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x06, 0x00,  // ................
    0x01, 0x00, 0x00, 0x00, 0x47, 0x4c, 0x53, 0x4c, 0x2e, 0x73, 0x74, 0x64, 0x2e, 0x34, 0x35, 0x30,  // ....GLSL.std.450
    0x00, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,  // ................
    0x0f, 0x00, 0x06, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x6d, 0x61, 0x69, 0x6e,  // ............main
    0x00, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x10, 0x00, 0x03, 0x00, 0x04, 0x00, 0x00, 0x00,  // ................
    0x07, 0x00, 0x00, 0x00, 0x03, 0x00, 0x03, 0x00, 0x02, 0x00, 0x00, 0x00, 0xcc, 0x01, 0x00, 0x00,  // ................
    0x05, 0x00, 0x04, 0x00, 0x04, 0x00, 0x00, 0x00, 0x6d, 0x61, 0x69, 0x6e, 0x00, 0x00, 0x00, 0x00,  // ........main....
    0x05, 0x00, 0x05, 0x00, 0x09, 0x00, 0x00, 0x00, 0x46, 0x72, 0x61, 0x67, 0x43, 0x6f, 0x6c, 0x6f,  // ........FragColo
    0x72, 0x00, 0x00, 0x00, 0x47, 0x00, 0x04, 0x00, 0x09, 0x00, 0x00, 0x00, 0x1e, 0x00, 0x00, 0x00,  // r...G...........
    0x00, 0x00, 0x00, 0x00, 0x13, 0x00, 0x02, 0x00, 0x02, 0x00, 0x00, 0x00, 0x21, 0x00, 0x03, 0x00,  // ............!...
    0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x16, 0x00, 0x03, 0x00, 0x06, 0x00, 0x00, 0x00,  // ................
    0x20, 0x00, 0x00, 0x00, 0x17, 0x00, 0x04, 0x00, 0x07, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00,  //  ...............
    0x04, 0x00, 0x00, 0x00, 0x20, 0x00, 0x04, 0x00, 0x08, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,  // .... ...........
    0x07, 0x00, 0x00, 0x00, 0x3b, 0x00, 0x04, 0x00, 0x08, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00,  // ....;...........
    0x03, 0x00, 0x00, 0x00, 0x2b, 0x00, 0x04, 0x00, 0x06, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00,  // ....+...........
    0x00, 0x00, 0x80, 0x3f, 0x2b, 0x00, 0x04, 0x00, 0x06, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00,  // ...?+...........
    0x00, 0x00, 0x00, 0x3f, 0x2b, 0x00, 0x04, 0x00, 0x06, 0x00, 0x00, 0x00, 0x0c, 0x00, 0x00, 0x00,  // ...?+...........
    0xcd, 0xcc, 0x4c, 0x3e, 0x2c, 0x00, 0x07, 0x00, 0x07, 0x00, 0x00, 0x00, 0x0d, 0x00, 0x00, 0x00,  // ..L>,...........
    0x0a, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x0c, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00,  // ................
    0x36, 0x00, 0x05, 0x00, 0x02, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  // 6...............
    0x03, 0x00, 0x00, 0x00, 0xf8, 0x00, 0x02, 0x00, 0x05, 0x00, 0x00, 0x00, 0x3e, 0x00, 0x03, 0x00,  // ............>...
    0x09, 0x00, 0x00, 0x00, 0x0d, 0x00, 0x00, 0x00, 0xfd, 0x00, 0x01, 0x00, 0x38, 0x00, 0x01, 0x00 // ............8...
};
    }
}
