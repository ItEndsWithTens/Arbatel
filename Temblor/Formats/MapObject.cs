using Eto.Gl;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Graphics;

namespace Temblor.Formats
{
	public class MapObject
	{
		/// <summary>
		/// The raw block of text that was parsed to create this object.
		/// </summary>
		public Block Block;

		/// <summary>
		/// A list of MapObjects nested within this one.
		/// </summary>
		/// <remarks>
		/// For example, in a Quake map, a func_group would be its own
		/// MapObject, and a func_detail inside it would be a child.
		/// </remarks>
		public List<MapObject> Children;

		/// <summary>
		/// Anything associated with this MapObject that's meant to be rendered.
		/// </summary>
		/// <remarks>
		/// A 3D solid, for example, or a UI element that's attached to this
		/// object and should be drawn whenever the object is drawn.
		/// </remarks>
		public List<Renderable> Renderables;

		public Color4 Color;

		public MapObject() : this(new Block())
		{
		}
		public MapObject(Block _block)
		{
			Block = _block;

			Children = new List<MapObject>();

			Renderables = new List<Renderable>();

			Color = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
		}

		public void Draw(Shader shader, GLSurface surface, Camera camera)
		{
			for (int i = 0; i < Children.Count; i++)
			{
				Children[i].Draw(shader, surface, camera);
			}

			for (int i = 0; i < Renderables.Count; i++)
			{
				Renderables[i].Draw(shader, surface, camera);
			}
		}

		public void Init(GLSurface surface)
		{
			foreach (var child in Children)
			{
				child.Init(surface);
			}

			foreach (var renderable in Renderables)
			{
				renderable.Init(surface);
			}
		}

		virtual protected void ExtractRenderables(Block block)
		{
		}
	}
}
