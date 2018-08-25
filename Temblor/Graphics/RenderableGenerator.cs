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
		public Color4 Color = Color4.White;

		public Transformability Transformability;

		public RenderableGenerator()
		{
		}
		public RenderableGenerator(Color4 _color)
		{
			Color = _color;
		}

		virtual public Renderable Generate()
		{
			return new Renderable();
		}
	}
}
