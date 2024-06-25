#version 450

layout(set = 0, binding = 0) uniform OurColor
{
   vec4 Color;
};

layout(location = 0) out vec4 FragColor;

void main()
{
    FragColor = Color;
}