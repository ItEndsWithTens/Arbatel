using Eto.Gl;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arbatel.Controls;
using Arbatel.Graphics;

namespace Arbatel.Formats
{
	/// <summary>
	/// What components of a MapObject should be kept when saving a Map.
	/// </summary>
	[Flags]
	public enum Saveability
	{
		/// <summary>
		/// Don't save this object.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// Save this object's entity definition.
		/// </summary>
		Entity = 0x1,

		/// <summary>
		/// Save this object's solids.
		/// </summary>
		Solids = 0x2,

		/// <summary>
		/// Save this object's children.
		/// </summary>
		Children = 0x4,

		/// <summary>
		/// Save this entire object; key/values, solids, children, and all.
		/// </summary>
		All = Entity | Solids | Children
	}

	public class MapObject
	{
		public Aabb Aabb { get; protected set; }

		/// <summary>
		/// The inclusive, flat list of all MapObjects contained by this one.
		/// </summary>
		public List<MapObject> AllObjects
		{
			get
			{
				var all = new List<MapObject>()
				{
					this
				};

				foreach (var child in Children)
				{
					all.AddRange(child.AllObjects);
				}

				return all;
			}
		}

		/// <summary>
		/// A list of MapObjects nested within this one.
		/// </summary>
		/// <remarks>
		/// For example, in a Quake map, a func_group would be its own
		/// MapObject, and a func_detail inside it would be a child.
		/// </remarks>
		public List<MapObject> Children;

		public Color4 Color;

		public Definition Definition;

		/// <summary>
		/// The set of key/value pairs currently defined for this entity.
		/// </summary>
		/// <remarks>
		/// Remember that not everything from the list of key/value pairs found
		/// in an entity Definition's KeyValsTemplate will be found here; the
		/// template is only a list of what's expected. Pairs left out of this
		/// list should be assumed to be set to their defaults, and any extras
		/// not in the template should be permitted to stay.
		/// </remarks>
		public Dictionary<string, Option> KeyVals;

		//private Vector3 position;
		//public Vector3 Position
		//{
		//	get { return position; }
		//	set
		//	{
		//		var diff = value - position;

		//		AABB.Min += diff;
		//		AABB.Max += diff;

		//		position = value;
		//	}
		//}
		public Vector3 Position { get; set; }

		/// <summary>
		/// Anything associated with this MapObject that's meant to be rendered.
		/// </summary>
		/// <remarks>
		/// A 3D solid, for example, or a UI element that's attached to this
		/// object and should be drawn whenever the object is drawn.
		/// </remarks>
		public List<Renderable> Renderables;

		/// <summary>
		/// What components of this object should be kept when saving a map.
		/// </summary>
		public Saveability Saveability { get; set; }

		/// <summary>
		/// The TextureCollection containing the textures used by this MapObject.
		/// </summary>
		public TextureDictionary TextureCollection;

		public bool Translucent;

		/// <summary>
		/// Any custom data associated with this MapObject.
		/// </summary>
		public object UserData { get; set; }

		public MapObject()
		{
			Aabb = new Aabb();
			Children = new List<MapObject>();
			Color = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
			KeyVals = new Dictionary<string, Option>();
			Renderables = new List<Renderable>();
			Translucent = false;
		}
		public MapObject(Block block, DefinitionDictionary definitions) : this()
		{
		}
		public MapObject(Block block, DefinitionDictionary definitions, TextureDictionary textures) : this()
		{
		}
		public MapObject(MapObject mo)
		{
			Aabb = new Aabb(mo.Aabb);
			Children = new List<MapObject>(mo.Children);
			Color = new Color4(mo.Color.R, mo.Color.G, mo.Color.B, mo.Color.A);
			Definition = new Definition(mo.Definition);
			KeyVals = new Dictionary<string, Option>(mo.KeyVals);
			Position = new Vector3(mo.Position);
			Renderables = new List<Renderable>(mo.Renderables);
			Saveability = mo.Saveability;
			TextureCollection = new TextureDictionary(mo.TextureCollection);
			Translucent = mo.Translucent;
			UserData = mo.UserData;
		}

		public void Draw(Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, GLSurface surface, Camera camera)
		{
			if (!camera.CanSee(this))
			{
				return;
			}

			for (int i = 0; i < Children.Count; i++)
			{
				Children[i].Draw(shaders, style, surface, camera);
			}

			for (int i = 0; i < Renderables.Count; i++)
			{
				Renderables[i].Draw(shaders, style, surface, camera);
			}
		}

