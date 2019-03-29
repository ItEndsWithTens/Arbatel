using Arbatel.Graphics;

namespace Arbatel.Controls
{
	public class OpenGLViewport : Viewport
	{
		public OpenGLViewport() : base(new OpenGLBackEnd())
		{
			var oglView = new OpenGLView3d()
			{
				BackEnd = (OpenGLBackEnd)BackEnd,
				Enabled = false,
				Visible = false
			};

			Views.Add(Views.Count, (oglView, "3D Wireframe", OpenGLBackEnd.SetUpWireframe));
			Views.Add(Views.Count, (oglView, "3D Flat", OpenGLBackEnd.SetUpFlat));
			Views.Add(Views.Count, (oglView, "3D Textured", OpenGLBackEnd.SetUpTextured));

			UpdateViews();
		}
	}
}
