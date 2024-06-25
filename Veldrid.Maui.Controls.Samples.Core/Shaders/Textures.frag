#version 450

layout(location = 0) in vec2 Position;
layout(location = 0) out vec4 FragColor;

layout(set = 0, binding = 0) uniform texture2D SurfaceTexture;//In veldrid, use 'uniform' to input something need 'set' descriptor 
layout(set = 0, binding = 1) uniform sampler SurfaceSampler;

void main()
{
    FragColor = texture(sampler2D(SurfaceTexture, SurfaceSampler), Position);
}