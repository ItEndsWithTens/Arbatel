using Eto;
using Eto.Forms;
using Eto.Gl;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Temblor.Controls
{
	public class View3d : View
	{
		private bool _forward = false;
		private bool _backward = false;
		private bool _left = false;
		private bool _right = false;
		private bool _up = false;
		private bool _down = false;

		public View3d()
		{
		}
	}
}
