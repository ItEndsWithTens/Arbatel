using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;

namespace Temblor.Graphics
{
	public class RenderableGenerator
	{
		public Color4 Color;

		public RenderableGenerator()
		{
			Color = Color4.White;
		}

		virtual public Renderable Generate()
		{
			return new Renderable();
		}
	}
}
