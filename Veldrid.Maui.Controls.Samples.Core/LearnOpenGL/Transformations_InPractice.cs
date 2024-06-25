﻿using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid.Maui.Controls.AssetPrimitives;
using Veldrid.Maui.Controls.Base;

namespace Veldrid.Maui.Controls.Samples.Core.LearnOpenGL
{
    public class Transformations_InPractice : BaseGpuDrawable
    {
        private DeviceBuffer _vertexBuffer;
        private Pipeline _pipeline;
        private CommandList _commandList;
        private ResourceSet _transSet;
        private ResourceSet _textureSet;
        private Shader[] _shaders;
        private DeviceBuffer _indexBuffer;
        ushort[] quadIndices;
        private ProcessedTexture texture1;
        private Texture _surfaceTexture1;
        private TextureView _surfaceTextureView1;
        private ProcessedTexture texture2;
        private Texture _surfaceTexture2;
        private TextureView _surfaceTextureView2;
        private DeviceBuffer _transBuffer;

        [StructLayout(LayoutKind.Sequential)]
        struct VerticeData
        {
            Vector3 Position;
            Vector3 Color;
            Vector2 TextureCoord;

            public VerticeData(float x, float y, float z, float r, float g, float b, float tx, float ty)
            {
                Position = new Vector3(x, y, z);
                Color = new Vector3(r, g, b);
                TextureCoord = new Vector2(tx, ty);
            }
        }

        protected unsafe override void CreateResources(ResourceFactory factory)
        {
            //vertices data of a quad
            VerticeData[] quadVertices = new VerticeData[]
            {  
                                // positions          // colors           // texture coords
                new VerticeData( 0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   1.0f, 1.0f),   // top right
                new VerticeData( 0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f),   // bottom right
                new VerticeData(-0.5f, -0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f, 0.0f),   // bottom left
                new VerticeData(-0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f)   // top left 
            };

            // create Buffer for vertices data
            BufferDescription vertexBufferDescription = new BufferDescription(
                (uint)(quadVertices.Length * sizeof(VerticeData)),
                BufferUsage.VertexBuffer);
            _vertexBuffer = factory.CreateBuffer(vertexBufferDescription);
            GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, quadVertices);// update data to Buffer

            // Index data (it specify how to use quadVertices)
            quadIndices = new ushort[] {
                0, 1, 2,// first triangle, order is Clockwise
                3, 0
            };
            // create IndexBuffer
            BufferDescription indexBufferDescription = new BufferDescription(
                (uint)(quadIndices.Length * sizeof(ushort)),
                BufferUsage.IndexBuffer);
            _indexBuffer = factory.CreateBuffer(indexBufferDescription);
            GraphicsDevice.UpdateBuffer(_indexBuffer, 0, quadIndices);// update data to Buffer

            string vertexCode = @"
#version 450

layout(set = 0, binding = 0) uniform  Trans
{
  mat4 Transform;
};

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;
layout (location = 2) in vec2 aTexCoord;

layout (location = 0) out vec3 ourColor;
layout (location = 1) out vec2 TexCoord;

void main()
{
    gl_Position = Transform * vec4(aPos, 1.0);
    ourColor = aColor;
    TexCoord = vec2(aTexCoord.x, 1 - aTexCoord.y);
}";

            string fragmentCode = @"
#version 450

layout (location = 0) out vec4 FragColor;
layout (location = 0) in vec3 ourColor;
layout (location = 1) in vec2 TexCoord;

layout(set = 1, binding = 0) uniform texture2D Texture1;//In veldrid, use 'uniform' to input something need 'set' descriptor 
layout(set = 1, binding = 1) uniform texture2D Texture2;
layout(set = 1, binding = 2) uniform sampler Sampler;

void main()
{
    FragColor = mix(texture(sampler2D(Texture1, Sampler), TexCoord),texture(sampler2D(Texture2, Sampler),TexCoord) ,0.2) * vec4(1,1,1, ourColor.x);
}";

