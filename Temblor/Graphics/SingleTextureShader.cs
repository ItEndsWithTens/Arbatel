using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Graphics
{
	/// <summary>
	/// A shader that applies a single texture to a surface.
	/// </summary>
	public class SingleTextureShader : Shader
	{
		public string[] VertexShaderSource330 =
		{
			"#version 330 core",
			"layout (location = 0) in vec3 position;",
			"layout (location = 1) in vec3 normal;",
			"layout (location = 2) in vec4 color;",
			"",
			"out vec4 vertexColor;",
			"out vec2 TexCoords;",
			"",
			"uniform mat4 view;",
			"uniform mat4 projection;",
			"uniform vec3 basisS;",
			"uniform vec3 basisT;",
			"uniform vec2 offset;",
			"uniform vec2 scale;",
			"uniform float textureWidth;",
			"uniform float textureHeight;",
			"",
			"void main()",
			"{",
			"	// Quake maps, like all clever, handsome developers, use",
			"	// left-handed, Z-up world coordinates. OpenGL, in contrast,",
			"	// uses right-handed, Y-up coordinates.",
			"	vec3 yUpRightHand = vec3(position.x, position.z, -position.y);",
			"   gl_Position = projection * view * vec4(yUpRightHand, 1.0f);",
			"	vertexColor = color;",
			"",
			"	float coordS = (dot(position, basisS) + (offset.x * scale.x)) / (textureWidth * scale.x);",
			"	float coordT = (dot(position, basisT) + (offset.y * scale.y)) / (textureHeight * scale.y);",
			"",
			"	TexCoords = vec2(coordS, coordT);",
			"}"
		};
		public string[] FragmentShaderSource330 =
		{
			"#version 330 core",
			"",
			"in vec4 vertexColor;",
			"in vec2 TexCoords;",
			"",
			"out vec4 color;",
			"",
			"uniform sampler2D testTexture;",
			"",
			"void main()",
			"{",
			"   //color = vertexColor;",
			"	color = texture(testTexture, TexCoords) * vertexColor;",
			"}"
		};

		public string[] VertexShaderSource120 =
		{
			"#version 120",
			"",
			"attribute vec3 position;",
			"attribute vec3 normal;",
			"attribute vec4 color;",
			"attribute vec2 texCoords;",
			"",
			"varying vec4 vertexColor;",
			"varying vec2 TexCoords;",
			"",
			"uniform mat4 view;",
			"uniform mat4 projection;",
			"",
			"void main()",
			"{",
			"	vec3 zUpLeftHand = vec3(position.x, position.z, -position.y);",
			"	gl_Position = projection * view * vec4(zUpLeftHand, 1.0f);",
			"	vertexColor = color;",
			"	TexCoords = texCoords;",
			"}"
		};
		public string[] FragmentShaderSource120 =
		{
			"#version 120",
			"",
			"varying vec4 vertexColor;",
			"varying vec2 TexCoords;",
			"",
			"uniform sampler2D testTexture;",
			"",
			"void main()",
			"{",
			"	gl_FragColor = texture2D(testTexture, TexCoords);",
			"}"
		};

		public SingleTextureShader() : base()
		{
		}
		public SingleTextureShader(int major, int minor)
		{
			if (major >= 3 && minor >= 3)
			{
				Compile(VertexShaderSource330, FragmentShaderSource330);
			}
			else
			{
				Compile(VertexShaderSource120, FragmentShaderSource120);
			}
		}
	}
}
