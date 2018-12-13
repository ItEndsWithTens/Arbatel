using Eto;
using Eto.Forms;
using Eto.Gl;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using Arbatel.Controllers;

namespace Arbatel.Controls
{
	public class View3d : View
	{
		public View3d()
		{
			Controller = new FirstPersonController(ref Camera);
		}
	}
}
