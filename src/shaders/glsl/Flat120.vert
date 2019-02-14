#version 120
attribute vec3 position;
attribute vec3 normal;
attribute vec4 color;
attribute vec2 texCoords;

varying vec4 vertexColor;

uniform Matrices
{
	mat4 projection;
	mat4 view;
};
uniform mat4 model;

void main()
{
	vec3 yUpRightHand = vec3(position.x, position.z, -position.y);
	gl_Position = projection * view * model * vec4(yUpRightHand, 1.0f);

	vertexColor = color;
}