		public void Init(List<View> views)
		{
			foreach (View view in views)
			{
				Init(view);
			}
		}
		public void Init(View view)
		{
			foreach (var shader in view.Shaders.Values)
			{
				Init(shader, view);
			}
		}
		public void Init(Shader shader, List<GLSurface> surfaces)
		{
			foreach (GLSurface surface in surfaces)
			{
				Init(shader, surface);
			}
		}
		public void Init(Shader shader, GLSurface surface)
		{
			var points = new List<Vector3>();

			foreach (MapObject child in Children)
			{
				child.Init(shader, surface);

				points.Add(child.Aabb.Min);
				points.Add(child.Aabb.Max);
			}

			foreach (Renderable renderable in Renderables)
			{
				renderable.Init(shader, surface);

				points.Add(renderable.AABB.Min);
				points.Add(renderable.AABB.Max);
			}

			if (points.Count > 0)
			{
				Aabb = new Aabb(points);
			}
		}

		/// <summary>
		/// Get a list of all Renderables contained by the tree of MapObjects
		/// rooted at this one.
		/// </summary>
		/// <returns></returns>
		virtual public List<Renderable> GetAllRenderables()
		{
			var totalRenderables = new List<Renderable>(Renderables);

			foreach (var child in Children)
			{
				totalRenderables.AddRange(child.GetAllRenderables());
			}

			return totalRenderables;
		}

		virtual public void Transform(Vector3 translation, Vector3 rotation, Vector3 scale)
		{
			foreach (var child in Children)
			{
				child.Transform(translation, rotation, scale);
			}

			foreach (var renderable in Renderables)
			{
				renderable.Transform(translation, rotation, scale);
			}

			// If this MapObject has a key/value pair specifying a separate
			// origin, this will be overwritten, but it takes care of entities
			// without such a key, while also updating the bounding box.
			Position = UpdateBounds().Center;

			var transformedKeyVals = new Dictionary<string, Option>();

			foreach (var kv in KeyVals)
			{
				var newOption = new Option(kv.Value);

				string[] split;
				switch (kv.Value.TransformType)
				{
					case TransformType.Angles:
						split = kv.Value.Value.Split(' ');

						var angles = new Vector3();
						float.TryParse(split[0], out angles.X);
						float.TryParse(split[1], out angles.Y);
						float.TryParse(split[2], out angles.Z);
						angles += rotation;

						newOption.Value = String.Join(" ", new float[] { angles.X, angles.Y, angles.Z });

						transformedKeyVals.Add(kv.Key, newOption);
						break;

					case TransformType.Position:
						split = kv.Value.Value.Split(' ');

						var origin = new Vertex();
						float.TryParse(split[0], out origin.Position.X);
						float.TryParse(split[1], out origin.Position.Y);
						float.TryParse(split[2], out origin.Position.Z);

						// TODO: Scale!
						origin = Vertex.Rotate(origin, rotation.Y, rotation.Z, rotation.X);
						origin.Position += translation;

						Position = origin.Position;

						newOption.Value = origin.Position.X + " " + origin.Position.Y + " " + origin.Position.Z;
						transformedKeyVals.Add(kv.Key, newOption);
						break;

					default:
						transformedKeyVals.Add(kv.Key, kv.Value);
						break;
				}
			}

			KeyVals = transformedKeyVals;
		}

		virtual public Aabb UpdateBounds()
		{
			if (Renderables.Count == 0)
			{
				return Aabb;
			}

			Vector3 baseline = Renderables[0].ToWorld().AABB.Center;
			Aabb.Min = new Vector3(baseline);
			Aabb.Max = new Vector3(baseline);

			foreach (var child in Children)
			{
				Aabb += child.UpdateBounds();
			}

			foreach (var renderable in Renderables)
			{
				Aabb += renderable.UpdateBounds();
			}

			return Aabb;
		}

		virtual public void UpdateTextures(TextureDictionary textures)
		{
			foreach (var child in Children)
			{
				child.UpdateTextures(textures);
			}

			foreach (var renderable in Renderables)
			{
				renderable.UpdateTextures(textures);
			}
		}

		virtual public bool UpdateTranslucency(List<string> translucents)
		{
			var opaqueChildren = new List<MapObject>();
			var translucentChildren = new List<MapObject>();
			foreach (var child in Children)
			{
				if (child.UpdateTranslucency(translucents))
				{
					Translucent = true;
					translucentChildren.Add(child);
				}
				else
				{
					opaqueChildren.Add(child);
				}
			}
			Children = opaqueChildren;
			Children.AddRange(translucentChildren);

			// If any renderable in this object is translucent, the entire
			// object should be considered translucent, but to ensure all
			// renderables' translucency is updated, don't break on true.
			var opaqueRenderables = new List<Renderable>();
			var translucentRenderables = new List<Renderable>();
			foreach (var renderable in Renderables)
			{
				if (renderable.UpdateTranslucency(translucents))
				{
					Translucent = true;
					translucentRenderables.Add(renderable);
				}
				else
				{
					opaqueRenderables.Add(renderable);
				}
			}
			Renderables = opaqueRenderables;
			Renderables.AddRange(translucentRenderables);

			return Translucent;
		}

		virtual protected void ExtractRenderables(Block block)
		{
		}
	}
}
