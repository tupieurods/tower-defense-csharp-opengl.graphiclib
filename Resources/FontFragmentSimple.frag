#version 330 core

uniform sampler2D Texture;
uniform vec4 inColor;

in vec2 fragTexcoord;
out vec4 color;

void main(void)
{
  color = vec4(1.0, 1.0, 1.0, texture2D(Texture, fragTexcoord).a) * inColor;
}