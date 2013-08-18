#version 330 core

uniform sampler2D Texture;
uniform vec4 inColor;

in vec2 fragTexcoord;
out vec4 color;

const float onePixelDistance = 0.03;
const float SOFT_EDGE_MIN = 0.5 - onePixelDistance * 3.0;
const float SOFT_EDGE_MAX = 0.5 + onePixelDistance * 3.0;

void main(void)
{
  float dist = texture2D(Texture, fragTexcoord).a;
  float alpha = inColor.a * smoothstep(SOFT_EDGE_MIN, SOFT_EDGE_MAX, dist);
  color = vec4(inColor.rgb, alpha);
}