using Arbatel.Controllers;

namespace Arbatel.Controls
{
	public class OpenGLView3d : OpenGLView
	{
		public OpenGLView3d()
		{
			Controller = new FirstPersonController(Camera);

			Fps = 60.0f;
		}
	}
}
