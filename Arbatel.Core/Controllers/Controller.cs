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
		public bool InvertMouseY { get; set; } = false;

		public float MouseSensitivity { get; set; } = 0.25f;

		public float Speed { get; set; } = 64.0f;

		protected bool Forward { get; set; } = false;
		protected bool Backward { get; set; } = false;
		protected bool Left { get; set; } = false;
		protected bool Right { get; set; } = false;
		protected bool Up { get; set; } = false;
		protected bool Down { get; set; } = false;

		virtual public void KeyDown(object sender, KeyEventArgs e)
		{
			// If a modifier is pressed, a view Controller isn't the intended
			// recipient of the key event.
			if (e.Modifiers != Keys.None)
			{
				return;
			}

			if (e.Key == Keys.Z)
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

					e.Handled = true;
				}
			}

			// Set each direction boolean true if the event represents the
			// appropriate key. The bitwise OR is less immediately clear than
			// the alternative set of if blocks, but makes the code cleaner.
			Forward |= e.Key == Keys.W;
			Backward |= e.Key == Keys.S;
			Left |= e.Key == Keys.A;
			Right |= e.Key == Keys.D;
			Up |= e.Key == Keys.E;
			Down |= e.Key == Keys.Q;

			e.Handled = true;
		}

		virtual public void KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Modifiers != Keys.None)
			{
				return;
			}

			Forward &= e.Key != Keys.W;
			Backward &= e.Key != Keys.S;
			Left &= e.Key != Keys.A;
			Right &= e.Key != Keys.D;
			Up &= e.Key != Keys.E;
			Down &= e.Key != Keys.Q;

			e.Handled = true;
		}

		virtual public void Update()
		{
		}

		virtual public void UpdateKeyboard()
		{
		}

		virtual public void UpdateMouse()
		{
		}
	}
}
