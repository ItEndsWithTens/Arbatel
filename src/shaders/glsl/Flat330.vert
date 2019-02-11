#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec4 color;

out vec4 vertexColor;

layout (std140) uniform Matrices
{
	mat4 projection;
	mat4 view;
};
uniform mat4 model;

layout (std140) uniform TextureInfo
{	
	float textureWidth;
	float textureHeight;
	vec4 basisS;
	vec4 basisT;
	vec2 offset;
	vec2 scale;
};

void main()
{
	// Quake maps, like all clever, handsome developers, use
	// left-handed, Z-up world coordinates. OpenGL, in contrast,
	// uses right-handed, Y-up coordinates.
	vec3 yUpRightHand = vec3(position.x, position.z, -position.y);
	gl_Position = projection * view * model * vec4(yUpRightHand, 1.0f);
	vertexColor = color;
}
