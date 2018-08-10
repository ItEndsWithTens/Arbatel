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
		public AABB AABB { get; private set; }

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

		/// <summary>
		/// Anything associated with this MapObject that's meant to be rendered.
		/// </summary>
		/// <remarks>
		/// A 3D solid, for example, or a UI element that's attached to this
		/// object and should be drawn whenever the object is drawn.
		/// </remarks>
		public List<Renderable> Renderables;

		public bool Translucent;

		public MapObject() : this(new Block(), new DefinitionCollection())
		{
		}
		public MapObject(Block _block, DefinitionCollection _definitions)
		{
			AABB = new AABB();

			Children = new List<MapObject>();

			Color = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

			KeyVals = new Dictionary<string, List<string>>();

			Renderables = new List<Renderable>();

			Translucent = false;
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
				AABB = new AABB(points);
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
