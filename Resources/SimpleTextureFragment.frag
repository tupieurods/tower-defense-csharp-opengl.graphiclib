#version 330 core

uniform sampler2D Texture;

in vec2 fragTexcoord;

out vec4 color;

void main(void)
{
  color = texture2D(Texture, fragTexcoord);
}