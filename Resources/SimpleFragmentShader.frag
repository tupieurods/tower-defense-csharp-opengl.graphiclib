#version 330 core

uniform vec4 fragmentColor;
out vec4 color;
void main(void)
{
  color = fragmentColor;
}


