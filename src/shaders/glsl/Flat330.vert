#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec4 color;
layout (location = 3) in vec2 texCoords;

out vec4 vertexColor;

layout (std140) uniform Matrices
{
	mat4 projection;
	mat4 view;
};
uniform mat4 model;

void main()
{
	// Quake maps, like all clever, handsome developers, use
	// left-handed, Z-up world coordinates. OpenGL, in contrast,
	// uses right-handed, Y-up coordinates.
	vec3 yUpRightHand = vec3(position.x, position.z, -position.y);
	gl_Position = projection * view * model * vec4(yUpRightHand, 1.0f);
	vertexColor = color;
}
