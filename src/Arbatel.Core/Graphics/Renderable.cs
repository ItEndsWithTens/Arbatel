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
using Arbatel.Controls;
using Arbatel.Formats;
using Arbatel.Utilities;

namespace Arbatel.Graphics
{
	public static class RenderableExtensions
	{
		public static Renderable ToWorld(this Renderable renderable)
		{
			var world = new Renderable(renderable);

			if (renderable.CoordinateSpace != CoordinateSpace.World)
			{
				var worldVerts = new List<Vertex>();
				foreach (var vertex in renderable.Vertices)
				{
					worldVerts.Add(vertex.ModelToWorld(renderable.ModelMatrix));
				}

				world.Vertices = worldVerts;
			}

			return world;
		}
	}

	public enum CoordinateSpace
	{
		Model,
		World,
		View,
		Projection,
		Screen
	}

	/// <summary>
	/// How a Renderable should be transformed when transforming the MapObject
	/// it belongs to.
	/// </summary>
	public enum Transformability
	{
		None = 0x0,
		Translate = 0x1,
		Rotate = 0x2,
		Scale = 0x4,
		All = Translate | Rotate | Scale
	}

	public class Polygon
	{
		/// <summary>
		/// The vertex indices of this polygon, relative to the Vertices list of
		/// the Renderable containing it.
		/// </summary>
		public List<int> Indices;

		public Texture Texture { get; set; } = new Texture();
		public Vector3 BasisS;
		public Vector3 BasisT;
		public Vector2 Offset;
		public float Rotation;
		public Vector2 Scale;

		public Vector3 Normal;

		public IntPtr IndexOffset;

		public Polygon()
		{
			Indices = new List<int>();
		}
		public Polygon(Polygon p)
		{
			Indices = new List<int>(p.Indices);
			Texture = p.Texture;
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

		/// <summary>
		/// The coordinate space in which this Renderable's vertices are stored.
		/// </summary>
		/// <remarks>Used to set the ModelMatrix for this Renderable.</remarks>
		public CoordinateSpace CoordinateSpace { get; set; }

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

		public List<Polygon> Polygons { get; } = new List<Polygon>();

		protected Vector3 _position;
		public Vector3 Position
		{
			get { return _position; }
			set
			{
				TranslateRelative(value - _position);
				_position = value;
			}
		}

		/// <summary>
		/// A mapping of requested shading style to supported shading style.
		/// </summary>
		public Dictionary<ShadingStyle, ShadingStyle> ShadingStyleDict;

		/// <summary>
		/// What transformations this Renderable supports.
		/// </summary>
		public Transformability Transformability { get; set; }

		public bool Translucent;

		/// <summary>
		/// Vertices of this object.
		/// </summary>
		public List<Vertex> Vertices;

		// TODO: Hoist these up into the backend? Make a dictionary, keyed by a Renderable
		// instance with a value of a tuple, (VertexOffset, IndexOffset). In principle, things
		// to be rendered shouldn't need to carry around information about their place in the
		// backend's buffers, right? Even though this doesn't involve any OpenTK/OpenGL-specific
		// stuff, it's still ugly, and I think unnecessary.
		/// <summary>
		/// The starting offset in bytes of this renderable's vertices, relative
		/// to the back end buffer they're stored in.
		/// </summary>
		public IntPtr VertexOffset { get; set; } = IntPtr.Zero;
		/// <summary>
		/// The starting offset in bytes of this renderable's vertex indices,
		/// relative to the back end buffer they're stored in.
		/// </summary>
		public IntPtr IndexOffset { get; set; } = IntPtr.Zero;

		public Renderable()
		{
			AABB = new Aabb();
			CoordinateSpace = CoordinateSpace.World;
			Indices = new List<int>();
			ModelMatrix = Matrix4.Identity;
			ShadingStyleDict = new Dictionary<ShadingStyle, ShadingStyle>().Default();
			Transformability = Transformability.All;
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
		public Renderable(Renderable r)
		{
			AABB = new Aabb(r.AABB);
			CoordinateSpace = r.CoordinateSpace;
			Indices = new List<int>(r.Indices);
			ModelMatrix = r.ModelMatrix;
			Polygons = new List<Polygon>(r.Polygons);
			_position = r.Position;
			ShadingStyleDict = new Dictionary<ShadingStyle, ShadingStyle>(r.ShadingStyleDict);
			Translucent = r.Translucent;
			Vertices = new List<Vertex>(r.Vertices);
		}

		protected void Rotate(Vector3 rotation)
		{
			Rotate(rotation.Y, rotation.Z, rotation.X);
		}
		protected void Rotate(float pitch, float yaw, float roll)
		{
			if (Transformability.HasFlag(Transformability.Rotate))
			{
				for (var i = 0; i < Vertices.Count; i++)
				{
					var world = Vertices[i].ModelToWorld(ModelMatrix);
					var rotated = Vertex.Rotate(world, pitch, yaw, roll);
					Vertices[i] = rotated.WorldToModel(ModelMatrix);
				}

				for (var i = 0; i < Polygons.Count; i++)
				{
					Polygons[i] = Polygon.Rotate(Polygons[i], pitch, yaw, roll);
				}
			}
			else if (Transformability.HasFlag(Transformability.Translate))
			{
				Position = Position.Rotate(pitch, yaw, roll);
			}

			UpdateBounds();
		}

		protected void Scale(Vector3 scale)
		{
			Scale(scale.X, scale.Y, scale.Z);
		}
		protected void Scale(float x, float y, float z)
		{
			// TODO: Implement scale.
		}

		public void Transform(Vector3 translation, Vector3 rotation, Vector3 scale)
		{
			if (Transformability.HasFlag(Transformability.Scale) && scale != null)
			{
				Scale(scale);
			}

			if (rotation != null)
			{
				Rotate(rotation);
			}

			if (Transformability.HasFlag(Transformability.Translate) && translation != null)
			{
				TranslateRelative(translation);
			}

			UpdateBounds();

			_position = AABB.Center;
		}

		protected void TranslateRelative(Vector3 diff)
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

			if (CoordinateSpace == CoordinateSpace.Model)
			{
				var yUpRightHand = new Vector3(diff.X, diff.Z, -diff.Y);
				ModelMatrix *= Matrix4.CreateTranslation(yUpRightHand);
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

		public virtual void UpdateTextureCoordinates()
		{
		}

		public void UpdateTextures(TextureDictionary textures)
		{
			foreach (var polygon in Polygons)
			{
				if (textures.ContainsKey(polygon.Texture.Name))
				{
					polygon.Texture = textures[polygon.Texture.Name];
				}
			}

			UpdateTextureCoordinates();
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
					//ModelMatrix = Matrix4.CreateTranslation(Position.X, Position.Z, -Position.Y);
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
