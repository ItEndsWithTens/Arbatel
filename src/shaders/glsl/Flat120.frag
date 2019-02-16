#version 120
varying vec4 colorFromVert;

void main()
{
	gl_FragColor = colorFromVert;

	if (gl_FragColor.a == 0.0f)
	{
		discard;
	}
}
