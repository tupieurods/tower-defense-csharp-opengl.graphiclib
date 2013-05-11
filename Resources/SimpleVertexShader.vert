#version 330 core

uniform mat4 projectionMatrix;
in vec2 position;
in vec4 vertexColor;
out vec4 fragmentColor;
void main(void)
{
  gl_Position = projectionMatrix * vec4(position, 0.0, 1.0);
  fragmentColor = vertexColor;
}
