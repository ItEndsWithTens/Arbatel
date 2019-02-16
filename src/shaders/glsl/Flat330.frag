#version 330 core
in vec4 colorFromVert;

out vec4 color;

void main()
{
	color = colorFromVert;

	if (color.a == 0.0f)
	{
		discard;
	}
}
