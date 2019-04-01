#version 450

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec4 color;
layout (location = 3) in vec2 texCoords;

layout (location = 0) out vec4 fsin_color;

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
	vec3 yUpRightHand = vec3(position.x, position.z, -position.y);
	gl_Position = projection * view * model * vec4(yUpRightHand, 1);

	if (flip_vertical)
	{
		gl_Position.y = -gl_Position.y;
	}

	fsin_color = color;
}
