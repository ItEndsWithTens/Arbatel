using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Graphics;

namespace Temblor.Formats
{
	public class QuakeMapObject : MapObject
	{
		public QuakeMapObject(Block _block) : this(_block as QuakeBlock)
		{
		}
		public QuakeMapObject(QuakeBlock _block) : base(_block)
		{
			foreach (var child in Block.Children)
			{
				Children.Add(new QuakeMapObject(child));
			}

			ExtractRenderables(Block);
		}

		protected override void ExtractRenderables(Block block)
		{
			var b = block as QuakeBlock;

			for (var i = 0; i < b.Sides.Count; i++)
			{
				var side = new QuakeSide(b.Sides[i]);

				Renderables.Add(new Renderable() { Position = side.Plane[0] });
				Renderables.Add(new Renderable() { Position = side.Plane[1] });
				Renderables.Add(new Renderable() { Position = side.Plane[2] });
			}
		}
	}
}