            // OpenGL no layout(set), so we need use Spirv to convert

            (byte[] vertexBytes, byte[] fragmentBytes) = ShadersGenerator.Constants.GetBytes(factory.BackendType, this.GetType().Name);
            string entryPoint = factory.BackendType == GraphicsBackend.Metal ? "main0" : "main";
            var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertexBytes, entryPoint);
            var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragmentBytes, entryPoint);

            var vertexShader = factory.CreateShader(vertexShaderDesc);
            var fragmentShader = factory.CreateShader(fragmentShaderDesc);
            _shaders = new Shader[] { vertexShader, fragmentShader };

            // VertexLayout tell Veldrid we store wnat in Vertex Buffer, it need match with vertex.glsl
            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
               new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
               new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
               new VertexElementDescription("TextureCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

            // from file load image as texture
            //texture1 = new ImageProcessor().ProcessT(this.ReadEmbedAssetStream("LearnOpenGL.Assets.Images.container.jpg"), ".jpg");
            texture1 = LoadEmbeddedAsset<ProcessedTexture>(this.ReadEmbedAssetPath("ProcessedImages.container.binary"));
            _surfaceTexture1 = texture1.CreateDeviceTexture(GraphicsDevice, ResourceFactory, TextureUsage.Sampled);
            _surfaceTextureView1 = factory.CreateTextureView(_surfaceTexture1);
            //texture2 = new ImageProcessor().ProcessT(this.ReadEmbedAssetStream("LearnOpenGL.Assets.Images.awesomeface.png"), ".png");
            texture2 = LoadEmbeddedAsset<ProcessedTexture>(this.ReadEmbedAssetPath("ProcessedImages.awesomeface.binary"));
            _surfaceTexture2 = texture2.CreateDeviceTexture(GraphicsDevice, ResourceFactory, TextureUsage.Sampled);
            _surfaceTextureView2 = factory.CreateTextureView(_surfaceTexture2);

            _transBuffer = factory.CreateBuffer(new BufferDescription((uint)sizeof(Matrix4x4), BufferUsage.UniformBuffer));
            ResourceLayout transLayout = factory.CreateResourceLayout(
               new ResourceLayoutDescription(
                   new ResourceLayoutElementDescription("Trans", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                   ));

            ResourceLayout textureLayout = factory.CreateResourceLayout(
                 new ResourceLayoutDescription(
                     new ResourceLayoutElementDescription("Texture1", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                     new ResourceLayoutElementDescription("Texture2", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                     new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
                    ));

            // create GraphicsPipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.Disabled;
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,//draw outline or fill
                frontFace: FrontFace.Clockwise,//order of drawing point, see Indices array.
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;//basis graphics is point,line,or triangle
            pipelineDescription.ResourceLayouts = new[] { transLayout, textureLayout };
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: _shaders);
            pipelineDescription.Outputs = MainSwapchain.Framebuffer.OutputDescription;

            _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
            // create CommandList
            _commandList = factory.CreateCommandList();
            // create ResourceSet for transform
            _transSet = factory.CreateResourceSet(new ResourceSetDescription(
              transLayout,
              _transBuffer
              ));
            // create ResourceSet for texture
            _textureSet = factory.CreateResourceSet(new ResourceSetDescription(
               textureLayout,
               _surfaceTextureView1,
               _surfaceTextureView2,
               GraphicsDevice.LinearSampler
               ));
        }

        protected override void Draw(float deltaSeconds)
        {
            // Begin() must be called before commands can be issued.
            _commandList.Begin();

            _commandList.SetFramebuffer(MainSwapchain.Framebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            var trans = Matrix4x4.CreateTranslation(1, 1, 0) * Matrix4x4.CreateScale(0.5f);
            _commandList.UpdateBuffer(_transBuffer, 0, trans);

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _commandList.SetPipeline(_pipeline);
            _commandList.SetGraphicsResourceSet(0, _transSet);
            _commandList.SetGraphicsResourceSet(1, _textureSet);
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
        }
    }
}
