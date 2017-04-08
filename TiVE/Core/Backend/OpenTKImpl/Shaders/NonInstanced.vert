#version 150 core
 
// incoming vertex information
in vec4 inPosition;
in vec4 inColor;
in vec3 inNormal;

flat out vec4 colorPass;
flat out vec3 normalPass;

void main(void)
{
    colorPass = inColor;
    normalPass = inNormal;
    gl_Position = inPosition;
}
