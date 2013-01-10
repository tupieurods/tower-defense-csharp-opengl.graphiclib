#version 330 core

uniform vec3 color;
uniform vec2 center;
uniform float a;
uniform float b;
void main(void)
{
  vec2 pos = (gl_FragCoord.xy * gl_FragCoord.w) - center;
  //Уравнение эллипса я выбираю тебя
  float c = (pos.x * pos.x) / (a * a) + (pos.y * pos.y) / (b * b);
  if(c < 1)
    gl_FragColor = vec4(color, smoothstep(0, 1, 1 -c));
  else
    discard;
  //Вытянуть контраст
  gl_FragColor = (min(a, b) / 4) * (gl_FragColor - 0.5) + 0.5;
}
