#version 120
varying vec4 vertexColor;

void main()
{
	gl_FragColor = vertexColor;

	if (gl_FragColor.a == 0.0f)
	{
		discard;
	}
}
