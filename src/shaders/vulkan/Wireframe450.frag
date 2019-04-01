#version 450

layout (location = 0) in vec4 fsin_color;

layout (location = 0) out vec4 fsout_color;
layout (location = 1) out vec2 fsin_texcoords;

layout (set = 0, binding = 0) uniform ProjectionMatrix
{
	mat4 projection;
};

layout (set = 1, binding = 0) uniform ViewMatrix
{
	mat4 view;
};

layout (set = 2, binding = 0) uniform ModelMatrix
{
	mat4 model;
};

layout (constant_id = 0) const bool flip_vertical = false;

void main()
{
	fsout_color = fsin_color;

	if (fsout_color.a == 0.0f)
	{
		discard;
	}
}
