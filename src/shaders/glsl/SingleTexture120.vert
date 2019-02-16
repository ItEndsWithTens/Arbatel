#version 120

#ifdef GL_ARB_uniform_buffer_object
#extension GL_ARB_uniform_buffer_object : require
#endif

attribute vec3 position;
attribute vec3 normal;
attribute vec4 color;
attribute vec2 texCoords;

varying vec4 colorFromVert;
varying vec2 texCoordsFromVert;

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

	colorFromVert = color;
	texCoordsFromVert = texCoords;
}
