﻿using Eto.Gl;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbatel.Graphics
{
	/// <summary>
	/// A shader that applies a solid color to a surface.
	/// </summary>
	public class FlatShader : Shader
	{
		public string[] VertexShaderSource330 =
		{
			"#version 330 core",
			"layout (location = 0) in vec3 position;",
			"layout (location = 1) in vec3 normal;",
			"layout (location = 2) in vec4 color;",
			"",
			"out vec4 vertexColor;",
			"",
			"uniform mat4 model;",
			"uniform mat4 view;",
			"uniform mat4 projection;",
			"",
			"void main()",
			"{",
			"	// Quake maps, like all clever, handsome developers, use",
			"	// left-handed, Z-up world coordinates. OpenGL, in contrast,",
			"	// uses right-handed, Y-up coordinates.",
			"	vec3 yUpRightHand = vec3(position.x, position.z, -position.y);",
			"   gl_Position = projection * view * model * vec4(yUpRightHand, 1.0f);",
			"	vertexColor = color;",
			"}"
		};
		public string[] FragmentShaderSource330 =
		{
			"#version 330 core",
			"",
			"in vec4 vertexColor;",
			"",
			"out vec4 color;",
			"",
			"void main()",
			"{",
			"	color = vertexColor;",
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
			"varying vec4 vertexColor;",
			"",
			"uniform mat4 model;",
			"uniform mat4 view;",
			"uniform mat4 projection;",
			"",
			"void main()",
			"{",
			"	vec3 yUpRightHand = vec3(position.x, position.z, -position.y);",
			"	gl_Position = projection * view * model * vec4(yUpRightHand, 1.0f);",
			"	vertexColor = color;",
			"}"
		};
		public string[] FragmentShaderSource120 =
		{
			"#version 120",
			"",
			"varying vec4 vertexColor;",
			"",
			"void main()",
			"{",
			"	gl_FragColor = vertexColor;",
			"}"
		};

		public int LocationBasisS;
		public int LocationBasisT;
		public int LocationOffset;
		public int LocationScale;
		public int LocationTextureWidth;
		public int LocationTextureHeight;

		public FlatShader() : base()
		{
		}
		public FlatShader(int major, int minor) : this()
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
		}

		public override void Draw(IEnumerable<Renderable> renderables, Camera camera)
		{
			if (renderables.Count() == 0)
			{
				return;
			}

			base.Draw(renderables, camera);

			int positionLocation = GL.GetAttribLocation(Program, "position");
			GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, Vertex.MemorySize, 0);
			GL.EnableVertexAttribArray(positionLocation);

			int normalLocation = GL.GetAttribLocation(Program, "normal");
			GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, Vertex.MemorySize, sizeof(float) * 3);
			GL.EnableVertexAttribArray(normalLocation);

			int colorLocation = GL.GetAttribLocation(Program, "color");
			GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, Vertex.MemorySize, sizeof(float) * 6);
			GL.EnableVertexAttribArray(colorLocation);

			foreach (Renderable r in renderables)
			{
				SetUniform(LocationModelMatrix, r.ModelMatrix);

				GL.DrawElements(PrimitiveType.Triangles, r.Indices.Count, DrawElementsType.UnsignedInt, r.IndexOffset);
			}
		}
	}
}
