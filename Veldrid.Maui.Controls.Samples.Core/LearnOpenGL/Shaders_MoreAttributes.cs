﻿using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.Maui.Controls.Base;

namespace Veldrid.Maui.Controls.Samples.Core.LearnOpenGL
{
    public class Shaders_MoreAttributes : BaseGpuDrawable
    {
        private DeviceBuffer _vertexBuffer;
        private Pipeline _pipeline;
        private CommandList _commandList;
        private Shader[] _shaders;
        private DeviceBuffer _indexBuffer;
        ushort[] quadIndices;

        protected unsafe override void CreateResources(ResourceFactory factory)
        {
            //vertices data of a triangle
            Vector3[] vertices = new Vector3[]
            {
                // positions                      // colors
                new Vector3( -0.5f, -0.5f, 0.0f), new Vector3( 1.0f, 0.0f, 0.0f ),
                new Vector3( 0.5f, -0.5f, 0.0f), new Vector3( 0.0f, 1.0f, 0.0f ),
                new Vector3( 0.0f, 0.5f, 0.0f), new Vector3( 0.0f, 0.0f, 1.0f )
            };

            // create Buffer for vertices data
            BufferDescription vbDescription = new BufferDescription(
                (uint)(vertices.Length * sizeof(Vector3)),
                BufferUsage.VertexBuffer);
            _vertexBuffer = factory.CreateBuffer(vbDescription);
            GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertices);

            // Index data
            quadIndices = new ushort[] { 0, 1, 2 };
            // create IndexBuffer
            BufferDescription ibDescription = new BufferDescription(
                (uint)(quadIndices.Length * sizeof(ushort)),
                BufferUsage.IndexBuffer);
            _indexBuffer = factory.CreateBuffer(ibDescription);
            GraphicsDevice.UpdateBuffer(_indexBuffer, 0, quadIndices);

            string vertexCode = @"
#version 450

layout (location = 0) in vec3 aPos;   // the position variable has attribute position 0
layout (location = 1) in vec3 aColor; // the color variable has attribute position 1

layout (location = 0) out vec3 ourColor; // output a color to the fragment shader

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
    ourColor = aColor; // set ourColor to the input color we got from the vertex data
}";

            string fragmentCode = @"
#version 450

layout(location = 0) out vec4 FragColor;
layout(location = 0) in vec3 ourColor; // the input variable from the vertex shader (same name and same type)  

void main()
{
    FragColor = vec4(ourColor, 1.0);
}";

            (byte[] vertexBytes, byte[] fragmentBytes) = ShadersGenerator.Constants.GetBytes(factory.BackendType, this.GetType().Name);
            string entryPoint = factory.BackendType == GraphicsBackend.Metal ? "main0" : "main";
            var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertexBytes, entryPoint);
            var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragmentBytes, entryPoint);

            var vertexShader = factory.CreateShader(vertexShaderDesc);
            var fragmentShader = factory.CreateShader(fragmentShaderDesc);
            _shaders = new Shader[] { vertexShader, fragmentShader };

            // VertexLayout tell Veldrid we store what in Vertex Buffer, it need match with vertex.glsl
            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
               new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
               new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));//Why use VertexElementSemantic.TextureCoordinate:https://github.com/mellinoe/veldrid/issues/121

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
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: _shaders);
            pipelineDescription.Outputs = MainSwapchain.Framebuffer.OutputDescription;

            _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
            // create CommandList
            _commandList = factory.CreateCommandList();
        }

        protected override void Draw(float deltaSeconds)
        {
            // Begin() must be called before commands can be issued.
            _commandList.Begin();

            _commandList.SetFramebuffer(MainSwapchain.Framebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _commandList.SetPipeline(_pipeline);
            _commandList.DrawIndexed(
                indexCount: (uint)quadIndices.Length,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);

            // End() must be called before commands can be submitted for execution.
            _commandList.End();
            GraphicsDevice?.SubmitCommands(_commandList);
            // Once commands have been submitted, the rendered image can be presented to the application window.
            GraphicsDevice?.SwapBuffers(MainSwapchain);
            GraphicsDevice.WaitForIdle();
        }

        public override void ReleaseResources()
        {
            _indexBuffer?.Dispose();
            _vertexBuffer?.Dispose();
            _pipeline?.Dispose();
            _commandList?.Dispose();
            foreach (var shader in _shaders)
                shader?.Dispose();
        }
    }
}
