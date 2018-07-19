using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Controls;

namespace Temblor.Controllers
{
	public class Controller
	{
		public bool MouseLook = false;

		// Flightstick: false, true
		// Spotlight: true, true
		// Goldeneye: false, false
		public bool InvertMouseX = false;
		public bool InvertMouseY = false;

		public float MouseSensitivity = 0.25f;

		public float Speed = 64.0f;

		protected bool _forward = false;
		protected bool _backward = false;
		protected bool _left = false;
		protected bool _right = false;
		protected bool _up = false;
		protected bool _down = false;

		virtual public void KeyEvent(object sender, KeyEventArgs e)
		{
			if (e.Modifiers == Keys.None)
			{
				if (e.IsKeyDown(Keys.Z))
				{
					if (MouseLook == false)
					{
						MouseLook = true;
						if (sender is View)
						{
							(sender as View).Style = "hidecursor";
						}
					}
					else
					{
						MouseLook = false;
						if (sender is View)
						{
							(sender as View).Style = "showcursor";
						}
					}
				}

				if (e.IsKeyDown(Keys.W))
				{
					_forward = true;
				}
				else if (e.IsKeyUp(Keys.W))
				{
					_forward = false;
				}

				if (e.IsKeyDown(Keys.S))
				{
					_backward = true;
				}
				else if (e.IsKeyUp(Keys.S))
				{
					_backward = false;
				}

				if (e.IsKeyDown(Keys.A))
				{
					_left = true;
				}
				else if (e.IsKeyUp(Keys.A))
				{
					_left = false;
				}

				if (e.IsKeyDown(Keys.D))
				{
					_right = true;
				}
				else if (e.IsKeyUp(Keys.D))
				{
					_right = false;
				}

				if (e.IsKeyDown(Keys.E))
				{
					_up = true;
				}
				else if (e.IsKeyUp(Keys.E))
				{
					_up = false;
				}

				if (e.IsKeyDown(Keys.Q))
				{
					_down = true;
				}
				else if (e.IsKeyUp(Keys.Q))
				{
					_down = false;
				}
			}
		}

		virtual public void MouseMove(object sender, MouseEventArgs e)
		{
		}

		virtual public void Move()
		{
		}
	}
}
