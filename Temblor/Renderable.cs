using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Temblor.Graphics;

namespace Temblor
{
	/// <summary>
	/// Any 2D or 3D object that can be drawn on screen.
	/// </summary>
	/// <remarks>
	/// Vector3s are used even for 2D objects to allow simple depth sorting.
	/// </remarks>
	public class Renderable
	{
		public int Vao;
		public int Vbo;
		public int Ebo;

		public List<int> Indices = new List<int>();

		public List<Vertex> Vertices = new List<Vertex>();

		public Renderable()
		{
			// TODO: Make this constructor do something useful!

			// Also note I'm assuming CCW winding for starters. I think that's the most
			// common in 3D graphics stuff? We'll see.
			var one = new Vertex(0.0f, 0.5f, 0.0f);
			var two = new Vertex(-0.5f, -0.5f, 0.0f);
			var three = new Vertex(0.5f, -0.5f, 0.0f);

			one.Color = Color4.Red;
			two.Color = Color4.Lime; // WTF? "Green" doesn't get the 255 variant, only "Lime".
			three.Color = Color4.Blue;

			Vertices.Add(one);
			Vertices.Add(two);
			Vertices.Add(three);

			Indices.Add(0);
			Indices.Add(1);
			Indices.Add(2);

			Init();
		}
		public Renderable(List<Vector3> vertices)
		{
			foreach (var vertex in vertices)
			{
				Vertices.Add(new Vertex(vertex.X, vertex.Y, vertex.Z));
			}

			Init();
		}

		public void Draw(Shader shader)
		{
			GL.BindVertexArray(Vao);
			GL.DrawArrays(PrimitiveType.Triangles, 0, Indices.Count);
			GL.BindVertexArray(0);
		}

		private void Init()
		{
			GL.GenVertexArrays(1, out Vao);
			GL.GenBuffers(1, out Vbo);
			GL.GenBuffers(1, out Ebo);

			GL.BindVertexArray(Vao);
			GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);

			int vertexSize = Marshal.SizeOf(typeof(Vertex));

			GL.BufferData(BufferTarget.ArrayBuffer, vertexSize * Vertices.Count, Vertices.ToArray(), BufferUsageHint.DynamicDraw);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo);
			GL.BufferData(BufferTarget.ElementArrayBuffer, 4 * Indices.Count, Indices.ToArray(), BufferUsageHint.DynamicDraw);

			// Configure position element.
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, 0);
			GL.EnableVertexAttribArray(0);

			// Normal
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 3);
			GL.EnableVertexAttribArray(1);

			// Color
			GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 6);
			GL.EnableVertexAttribArray(2);

			// TexCoords
			GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 10);
			GL.EnableVertexAttribArray(3);

			GL.BindVertexArray(0);
		}
	}
}
