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
using Temblor.Formats;
using Temblor.Utilities;

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

	public class Polygon
	{
		/// <summary>
		/// The vertex indices of this polygon, relative to the Vertices list of
		/// the Renderable containing it.
		/// </summary>
		public List<int> Indices;

		public string TextureName;
		public Vector3 BasisS;
		public Vector3 BasisT;
		public Vector2 Offset;
		public Vector2 Scale;

		public Polygon()
		{
			Indices = new List<int>();
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
		// NO, they should be relative to whatever; add a custom model matrix, per Renderable,
		// that accommodates however a given Renderable's vertices are represented.
		/// <summary>
		/// Vertices of this object, with coordinates relative to its Position.
		/// </summary>
		public List<Vertex> Vertices;

		/// <summary>
		/// The vertex indices of this object, relative to the Vertices list.
		/// </summary>
		public List<int> Indices;

		public List<Polygon> Polygons;

		public Dictionary<GLSurface, Buffers> Buffers;

		private readonly int VertexSize = Marshal.SizeOf(typeof(Vertex));

		public Renderable()
		{
			Position = new Vector3(0.0f, 0.0f, 0.0f);
			Vertices = new List<Vertex>();
			Indices = new List<int>();
			Polygons = new List<Polygon>();
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
			Buffers b = Buffers[surface];

			GL.BindVertexArray(b.Vao);
			GL.BindAttribLocation(shader.Program, 0, "position");
			GL.BindAttribLocation(shader.Program, 1, "normal");
			GL.BindAttribLocation(shader.Program, 2, "color");
			GL.BindAttribLocation(shader.Program, 3, "texCoords");

			GL.BindBuffer(BufferTarget.ArrayBuffer, b.Vbo);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, b.Ebo);

			GL.ActiveTexture(TextureUnit.Texture0);

			int locationBasisS = GL.GetUniformLocation(shader.Program, "basisS");
			int locationBasisT = GL.GetUniformLocation(shader.Program, "basisT");
			int locationOffset = GL.GetUniformLocation(shader.Program, "offset");
			int locationScale = GL.GetUniformLocation(shader.Program, "scale");
			int locationTextureWidth = GL.GetUniformLocation(shader.Program, "textureWidth");
			int locationTextureHeight = GL.GetUniformLocation(shader.Program, "textureHeight");

			IntPtr elementOffset = IntPtr.Zero;
			for (var i = 0; i < Polygons.Count; i++)
			{
				Polygon p = Polygons[i];

				GL.Uniform3(locationBasisS, ref p.BasisS);
				GL.Uniform3(locationBasisT, ref p.BasisT);
				GL.Uniform2(locationOffset, p.Offset);
				GL.Uniform2(locationScale, p.Scale);

				Texture texture = MainForm.Wad.Textures[p.TextureName.ToLower()];
				GL.Uniform1(locationTextureWidth, (float)texture.Width);
				GL.Uniform1(locationTextureHeight, (float)texture.Height);

				GL.BindTexture(TextureTarget.Texture2D, MainForm.testTextureDict[p.TextureName.ToLower()]);

				// The last parameter of DrawRangeElements is a perhaps poorly
				// labeled offset into the element buffer.
				GL.DrawRangeElements(PrimitiveType.Triangles, p.Indices.Min(), p.Indices.Max(), p.Indices.Count, DrawElementsType.UnsignedInt, elementOffset);
				elementOffset += p.Indices.Count * 4;

				GL.BindTexture(TextureTarget.Texture2D, 0);
			}

			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
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

			var error = GL.GetError();

			GL.BindVertexArray(b.Vao);
			GL.BindBuffer(BufferTarget.ArrayBuffer, b.Vbo);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, b.Ebo);

			// Configure position element.
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VertexSize, 0);
			GL.EnableVertexAttribArray(0);

			// Normal
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, VertexSize, sizeof(float) * 3);
			GL.EnableVertexAttribArray(1);

			// Color
			GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, VertexSize, sizeof(float) * 6);
			GL.EnableVertexAttribArray(2);

			GL.BufferData(BufferTarget.ArrayBuffer, VertexSize * Vertices.Count, Vertices.ToArray(), BufferUsageHint.DynamicDraw);

			GL.BufferData(BufferTarget.ElementArrayBuffer, 4 * Indices.Count, Indices.ToArray(), BufferUsageHint.DynamicDraw);

			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}
	}
}
