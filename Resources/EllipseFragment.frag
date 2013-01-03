#version 330 core

/*
//Color:
RGB
//Center
XY0
//xRadius, yRadius and border
ABborder
*/
uniform vec3 conf[3];
const float support = 1.0;

//Get alpha function
float getAlpha(float a, float b, float x, float y, bool flag)
{
  float d = sqrt(x * x / (a * a) + y * y / (b * b));
  float width = fwidth(d) * support / 1.25;
  if(flag)
    return smoothstep(1.0 - width, 1.0 + width, d);
  else
    return smoothstep(1.0 + width, 1.0 - width, d);
}

void main(void)
{
  //Контур эллипса
  vec2 pos = (gl_FragCoord.xy * gl_FragCoord.w - conf[1].xy);
  if((conf[2].z != 0) && (distance(pos, vec2(0,0))< 1))
    discard;
  float alpha1 = getAlpha(conf[2].x + conf[2].z / 2.0, conf[2].y + conf[2].z / 2.0, pos.x, pos.y, true);
  float alpha2 = (conf[2].z == 0) ? alpha1 : getAlpha(conf[2].x - conf[2].z / 2.0, conf[2].y - conf[2].z / 2.0, pos.x, pos.y, false);
  float alpha = (1.0 - max(alpha1, alpha2));
  gl_FragColor = vec4(conf[0].rgb, alpha * 1.0);
}