#version 330 core

uniform vec4 fragmentColor;
out vec4 color;
void main(void)
{
  if(fragmentColor.a==0.0)
    discard;
  color = fragmentColor;
}


