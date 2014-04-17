#version 150 core

flat in vec4 fragment_color;

out vec4 color;

void main(void)
{
	color = fragment_color;
}
