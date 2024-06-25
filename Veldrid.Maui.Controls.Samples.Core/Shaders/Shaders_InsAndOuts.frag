#version 450

layout(location = 0) in vec4 vertexColor; // the input variable from the vertex shader (same name and same type)  

layout(location = 0) out vec4 FragColor;

void main()
{
    FragColor = vertexColor;
}