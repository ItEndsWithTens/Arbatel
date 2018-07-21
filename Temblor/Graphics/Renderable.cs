using Eto.Gl;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Temblor.Controls;

namespace Temblor.Graphics
{
	public class Buffers
	{
		public int Vao;
		public int Vbo;
		public int Ebo;

		public Buffers()
		{
			GL.GenVertexArrays(1, out Vao);
			GL.GenBuffers(1, out Vbo);
			GL.GenBuffers(1, out Ebo);
		}

		public void CleanUp()
		{
			GL.DeleteBuffer(Ebo);
			GL.DeleteBuffer(Vbo);
			GL.DeleteVertexArray(Vao);
		}
	}

	/// <summary>
	/// Any 2D or 3D object that can be drawn on screen.
	/// </summary>
	/// <remarks>
	/// Vector3s are used even for 2D objects to allow simple depth sorting.
	/// </remarks>
	public class Renderable
	{
		/// <summary>
		/// Position of this object in left-handed, Z-up world coordinates.
		/// </summary>
		public Vector3 Position;

		// TODO: Are these actually relative to Position? Or are they also world coords?
		/// <summary>
		/// Vertices of this object, with coordinates relative to its Position.
		/// </summary>
		public List<Vertex> Vertices;

		/// <summary>
		/// The vertex indices of this object, relative to the Vertices list.
		/// </summary>
		public List<int> Indices;

		public Dictionary<GLSurface, Buffers> Buffers;

		private readonly int VertexSize = Marshal.SizeOf(typeof(Vertex));

		public Renderable()
		{
			Position = new Vector3(0.0f, 0.0f, 0.0f);
			Vertices = new List<Vertex>();
			Indices = new List<int>();

			Buffers = new Dictionary<GLSurface, Buffers>();
		}
		public Renderable(List<Vector3> vertices) : this()
		{
			foreach (var vertex in vertices)
			{
				Vertices.Add(new Vertex(vertex.X, vertex.Y, vertex.Z));
			}
		}

		public void Draw(Shader shader, GLSurface surface)
		{
			// Quake maps, like all right-thinking, clever, handsome developers,
			// uses left-handed, Z-up world coordinates. The Camera class, in
			// contrast, uses right-handed, Y-up coordinates.
			var model = Matrix4.CreateTranslation(Position.X, Position.Z, -Position.Y);
			shader.SetMatrix4("model", ref model);

			GL.BindVertexArray(Buffers[surface].Vao);

			GL.DrawElements(BeginMode.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);

			GL.BindVertexArray(0);
		}

		public void Init(GLSurface surface)
		{
			surface.MakeCurrent();

			Buffers b;

			if (Buffers.ContainsKey(surface))
			{
				b = Buffers[surface];

				b.CleanUp();
			}
			else
			{
				b = new Buffers();

				Buffers.Add(surface, b);
			}
			
			GL.BindVertexArray(b.Vao);

			GL.BindBuffer(BufferTarget.ArrayBuffer, b.Vbo);
			GL.BufferData(BufferTarget.ArrayBuffer, VertexSize * Vertices.Count, Vertices.ToArray(), BufferUsageHint.StaticDraw);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, b.Ebo);
			GL.BufferData(BufferTarget.ElementArrayBuffer, 4 * Indices.Count, Indices.ToArray(), BufferUsageHint.StaticDraw);

			// Configure position element.
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VertexSize, 0);
			GL.EnableVertexAttribArray(0);

			// Normal
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, VertexSize, sizeof(float) * 3);
			GL.EnableVertexAttribArray(1);

			// Color
			GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, VertexSize, sizeof(float) * 6);
			GL.EnableVertexAttribArray(2);

			// TexCoords
			GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, VertexSize, sizeof(float) * 10);
			GL.EnableVertexAttribArray(3);

			GL.BindVertexArray(0);
		}
	}
}
