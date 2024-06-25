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

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 InstancePosition;

layout (location = 0) out vec2 TexCoord;

void main()
{
    mat4 delta = mat4(vec4(1.0, 0.0, 0.0, 0.0),vec4(0.0, 1.0, 0.0, 0.0),vec4(0.0, 0.0, 1.0, 0.0),vec4(InstancePosition, 1.0));
    // note that we read the multiplication from right to left
    gl_Position = projection * view *  delta * model * vec4(aPos, 1.0);
    TexCoord = vec2(aTexCoord.x, 1 - aTexCoord.y);
}