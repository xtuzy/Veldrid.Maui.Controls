#version 450

layout(location = 0) in vec3 aPos;
layout(location = 0) out vec2 Position;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
    Position = vec2(aPos.x, aPos.y);
}