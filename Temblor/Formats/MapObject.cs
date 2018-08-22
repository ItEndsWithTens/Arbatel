using Eto.Gl;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Controls;
using Temblor.Graphics;

namespace Temblor.Formats
{
	public class MapObject
	{
		public Aabb AABB { get; protected set; }

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
		public Dictionary<string, List<string>> KeyVals;

		private Vector3 _position;
		public Vector3 Position
		{
			get { return _position; }
			set
			{
				var diff = value - _position;

				AABB.Min += diff;
				AABB.Max += diff;

				_position = value;
			}
		}

		/// <summary>
		/// Anything associated with this MapObject that's meant to be rendered.
		/// </summary>
		/// <remarks>
		/// A 3D solid, for example, or a UI element that's attached to this
		/// object and should be drawn whenever the object is drawn.
		/// </remarks>
		public List<Renderable> Renderables;

		/// <summary>
		/// The TextureCollection containing the textures used by this MapObject.
		/// </summary>
		public TextureCollection TextureCollection;

		public bool Translucent;

		/// <summary>
		/// Any custom data associated with this MapObject.
		/// </summary>
		public object UserData { get; set; }

		public MapObject()
		{
			AABB = new Aabb();
			Children = new List<MapObject>();
			Color = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
			KeyVals = new Dictionary<string, List<string>>();
			Renderables = new List<Renderable>();
			Translucent = false;
		}
		public MapObject(Block _block, DefinitionCollection _definitions) : this()
		{
		}
		public MapObject(Block _block, DefinitionCollection _definitions, TextureCollection _textures) : this()
		{
		}
		public MapObject(MapObject mo)
		{
			AABB = new Aabb(mo.AABB);
			Children = new List<MapObject>(mo.Children);
			Color = new Color4(mo.Color.R, mo.Color.G, mo.Color.B, mo.Color.A);
			Definition = new Definition(mo.Definition);
			KeyVals = new Dictionary<string, List<string>>(mo.KeyVals);
			Position = new Vector3(mo.Position);
			Renderables = new List<Renderable>(mo.Renderables);
			TextureCollection = new TextureCollection(mo.TextureCollection);
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

				points.Add(child.AABB.Min);
				points.Add(child.AABB.Max);
			}

			foreach (Renderable renderable in Renderables)
			{
				renderable.Init(shader, surface);

				points.Add(renderable.AABB.Min);
				points.Add(renderable.AABB.Max);
			}

			if (points.Count > 0)
			{
				AABB = new Aabb(points);
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
		}
		
		virtual public Aabb UpdateBounds()
		{
			AABB.Min = new Vector3(Position);
			AABB.Max = new Vector3(Position);

			foreach (var child in Children)
			{
				AABB += child.UpdateBounds();
			}

			foreach (var renderable in Renderables)
			{
				AABB += renderable.UpdateBounds();
			}

			return AABB;
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
