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
	public enum CoordinateSpace
	{
		Model,
		World,
		View,
		Projection,
		Screen
	}

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

		public Texture Texture;
		public Vector3 BasisS;
		public Vector3 BasisT;
		public Vector2 Offset;
		public float Rotation;
		public Vector2 Scale;

		public Vector3 Normal;

		public Polygon()
		{
			Indices = new List<int>();
			Texture = new Texture() { Name = "NOODLES" };
		}
		public Polygon(Polygon p)
		{
			Indices = new List<int>(p.Indices);
			Texture = new Texture(p.Texture);
			BasisS = new Vector3(p.BasisS);
			BasisT = new Vector3(p.BasisT);
			Offset = new Vector2(p.Offset.X, p.Offset.Y);
			Rotation = p.Rotation;
			Scale = new Vector2(p.Scale.X, p.Scale.Y);
			Normal = new Vector3(p.Normal);
		}

		public static Polygon Rotate(Polygon polygon, float pitch, float yaw, float roll)
		{
			if (pitch < 0.0f)
			{
				pitch = Math.Abs(pitch);
			}
			else
			{
				pitch = 360.0f - pitch;
			}

			Matrix4 rotZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(pitch));
			Matrix4 rotY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(yaw));
			Matrix4 rotX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(roll));

			Matrix4 rotation = rotZ * rotY * rotX;

			var p = new Polygon(polygon);

			var yUpRightHand = new Vector4(p.BasisS.X, p.BasisS.Z, -p.BasisS.Y, 1.0f);
			Vector4 rotated = yUpRightHand * rotation;
			var zUpLeftHand = new Vector3(rotated.X, -rotated.Z, rotated.Y);
			p.BasisS = zUpLeftHand;

			yUpRightHand = new Vector4(p.BasisT.X, p.BasisT.Z, -p.BasisT.Y, 1.0f);
			rotated = yUpRightHand * rotation;
			zUpLeftHand = new Vector3(rotated.X, -rotated.Z, rotated.Y);
			p.BasisT = zUpLeftHand;

			yUpRightHand = new Vector4(p.Normal.X, p.Normal.Z, -p.Normal.Y, 1.0f);
			rotated = yUpRightHand * rotation;
			zUpLeftHand = new Vector3(rotated.X, -rotated.Z, rotated.Y);
			p.Normal = zUpLeftHand;

			return p;
		}

		public static Polygon TranslateRelative(Polygon polygon, Vector3 diff)
		{
			var p = new Polygon(polygon);

			// The dot product projects one vector onto another, in essence
			// describing how far along one of them the other is. That gives the
			// relative offset on the respective basis vector, though scaled.
			p.Offset.X -= Vector3.Dot(diff, p.BasisS) / p.Scale.X;
			p.Offset.Y -= Vector3.Dot(diff, p.BasisT) / p.Scale.Y;

			return p;
		}
		public static Polygon TranslateRelative(Polygon polygon, float diffX, float diffY, float diffZ)
		{
			return TranslateRelative(polygon, new Vector3(diffX, diffY, diffZ));
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
		public Aabb AABB { get; protected set; }

		public Dictionary<GLSurface, Buffers> Buffers;

		/// <summary>
		/// The coordinate space in which this Renderable's vertices are stored.
		/// </summary>
		/// <remarks>Used to set the ModelMatrix for this Renderable.</remarks>
		private CoordinateSpace _coordinateSpace;
		public CoordinateSpace CoordinateSpace
		{
			get { return _coordinateSpace; }
			set
			{
				_coordinateSpace = value;

				UpdateModelMatrix();
			}
		}

		/// <summary>
		/// The vertex indices of this object, relative to the Vertices list.
		/// </summary>
		public List<int> Indices;

		/// <summary>
		/// The transformation matrix used to bring this Renderable's vertices
		/// into world space from object space.
		/// </summary>
		/// <remarks>Allows for object's vertices to be stored as coordinates in
		/// world space, object space, or anything else. Actual 3D objects can
		/// be drawn with minimal effort, as well as UI elements associated with
		/// said objects, or placeholder meshes, etc. Set the CoordinateSpace
		/// field to switch the matrix used for this Renderable.</remarks>
		public Matrix4 ModelMatrix { get; protected set; }

		public List<Polygon> Polygons;

		private Vector3 _position;
		public Vector3 Position
		{
			get { return _position; }
			set
			{
				TranslateRelative(value - _position);
				_position = value;

				UpdateModelMatrix();
			}
		}

		/// <summary>
		/// A mapping of requested shading style to supported shading style.
		/// </summary>
		public Dictionary<ShadingStyle, ShadingStyle> ShadingStyleDict;

		/// <summary>
		/// The TextureCollection containing this Renderable's textures.
		/// </summary>
		public TextureCollection TextureCollection;

		public bool Translucent;

		/// <summary>
		/// Vertices of this object.
		/// </summary>
		public List<Vertex> Vertices;

		private readonly int VertexSize = Marshal.SizeOf(typeof(Vertex));

		public Renderable()
		{
			AABB = new Aabb();
			Buffers = new Dictionary<GLSurface, Buffers>();
			CoordinateSpace = CoordinateSpace.World;
			Indices = new List<int>();
			ModelMatrix = Matrix4.Identity;
			Polygons = new List<Polygon>();
			ShadingStyleDict = new Dictionary<ShadingStyle, ShadingStyle>().Default();
			TextureCollection = new TextureCollection();
			Translucent = false;
			Vertices = new List<Vertex>();
		}
		public Renderable(List<Vector3> points) : this()
		{
			foreach (var point in points)
			{
				Vertices.Add(new Vertex(point.X, point.Y, point.Z));
			}

			AABB = new Aabb(Vertices);
		}
		public Renderable(List<Vertex> vertices) : this()
		{
			foreach (var vertex in vertices)
			{
				Vertices.Add(vertex);
			}

			AABB = new Aabb(Vertices);
		}

		public void Draw(Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, GLSurface surface, Camera camera)
		{
			ShadingStyle actualStyle = ShadingStyleDict[style];

			shaders[actualStyle].Draw(this, surface, camera);
		}

		public void Init(Shader shader, GLSurface surface)
		{
			surface.MakeCurrent();

			Buffers b;

			if (Buffers.ContainsKey(surface))
			{
				b = Buffers[surface];
			}
			else
			{
				b = new Buffers();

				Buffers.Add(surface, b);
			}

			GL.BindVertexArray(b.Vao);
			GL.BindBuffer(BufferTarget.ArrayBuffer, b.Vbo);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, b.Ebo);

			// Configure position element.
			int positionLocation = GL.GetAttribLocation(shader.Program, "position");
			GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, VertexSize, 0);
			GL.EnableVertexAttribArray(positionLocation);

			// Normal
			int normalLocation = GL.GetAttribLocation(shader.Program, "normal");
			GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, VertexSize, sizeof(float) * 3);
			GL.EnableVertexAttribArray(normalLocation);

			// Color
			int colorLocation = GL.GetAttribLocation(shader.Program, "color");
			GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, VertexSize, sizeof(float) * 6);
			GL.EnableVertexAttribArray(colorLocation);

			GL.BufferData(BufferTarget.ArrayBuffer, VertexSize * Vertices.Count, Vertices.ToArray(), BufferUsageHint.StaticDraw);

			GL.BufferData(BufferTarget.ElementArrayBuffer, 4 * Indices.Count, Indices.ToArray(), BufferUsageHint.StaticDraw);

			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		public void Rotate(Vector3 rotation)
		{
			Rotate(rotation.Y, rotation.Z, rotation.X);
		}
		public void Rotate(float pitch, float yaw, float roll)
		{
			for (var i = 0; i < Vertices.Count; i++)
			{
				Vertices[i] = Vertex.Rotate(Vertices[i], pitch, yaw, roll);
			}

			for (var i = 0; i < Polygons.Count; i++)
			{
				Polygons[i] = Polygon.Rotate(Polygons[i], pitch, yaw, roll);
			}

			UpdateBounds();
		}

		public void Scale(Vector3 scale)
		{
			Scale(scale.X, scale.Y, scale.Z);
		}
		public void Scale(float x, float y, float z)
		{
			// TODO: Implement scale.
		}

		public void Transform(Vector3 translation, Vector3 rotation, Vector3 scale)
		{
			if (scale != null)
			{
				Scale(scale);
			}

			if (rotation != null)
			{
				Rotate(rotation);
			}

			if (translation != null)
			{
				Position += translation;
			}
		}

		public void TranslateRelative(Vector3 diff)
		{
			if (CoordinateSpace == CoordinateSpace.World)
			{
				for (var i = 0; i < Vertices.Count; i++)
				{
					Vertices[i] = Vertex.TranslateRelative(Vertices[i], diff);
				}
			}

			for (var i = 0; i < Polygons.Count; i++)
			{
				Polygons[i] = Polygon.TranslateRelative(Polygons[i], diff);
			}

			AABB += diff;
		}
		public void TranslateRelative(float diffX, float diffY, float diffZ)
		{
			TranslateRelative(new Vector3(diffX, diffY, diffZ));
		}

		public Aabb UpdateBounds()
		{
			var worldVerts = new List<Vector3>();
			foreach (var v in Vertices)
			{
				var model = new Vector4(v.Position.X, v.Position.Z, -v.Position.Y, 1.0f);
				var world = model * ModelMatrix;

				var updated = new Vector3(world.X, -world.Z, world.Y);
				worldVerts.Add(updated);
			}

			AABB = new Aabb(worldVerts);

			return AABB;
		}

		public bool UpdateTranslucency(List<string> translucents)
		{
			foreach (var polygon in Polygons)
			{
				if (polygon.Texture.Name == null)
				{
					continue;
				}

				// The most granular translucency information that matters is
				// per-Renderable, not per-Polygon, so breaking on true is safe.
				if (translucents.Contains(polygon.Texture.Name.ToLower()))
				{
					Translucent = true;
					break;
				}
			}

			return Translucent;
		}

		protected void UpdateModelMatrix()
		{
			switch (CoordinateSpace)
			{
				case CoordinateSpace.Model:
					ModelMatrix = Matrix4.CreateTranslation(Position.X, Position.Z, -Position.Y);
					break;
				case CoordinateSpace.World:
				default:
					ModelMatrix = Matrix4.Identity;
					break;
				case CoordinateSpace.View:
					break;
				case CoordinateSpace.Projection:
					break;
				case CoordinateSpace.Screen:
					break;
			}
		}
	}
}
