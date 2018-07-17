using Eto.Gl;
using OpenTK;
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
		public Block Block;

		public List<MapObject> Children = new List<MapObject>();

		public List<Renderable> Renderables = new List<Renderable>();

		public MapObject() : this(new Block())
		{
		}
		public MapObject(Block _block)
		{
			Block = _block;

			foreach (var child in Block.Children)
			{
				Children.Add(new MapObject(child));
			}

			ExtractRenderables(Block);
		}

		public void Draw(Shader shader)
		{
			foreach (var child in Children)
			{
				child.Draw(shader);
			}

			foreach (var renderable in Renderables)
			{
				renderable.Draw(shader);
			}
		}

		public void Init(List<GLSurface> surfaces)
		{
			foreach (var child in Children)
			{
				child.Init(surfaces);
			}

			foreach (var renderable in Renderables)
			{
				renderable.Init(surfaces);
			}
		}

		private void ExtractRenderables(Block block)
		{
			// RawStartIndex is the only unique value accessible from here, but
			// works well enough for this preliminary design stage.
			var pos = new Vector3((float)block.RawStartIndex, 0.0f, 0.0f);

			Renderables.Add(new Renderable() { Position = pos });
		}
	}
}
