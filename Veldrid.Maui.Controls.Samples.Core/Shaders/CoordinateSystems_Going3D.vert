#version 450

layout(set = 0, binding = 0) uniform ModelTrans
{
  mat4 model;
};
layout(set = 0, binding = 1) uniform ViewTrans
{
  mat4 view;
};
layout(set = 0, binding = 2) uniform ProjectionTrans
{
  mat4 projection;
};

layout (location = 0) in vec3 Position;
layout (location = 1) in vec2 TextureCoord;

layout (location = 0) out vec2 TexCoord;

void main()
{
    // note that we read the multiplication from right to left
    vec4 worldPosition = model * vec4(Position, 1);
    vec4 viewPosition = view * worldPosition;
    vec4 clipPosition = projection * viewPosition;
    gl_Position = clipPosition;
    //gl_Position = projection * view * model * vec4(Position, 1.0);
    TexCoord = vec2(TextureCoord.x, 1 - TextureCoord.y);
}