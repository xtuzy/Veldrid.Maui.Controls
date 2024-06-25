// See https://aka.ms/new-console-template for more information
using Veldrid.SPIRV;
using Veldrid;
using System.Text;
namespace ShaderGeneratorExe;
internal class SpirvCrossHelper
{
    public (byte[] vb, byte[] fb) GetShaderBytes(GraphicsBackend target, string v, string f)
    {
        var vertexCode = v;
        var fragmentCode = f;
        var factory = new TempResourceFactory(target);
        var _shaders = CreateFromSpirv(factory, v, f);
        return (_shaders.vb, _shaders.fb);
    }

    (byte[] vb, byte[] fb) CreateFromSpirv(ResourceFactory factory, string vertexCode, string fragmentCode)
    {
        CrossCompileOptions options = new CrossCompileOptions();
        var vertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertexCode), "main");
        var fragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragmentCode), "main");

        GraphicsBackend backendType = factory.BackendType;
        if (backendType == GraphicsBackend.Vulkan)
        {
            vertexShaderDescription.ShaderBytes = EnsureSpirv(vertexCode, vertexShaderDescription);
            fragmentShaderDescription.ShaderBytes = EnsureSpirv(fragmentCode, fragmentShaderDescription);
            return (vertexShaderDescription.ShaderBytes, fragmentShaderDescription.ShaderBytes);
        }

        CrossCompileTarget compilationTarget = GetCompilationTarget(factory.BackendType);
        VertexFragmentCompilationResult vertexFragmentCompilationResult = SpirvCompilation.CompileVertexFragment(vertexShaderDescription.ShaderBytes, fragmentShaderDescription.ShaderBytes, compilationTarget, options);
        string entryPoint = ((backendType == GraphicsBackend.Metal && vertexShaderDescription.EntryPoint == "main") ? "main0" : vertexShaderDescription.EntryPoint);
        byte[] bytes = GetBytes(backendType, vertexFragmentCompilationResult.VertexShader);
        //Shader shader = factory.CreateShader(new ShaderDescription(vertexShaderDescription.Stage, bytes, entryPoint));
        string entryPoint2 = ((backendType == GraphicsBackend.Metal && fragmentShaderDescription.EntryPoint == "main") ? "main0" : fragmentShaderDescription.EntryPoint);
        byte[] bytes2 = GetBytes(backendType, vertexFragmentCompilationResult.FragmentShader);
        //Shader shader2 = factory.CreateShader(new ShaderDescription(fragmentShaderDescription.Stage, bytes2, entryPoint2));
        return (bytes, bytes2);
    }

    private byte[] EnsureSpirv(string shaderString, ShaderDescription description)
    {
        if (Util.HasSpirvHeader(description.ShaderBytes))
        {
            return description.ShaderBytes;
        }

        /*fixed (byte* sourceTextPtr = description.ShaderBytes)
        {
            return SpirvCompilation.CompileGlslToSpirv((uint)description.ShaderBytes.Length, sourceTextPtr, null, description.Stage, description.Debug, 0u, null).SpirvBytes;
        }*/
        return SpirvCompilation.CompileGlslToSpirv(shaderString, null, description.Stage, new GlslCompileOptions() { Debug = description.Debug }).SpirvBytes;
    }

    private byte[] GetBytes(GraphicsBackend backend, string code)
    {
        switch (backend)
        {
            case GraphicsBackend.Direct3D11:
            case GraphicsBackend.OpenGL:
            case GraphicsBackend.OpenGLES:
                return Encoding.ASCII.GetBytes(code);
            case GraphicsBackend.Metal:
                return Encoding.UTF8.GetBytes(code);
            default:
                throw new SpirvCompilationException($"Invalid GraphicsBackend: {backend}");
        }
    }

    private CrossCompileTarget GetCompilationTarget(GraphicsBackend backend)
    {
        return backend switch
        {
            GraphicsBackend.Direct3D11 => CrossCompileTarget.HLSL,
            GraphicsBackend.OpenGL => CrossCompileTarget.GLSL,
            GraphicsBackend.Metal => CrossCompileTarget.MSL,
            GraphicsBackend.OpenGLES => CrossCompileTarget.ESSL,
            _ => throw new SpirvCompilationException($"Invalid GraphicsBackend: {backend}"),
        };
    }
}
