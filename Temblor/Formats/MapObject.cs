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

		public List<MapObject> Children;

		public List<Renderable> Renderables;

		public MapObject() : this(new Block())
		{
		}
		public MapObject(Block _block)
		{
			Block = _block;

			Children = new List<MapObject>();

			Renderables = new List<Renderable>();
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

		virtual protected void ExtractRenderables(Block block)
		{
		}
	}
}
