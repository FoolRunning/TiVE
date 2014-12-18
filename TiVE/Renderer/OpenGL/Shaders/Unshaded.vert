#version 150 core 
 
// premultiplied model to projection transformation
uniform mat4 matrix_ModelViewProjection;
 
// incoming vertex information
in vec3 in_Position;
in vec4 in_Color;

flat out vec4 fragment_color;
 
void main(void)
{
    fragment_color = in_Color;

    // transforming the incoming vertex position
    gl_Position = matrix_ModelViewProjection * vec4(in_Position, 1.0);
}
