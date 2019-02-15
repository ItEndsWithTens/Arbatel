using Arbatel.Controls;
using OpenTK.Graphics;

namespace Arbatel.Graphics
{
	public class RenderableGenerator
	{
		public Color4 Color { get; set; } = Color4.White;

		public Transformability Transformability { get; set; }

		public RenderableGenerator()
		{
		}
		public RenderableGenerator(Color4 color)
		{
			Color = color;
		}

		public virtual Renderable Generate()
		{
			var r = new Renderable();
			r.Colors[ShadingStyle.Wireframe] = (Color, r.Colors[ShadingStyle.Wireframe].selected);
			r.Colors[ShadingStyle.Flat] = (Color, r.Colors[ShadingStyle.Flat].selected);
			r.Colors[ShadingStyle.Textured] = (Color, r.Colors[ShadingStyle.Textured].selected);

			return r;
		}
	}
}
