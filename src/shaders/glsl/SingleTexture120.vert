#version 120
attribute vec3 position;
attribute vec3 normal;
attribute vec4 color;

varying vec2 texCoords;

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
	vec3 yUpRightHand = vec3(position.x, position.z, -position.y);
	gl_Position = projection * view * model * vec4(yUpRightHand, 1.0f);

	float coordS = (dot(position, basisS) + (offset.x * scale.x)) / (textureWidth * scale.x);
	float coordT = (dot(position, basisT) + (offset.y * scale.y)) / (textureHeight * scale.y);

	texCoords = vec2(coordS, coordT);
}
