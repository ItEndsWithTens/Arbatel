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
			return new Renderable();
		}
	}
}
