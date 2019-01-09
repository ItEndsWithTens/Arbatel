using Eto.Gl;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arbatel.UI;

namespace Arbatel.Graphics
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
			"out vec2 texCoords;",
			"",
			"uniform mat4 model;",
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
			"   gl_Position = projection * view * model * vec4(yUpRightHand, 1.0f);",
			"",
			"	float coordS = (dot(position, basisS) + (offset.x * scale.x)) / (textureWidth * scale.x);",
			"	float coordT = (dot(position, basisT) + (offset.y * scale.y)) / (textureHeight * scale.y);",
			"",
			"	texCoords = vec2(coordS, coordT);",
			"}"
		};
		public string[] FragmentShaderSource330 =
		{
			"#version 330 core",
			"",
			"in vec2 texCoords;",
			"",
			"out vec4 color;",
			"",
			"uniform sampler2D tex;",
			"",
			"void main()",
			"{",
			"	color = texture(tex, texCoords);",
			"}"
		};

		public string[] VertexShaderSource120 =
		{
			"#version 120",
			"",
			"attribute vec3 position;",
			"attribute vec3 normal;",
			"attribute vec4 color;",
			"",
			"varying vec2 texCoords;",
			"",
			"uniform mat4 model;",
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
			"	vec3 yUpRightHand = vec3(position.x, position.z, -position.y);",
			"	gl_Position = projection * view * model * vec4(yUpRightHand, 1.0f);",
			"",
			"	float coordS = (dot(position, basisS) + (offset.x * scale.x)) / (textureWidth * scale.x);",
			"	float coordT = (dot(position, basisT) + (offset.y * scale.y)) / (textureHeight * scale.y);",
			"",
			"	texCoords = vec2(coordS, coordT);",
			"}"
		};
		public string[] FragmentShaderSource120 =
		{
			"#version 120",
			"",
			"varying vec2 texCoords;",
			"",
			"uniform sampler2D tex;",
			"",
			"void main()",
			"{",
			"	gl_FragColor = texture2D(tex, texCoords);",
			"}"
		};

		public int LocationBasisS;
		public int LocationBasisT;
		public int LocationOffset;
		public int LocationScale;
		public int LocationTextureWidth;
		public int LocationTextureHeight;

		public SingleTextureShader() : base()
		{
			LocationBasisS = 0;
			LocationBasisT = 0;
			LocationOffset = 0;
			LocationScale = 0;
			LocationTextureWidth = 0;
			LocationTextureHeight = 0;
		}
		public SingleTextureShader(int major, int minor) : this()
		{
			if (major >= 3 && minor >= 3)
			{
				Compile(VertexShaderSource330, FragmentShaderSource330);
			}
			else
			{
				Compile(VertexShaderSource120, FragmentShaderSource120);
			}

			LocationModelMatrix = GL.GetUniformLocation(Program, "model");
			LocationViewMatrix = GL.GetUniformLocation(Program, "view");
			LocationProjectionMatrix = GL.GetUniformLocation(Program, "projection");

			LocationBasisS = GL.GetUniformLocation(Program, "basisS");
			LocationBasisT = GL.GetUniformLocation(Program, "basisT");
			LocationOffset = GL.GetUniformLocation(Program, "offset");
			LocationScale = GL.GetUniformLocation(Program, "scale");
			LocationTextureWidth = GL.GetUniformLocation(Program, "textureWidth");
			LocationTextureHeight = GL.GetUniformLocation(Program, "textureHeight");
		}

		public override void Draw(Renderable renderable, GLSurface surface, Camera camera)
		{
			base.Draw(renderable, surface, camera);

			Buffers b = renderable.Buffers[surface];

			GL.BindVertexArray(b.Vao);
			GL.BindBuffer(BufferTarget.ArrayBuffer, b.Vbo);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, b.Ebo);

			GL.ActiveTexture(TextureUnit.Texture0);

			IntPtr elementOffset = IntPtr.Zero;
			for (var i = 0; i < renderable.Polygons.Count; i++)
			{
				Polygon p = renderable.Polygons[i];

				// OpenGL offers its own backface culling, if it's enabled, but
				// that only does its work after a draw call. An early backface
				// check here skips texture binding and setting the uniforms.
				Vector3 point = renderable.Vertices[p.Indices[0]];
				var yUpRightHand = new Vector4(point.X, point.Z, -point.Y, 1.0f);
				var transformed = yUpRightHand * renderable.ModelMatrix;
				var zUpLeftHand = new Vector3(transformed.X, -transformed.Z, transformed.Y);
				var toPoint = camera.WorldPosition - new Vector3(zUpLeftHand);
				if (Vector3.Dot(toPoint, p.Normal) > 0.0f)
				{
					SetUniform(LocationBasisS, p.BasisS);
					SetUniform(LocationBasisT, p.BasisT);
					SetUniform(LocationOffset, p.Offset);
					SetUniform(LocationScale, p.Scale);

					SetUniform(LocationTextureWidth, (float)p.Texture.Width);
					SetUniform(LocationTextureHeight, (float)p.Texture.Height);

					GL.BindTexture(TextureTarget.Texture2D, BackEnd.Textures[p.Texture.Name.ToLower()]);

					// The last parameter of DrawRangeElements is a perhaps poorly
					// labeled offset into the element buffer.
					GL.DrawRangeElements(PrimitiveType.Triangles, p.Indices.Min(), p.Indices.Max(), p.Indices.Count, DrawElementsType.UnsignedInt, elementOffset);
				}

				elementOffset += p.Indices.Count * sizeof(int);
			}

			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}
	}
}
