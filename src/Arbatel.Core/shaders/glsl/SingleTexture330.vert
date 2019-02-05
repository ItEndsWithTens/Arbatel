#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec4 color;

out vec2 texCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 basisS;
uniform vec3 basisT;
uniform vec2 offset;
uniform vec2 scale;
uniform float textureWidth;
uniform float textureHeight;

void main()
{
	// Quake maps, like all clever, handsome developers, use
	// left-handed, Z-up world coordinates. OpenGL, in contrast,
	// uses right-handed, Y-up coordinates.
	vec3 yUpRightHand = vec3(position.x, position.z, -position.y);
	gl_Position = projection * view * model * vec4(yUpRightHand, 1.0f);

	float coordS = (dot(position, basisS) + (offset.x * scale.x)) / (textureWidth * scale.x);
	float coordT = (dot(position, basisT) + (offset.y * scale.y)) / (textureHeight * scale.y);

	texCoords = vec2(coordS, coordT);
}
