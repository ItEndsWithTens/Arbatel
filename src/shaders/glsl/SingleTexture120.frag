#version 120
varying vec4 colorFromVert;
varying vec2 texCoordsFromVert;

uniform sampler2D tex;

void main()
{
	gl_FragColor = texture2D(tex, texCoordsFromVert) * colorFromVert;

	if (gl_FragColor.a == 0.0f)
	{
		discard;
	}
}
