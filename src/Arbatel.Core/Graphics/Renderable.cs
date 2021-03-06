﻿using Arbatel.Controls;
using Arbatel.Formats;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;

namespace Arbatel.Graphics
{
	/// <summary>
	/// Anything that, upon update, needs its representation in the back end updated.
	/// </summary>
	public interface IUpdateBackEnd
	{
		event EventHandler Updated;
	}

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

	/// <summary>
	/// Any 2D or 3D object that can be drawn on screen.
	/// </summary>
	/// <remarks>
	/// Vector3s are used even for 2D objects to allow simple depth sorting.
	/// </remarks>
	public class Renderable : IUpdateBackEnd
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
		public List<int> LineLoopIndices { get; } = new List<int>();

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

		public bool Selected { get; set; } = false;

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

		public IntPtr LineLoopIndexOffset { get; set; } = IntPtr.Zero;

		public Dictionary<ShadingStyle, (Color4 deselected, Color4 selected)> Colors { get; } = new Dictionary<ShadingStyle, (Color4, Color4)>
		{
			{ ShadingStyle.Wireframe, (Color4.White, Color4.Red) },
			{ ShadingStyle.Flat, (Color4.White, Color4.Red) },
			{ ShadingStyle.Textured, (Color4.White, Color4.Red) }
		};

		/// <summary>
		/// If defined, this color will replace this Renderable's deselected color.
		/// </summary>
		public Color4? Tint { get; set; } = null;

		public event EventHandler Updated;

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
			LineLoopIndices = new List<int>(r.LineLoopIndices);
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
			for (int i = 0; i < Vertices.Count; i++)
			{
				Vertex scaled = Vertices[i];

				scaled.Position = new Vector3
				{
					X = scaled.Position.X * x,
					Y = scaled.Position.Y * y,
					Z = scaled.Position.Z * z
				};

				Vertices[i] = scaled;
			}

			for (int i = 0; i < Polygons.Count; i++)
			{
				Polygon scaled = Polygons[i];

				// TODO: Understand, and explain in a comment, why the basis
				// vectors need to be divided by the scale instead of multiplied
				// like the vertex positions. I figured this out by observation,
				// and honestly have no idea why it works.

				scaled.BasisS = new Vector3
				{
					X = scaled.BasisS.X / x,
					Y = scaled.BasisS.Y / y,
					Z = scaled.BasisS.Z / z
				};

				scaled.BasisT = new Vector3
				{
					X = scaled.BasisT.X / x,
					Y = scaled.BasisT.Y / y,
					Z = scaled.BasisT.Z / z
				};

				Polygons[i] = scaled;
			}
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

		// TODO: Add a boolean to avoid updating the buffers? Add an overload
		// for it so it defaults to true, but allow people to avoid the update
		// so they can do a run of renderable color changes at once.
		//
		// TODO: Have SetColor return the object's previous color? To allow
		// callers to set the old color back, if they want.
		/// <summary>
		/// Give this renderable's vertices an arbitrary color, ignoring the
		/// values in its Colors dictionary.
		/// </summary>
		public void SetColor(Color4 color)
		{
			for (int i = 0; i < Vertices.Count; i++)
			{
				Vertex v = Vertices[i];
				v.Color = color;

				Vertices[i] = v;
			}

			OnUpdated();
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
			foreach (Polygon p in Polygons)
			{
				if (String.IsNullOrEmpty(p.IntendedTextureName))
				{
					continue;
				}

				if (textures.ContainsKey(p.IntendedTextureName))
				{
					p.CurrentTexture = textures[p.IntendedTextureName];
				}
				else
				{
					p.CurrentTexture = textures["ARBATEL_MISSING_TEXTURE"];
				}
			}

			UpdateTextureCoordinates();

			OnUpdated();
		}

		public bool UpdateTranslucency(List<string> translucents)
		{
			foreach (var polygon in Polygons)
			{
				if (String.IsNullOrEmpty(polygon.CurrentTexture.Name))
				{
					continue;
				}

				// The most granular translucency information that matters is
				// per-Renderable, not per-Polygon, so breaking on true is safe.
				//
				// Also, basing translucency on the actual current texture will
				// ensure that missing textures are less likely to go unnoticed
				// by way of being dim. Opaque, garish placeholders catch eyes.
				if (translucents.Contains(polygon.CurrentTexture.Name.ToLower()))
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

		protected virtual void OnUpdated()
		{
			OnUpdated(new EventArgs());
		}
		protected virtual void OnUpdated(EventArgs e)
		{
			Updated?.Invoke(this, e);
		}
	}
}
