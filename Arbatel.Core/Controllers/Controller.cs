using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arbatel.Controls;

namespace Arbatel.Controllers
{
	public class Controller
	{
		public bool MouseLook { get; set; } = false;

		// Flightstick: false, true
		// Spotlight: true, true
		// Goldeneye: false, false
		public bool InvertMouseX { get; set; } = false;
		public bool InvertMouseY { get; set; } = true;

		public float MouseSensitivity { get; set; } = 0.25f;

		public float Speed { get; set; } = 64.0f;

		protected bool Forward { get; set; } = false;
		protected bool Backward { get; set; } = false;
		protected bool Left { get; set; } = false;
		protected bool Right { get; set; } = false;
		protected bool Up { get; set; } = false;
		protected bool Down { get; set; } = false;

		virtual public void KeyEvent(object sender, KeyEventArgs e)
		{
			if (e.Modifiers == Keys.None)
			{
				if (e.IsKeyDown(Keys.Z))
				{
					if (sender is View v)
					{
						if (MouseLook == false)
						{
							MouseLook = true;
							v.Style = "hidecursor";
						}
						else
						{
							MouseLook = false;
							v.Style = "showcursor";
						}
					}
				}

				if (e.IsKeyDown(Keys.W))
				{
					Forward = true;
				}
				else if (e.IsKeyUp(Keys.W))
				{
					Forward = false;
				}

				if (e.IsKeyDown(Keys.S))
				{
					Backward = true;
				}
				else if (e.IsKeyUp(Keys.S))
				{
					Backward = false;
				}

				if (e.IsKeyDown(Keys.A))
				{
					Left = true;
				}
				else if (e.IsKeyUp(Keys.A))
				{
					Left = false;
				}

				if (e.IsKeyDown(Keys.D))
				{
					Right = true;
				}
				else if (e.IsKeyUp(Keys.D))
				{
					Right = false;
				}

				if (e.IsKeyDown(Keys.E))
				{
					Up = true;
				}
				else if (e.IsKeyUp(Keys.E))
				{
					Up = false;
				}

				if (e.IsKeyDown(Keys.Q))
				{
					Down = true;
				}
				else if (e.IsKeyUp(Keys.Q))
				{
					Down = false;
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
