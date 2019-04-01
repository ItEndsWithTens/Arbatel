using Arbatel.Controllers;

namespace Arbatel.Controls
{
	public class VeldridView3d : VeldridView
	{
		public VeldridView3d()
		{
			Controller = new FirstPersonController(Camera);

			Fps = 60.0f;
		}
	}
}
