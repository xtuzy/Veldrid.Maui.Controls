#version 450

layout(location = 0) out vec4 FragColor;
layout(location = 0) in vec3 ourColor; // the input variable from the vertex shader (same name and same type)  

void main()
{
    FragColor = vec4(ourColor, 1.0);
}