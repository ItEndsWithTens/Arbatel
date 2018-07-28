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

		public Vector3 BasisS;
		public Vector3 BasisT;
		public float OffsetS;
		public float OffsetT;
		public float ScaleS;
		public float ScaleT;

		/// <summary>
		/// The texture coordinates at each index of this 
		/// </summary>
		public Dictionary<int, Vector2> TexCoords;

		public Polygon()
		{
			Indices = new List<int>();
			TexCoords = new Dictionary<int, Vector2>();
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

		public List<Polygon> Polygons;

		public Dictionary<GLSurface, Buffers> Buffers;

		private readonly int VertexSize = Marshal.SizeOf(typeof(Vertex));

		public Renderable()
		{
			Position = new Vector3(0.0f, 0.0f, 0.0f);
			Vertices = new List<Vertex>();
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
			// Quake maps, like all right-thinking, clever, handsome developers,
			// use left-handed, Z-up world coordinates. The Camera class, in
			// contrast, uses right-handed, Y-up coordinates.
			//var model = Matrix4.CreateTranslation(Position.X, Position.Z, -Position.Y);
			//shader.SetMatrix4("model", ref model);

			Buffers b = Buffers[surface];

			

			GL.BindVertexArray(b.Vao);
			GL.BindBuffer(BufferTarget.ArrayBuffer, b.Vbo);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, b.Ebo);

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, MainForm.testTextureID);
			//GL.Uniform1(GL.GetUniformLocation(shader.Program, "testTexture"), 0);

			for (var i = 0; i < Polygons.Count; i++)
			{
				Polygon p = Polygons[i];

				GL.BufferData(BufferTarget.ElementArrayBuffer, p.Indices.Count * 4, p.Indices.ToArray(), BufferUsageHint.DynamicDraw);


				GL.Uniform3(GL.GetUniformLocation(shader.Program, "basisS"), ref p.BasisS);
				GL.Uniform3(GL.GetUniformLocation(shader.Program, "basisT"), ref p.BasisT);
				GL.Uniform1(GL.GetUniformLocation(shader.Program, "offsetS"), p.OffsetS);
				GL.Uniform1(GL.GetUniformLocation(shader.Program, "offsetT"), p.OffsetT);
				GL.Uniform1(GL.GetUniformLocation(shader.Program, "scaleS"), p.ScaleS);
				GL.Uniform1(GL.GetUniformLocation(shader.Program, "scaleT"), p.ScaleT);

				GL.DrawElements(BeginMode.Triangles, p.Indices.Count, DrawElementsType.UnsignedInt, 0);
				continue;



				//var distinct = p.Indices.Distinct().ToList();

				// THIS SHIT IS WRONG, I'm not looping through the vertices one at a time; I'm going polygon
				// by polygon, since each face involves different vertices, with different indices. I need to
				// set offset at the beginning of each loop, to (current index * VertexSize) + 40, to get an
				// arbitrary vertex's texCoords position. Here goes nothing.
				//// 12 bytes for position, 12 for normal, 16 for color.
				//var offset = IntPtr.Zero + (12 + 12 + 16);
				var pitch = VertexSize;
				//foreach (var index in p.Indices)
				for (var j = 0; j < p.Indices.Count; j++)
				//for (var j = 0; j < p.Indices.Distinct().ToList().Count; j++)
				//for (var j = 0; j < distinct.Count; j++)
				{
					var index = p.Indices[j];
					//var index = distinct[j];

					var offset = IntPtr.Zero + ((VertexSize * index) + 40);

					var coords = new float[2] { p.TexCoords[index].X , p.TexCoords[index].Y };

					GL.BufferSubData(BufferTarget.ArrayBuffer, offset, 8, coords);
					//offset += pitch;


					

					//var readEbo = new int[6];
					//var readVboPosition = new float[3];
					//var readVboNormal = new float[3];
					//var readVboColor = new float[4];
					//var readVboTexCoords = new float[2];
					//
					//var baseOffset = IntPtr.Zero + (VertexSize * 1);
					//GL.GetBufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, 6 * 4, readEbo);
					//GL.GetBufferSubData(BufferTarget.ArrayBuffer, baseOffset, 3 * 4, readVboPosition);
					//GL.GetBufferSubData(BufferTarget.ArrayBuffer, baseOffset + (3 * 4), 3 * 4, readVboNormal);
					//GL.GetBufferSubData(BufferTarget.ArrayBuffer, baseOffset + (6 * 4), 4 * 4, readVboColor);
					//GL.GetBufferSubData(BufferTarget.ArrayBuffer, baseOffset + (10 * 4), 2 * 4, readVboTexCoords);

					var randomthing = 4;

				}


				//var readEbo = new int[6];
				//var readVboPosition = new float[3];
				//var readVboNormal = new float[3];
				//var readVboColor = new float[4];
				//var readVboTexCoords = new float[2];

				//var baseOffset = IntPtr.Zero + (VertexSize * 2);
				//GL.GetBufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, 6 * 4, readEbo);
				//GL.GetBufferSubData(BufferTarget.ArrayBuffer, baseOffset, 3 * 4, readVboPosition);
				//GL.GetBufferSubData(BufferTarget.ArrayBuffer, baseOffset + (3 * 4), 3 * 4, readVboNormal);
				//GL.GetBufferSubData(BufferTarget.ArrayBuffer, baseOffset + (6 * 4), 4 * 4, readVboColor);
				//GL.GetBufferSubData(BufferTarget.ArrayBuffer, baseOffset + (10 * 4), 2 * 4, readVboTexCoords);

				GL.DrawElements(BeginMode.Triangles, p.Indices.Count, DrawElementsType.UnsignedInt, 0);

				//surface.SwapBuffers();

				

				var breakvar = 4;
			}

			//GL.BindTexture(TextureTarget.Texture2D, 0);

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

			// TexCoords
			GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, VertexSize, sizeof(float) * 10);
			GL.EnableVertexAttribArray(3);

			
			GL.BufferData(BufferTarget.ArrayBuffer, VertexSize * Vertices.Count, Vertices.ToArray(), BufferUsageHint.DynamicDraw);

			

			//// A given renderable can have an arbitrary number of polygons, and
			//// each of those can have an arbitrary number of indices.
			//var indexCount = 0;
			//foreach (var polygon in Polygons)
			//{
			//	indexCount += polygon.Indices.Count;
			//}
			//
			//// Initialize the element buffer, but don't copy anything yet.
			//GL.BufferData(BufferTarget.ElementArrayBuffer, 4 * indexCount, IntPtr.Zero, BufferUsageHint.StaticDraw);
			//
			//// Now handle each polygon's indices separately.
			//var offset = IntPtr.Zero;
			//for (var i = 0; i < Polygons.Count; i++)
			//{
			//	List<int> indices = Polygons[i].Indices;
			//
			//	GL.BufferSubData(BufferTarget.ElementArrayBuffer, offset, indices.Count * 4, indices.ToArray());
			//
			//	offset += indices.Count * 4;
			//}
			//
			////var test = new int[24];
			////GL.GetBufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, 4 * 24, test);


			

			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			
		}
	}
}
