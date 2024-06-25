// See https://aka.ms/new-console-template for more information
using ShaderGeneratorExe;
using SharpDX.DXGI;
using System.Text;
using Veldrid;
internal class Program
{
    static void Main(string[] args)
    {
        string projectFolder = string.Empty;
        if (args.Length > 0)
         projectFolder = args[0];
        Console.WriteLine("Project Folder:" + projectFolder);
        //projectFolder = @"F:\PlayCode\Veldrid.Maui.Controls\Veldrid.Maui.Controls.Samples.Core";
        var shadersDirectory = Directory.CreateDirectory(Path.Combine(projectFolder, @"Shaders"));
        var outputDirectory = Directory.CreateDirectory(Path.Combine(projectFolder, @"bin\Generated"));
        var dictionary = new Dictionary<string, (string v, string f)>();
        var files = shadersDirectory.GetFiles();
        var verts = files.Where((f) => f.Extension == ".vert").ToList();
        var frags = files.Where((f) => f.Extension == ".frag").ToList();
        if (verts.Count() != frags.Count())
        {
            throw new Exception("vertex shader not equal to fragment shader.");
        }
        for (var i = 0; i < verts.Count(); i++)
        {
            var vertexFile = verts[i];
            var vertexFileName = vertexFile.Name.Split(".vert")[0];
            var vertexCode = File.ReadAllText(vertexFile.FullName);
            var fragmentFullPath = Path.Combine(shadersDirectory.FullName, vertexFileName + ".frag");
            var fragmentCode = File.ReadAllText(fragmentFullPath);
            dictionary.Add(vertexFileName, (vertexCode, fragmentCode));
        }
        var code = SourceFileFromShadersPath(dictionary);
        File.WriteAllText(Path.Combine(outputDirectory.FullName, "ShadersGenerator.g.cs"), code);
    }

    static string SourceFileFromShadersPath(Dictionary<string, (string v, string f)> shaderSources)
    {
        var spirv = new SpirvCrossHelper();
        StringBuilder targetFileCode = new StringBuilder();

        string GetCodeStringOfBytes(byte[] bytes)
        {
            StringBuilder vb = new StringBuilder();
            for (var index = 0; index < bytes.Length; index++)
            {
                var b = bytes[index];
                vb.Append(b);
                if (index != bytes.Length - 1)
                    vb.Append(',');
            }
            return vb.ToString();
        }

        targetFileCode.Append($@"
using Veldrid;
namespace ShadersGenerator {{
    public static partial class Constants {{
");
        targetFileCode.Append($@"public static (byte[] v, byte[] f) GetBytes(GraphicsBackend backend, string name)
        {{");
        foreach (var shader in shaderSources)
        {
            targetFileCode.Append($@"
        if(name == ""{shader.Key}""){{
            byte[] vertexBytes;
            byte[] fragmentBytes;
            if(backend == GraphicsBackend.Vulkan)
            {{    
                vertexBytes = {shader.Key}_Vertex_SPIRV;
                fragmentBytes = {shader.Key}_Fragment_SPIRV;
            }}else
            {{
                var vertexCode =
                backend == GraphicsBackend.Metal ? {shader.Key}_Vertex_MLSL : backend == GraphicsBackend.Direct3D11 ? {shader.Key}_Vertex_HLSL : {shader.Key}_Vertex_GLES;
                var fragmentCode =
                backend == GraphicsBackend.Metal ? {shader.Key}_Fragment_MLSL : backend == GraphicsBackend.Direct3D11 ? {shader.Key}_Fragment_HLSL : {shader.Key}_Fragment_GLES;
            
                vertexBytes = System.Text.Encoding.UTF8.GetBytes(vertexCode);
                fragmentBytes = System.Text.Encoding.UTF8.GetBytes(fragmentCode);
            }}
            return (vertexBytes, fragmentBytes);
        }}");
        }
        targetFileCode.Append($@"
        throw new NotImplementedException();
        }}");

        foreach (var shader in shaderSources)
        {
            (var vb_MLSL, var fb_MLSL) = spirv.GetShaderBytes(GraphicsBackend.Metal, shader.Value.v, shader.Value.f);
            (var vb_HLSL, var fb_HLSL) = spirv.GetShaderBytes(GraphicsBackend.Direct3D11, shader.Value.v, shader.Value.f);
            (var vb_GLES, var fb_GLES) = spirv.GetShaderBytes(GraphicsBackend.OpenGLES, shader.Value.v, shader.Value.f);
            (var vb_SPIRV, var fb_SPIRV) = spirv.GetShaderBytes(GraphicsBackend.Vulkan, shader.Value.v, shader.Value.f);

            targetFileCode.Append($@"
                public static string {shader.Key}_Vertex_MLSL = @"" 
                    {System.Text.Encoding.UTF8.GetString(vb_MLSL)} "";
                public static string {shader.Key}_Fragment_MLSL = @"" 
                    {System.Text.Encoding.UTF8.GetString(fb_MLSL)} "";
                public static string {shader.Key}_Vertex_HLSL = @"" 
                    {System.Text.Encoding.UTF8.GetString(vb_HLSL)} "";
                public static string {shader.Key}_Fragment_HLSL = @"" 
                    {System.Text.Encoding.UTF8.GetString(fb_HLSL)} "";
                public static string {shader.Key}_Vertex_GLES = @"" 
                    {System.Text.Encoding.UTF8.GetString(vb_GLES)} "";
                public static string {shader.Key}_Fragment_GLES = @"" 
                    {System.Text.Encoding.UTF8.GetString(fb_GLES)} "";
                public static byte[] {shader.Key}_Vertex_SPIRV = {{ {GetCodeStringOfBytes(vb_SPIRV)} }};
                public static byte[] {shader.Key}_Fragment_SPIRV = {{ {GetCodeStringOfBytes(fb_SPIRV)} }};
            ");
        }
        targetFileCode.Append($@"}}
}}
");
        return targetFileCode.ToString();
    }
}
