#version 120
attribute vec3 position;
attribute vec3 normal;
attribute vec4 color;

varying vec4 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
	vec3 yUpRightHand = vec3(position.x, position.z, -position.y);
	gl_Position = projection * view * model * vec4(yUpRightHand, 1.0f);
	vertexColor = color;
}
