#version 150 core
 
// incoming vertex information
in vec4 inPosition;
in vec4 inColor;

flat out vec4 colorPass;

void main(void)
{
    colorPass = inColor;
    gl_Position = inPosition;
}
