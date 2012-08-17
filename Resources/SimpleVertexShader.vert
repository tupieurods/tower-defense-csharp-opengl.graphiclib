#version 330 core

uniform mat4 projectionMatrix;
in vec2 position;
void main(void)
{
  gl_Position = projectionMatrix * vec4(position, 0.0, 1.0);
}
