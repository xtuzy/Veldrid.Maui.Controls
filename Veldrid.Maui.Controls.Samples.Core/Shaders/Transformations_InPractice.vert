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
}