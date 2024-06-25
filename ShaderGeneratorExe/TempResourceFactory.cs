using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;

namespace ShaderGeneratorExe
{
    public class TempResourceFactory : ResourceFactory
    {
        public TempResourceFactory(GraphicsBackend backendType, GraphicsDeviceFeatures features = null) : base(features)
        {
            this.backendType = backendType;
        }

        GraphicsBackend backendType;
        public override GraphicsBackend BackendType { get => backendType; }

        public override CommandList CreateCommandList(ref CommandListDescription description)
        {
            throw new NotImplementedException();
        }

        public override Pipeline CreateComputePipeline(ref ComputePipelineDescription description)
        {
            throw new NotImplementedException();
        }

        public override Fence CreateFence(bool signaled)
        {
            throw new NotImplementedException();
        }

        public override Framebuffer CreateFramebuffer(ref FramebufferDescription description)
        {
            throw new NotImplementedException();
        }

        public override ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description)
        {
            throw new NotImplementedException();
        }

        public override ResourceSet CreateResourceSet(ref ResourceSetDescription description)
        {
            throw new NotImplementedException();
        }

        public override Swapchain CreateSwapchain(ref SwapchainDescription description)
        {
            throw new NotImplementedException();
        }

        protected override DeviceBuffer CreateBufferCore(ref BufferDescription description)
        {
            throw new NotImplementedException();
        }

        protected override Pipeline CreateGraphicsPipelineCore(ref GraphicsPipelineDescription description)
        {
            throw new NotImplementedException();
        }

        protected override Sampler CreateSamplerCore(ref SamplerDescription description)
        {
            throw new NotImplementedException();
        }

        protected override Shader CreateShaderCore(ref ShaderDescription description)
        {
            throw new NotImplementedException();
        }

        protected override Texture CreateTextureCore(ulong nativeTexture, ref TextureDescription description)
        {
            throw new NotImplementedException();
        }

        protected override Texture CreateTextureCore(ref TextureDescription description)
        {
            throw new NotImplementedException();
        }

        protected override TextureView CreateTextureViewCore(ref TextureViewDescription description)
        {
            throw new NotImplementedException();
        }
    }
}
