using Arbatel.Graphics;

namespace Arbatel.Controls
{
	public class OpenGLViewport : Viewport
	{
		public OpenGLViewport() : base(new OpenGL4BackEnd())
		{
			var oglView = new OpenGLView3d()
			{
				BackEnd = BackEnd,
				Enabled = false,
				Visible = false
			};

			Views.Add(Views.Count, (oglView, "3D Wireframe", OpenGLView.SetUpWireframe));
			Views.Add(Views.Count, (oglView, "3D Flat", OpenGLView.SetUpFlat));
			Views.Add(Views.Count, (oglView, "3D Textured", OpenGLView.SetUpTextured));

			UpdateViews();
		}
	}
}
