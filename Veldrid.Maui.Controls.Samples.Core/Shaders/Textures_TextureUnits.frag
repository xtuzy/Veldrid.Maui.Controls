#version 450

layout (location = 0) out vec4 FragColor;
layout (location = 0) in vec3 ourColor;
layout (location = 1) in vec2 TexCoord;

layout(set = 0, binding = 0) uniform texture2D Texture1;//In veldrid, use 'uniform' to input something need 'set' descriptor 
layout(set = 0, binding = 1) uniform texture2D Texture2;
layout(set = 0, binding = 2) uniform sampler Sampler;

void main()
{
    FragColor = mix(texture(sampler2D(Texture1, Sampler), TexCoord),texture(sampler2D(Texture2, Sampler),TexCoord) ,0.2) * vec4(1,1,1, ourColor.x);//SPIRV will optimize and ignore unused. in Vertices data,if i don't use ourColor data, TexCoord data after Color data, so it use Color data as TextCoord  data
}