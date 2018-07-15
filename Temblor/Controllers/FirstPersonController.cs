using Eto.Drawing;
using Eto.Forms;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Controls;
using Temblor.Graphics;

namespace Temblor.Controllers
{
	public class FirstPersonController : Controller
	{
		public Camera Camera;

		public FirstPersonController(ref Camera _camera)
		{
			Camera = _camera;

			Camera.MaxPitch = 89.0f;
			Camera.MinPitch = -89.0f;
		}

		public override void Move()
		{
			if (_forward)
			{
				// Remember that OpenGL uses right-handed coordinates.
				Camera.Position += Speed * Camera.Front;
			}
			else if (_backward)
			{
				Camera.Position -= Speed * Camera.Front;
			}

			if (_left)
			{
				Camera.Position -= Speed * Camera.Right;
			}
			else if (_right)
			{
				Camera.Position += Speed * Camera.Right;
			}

			if (_up)
			{
				Camera.Position += Speed * Camera.WorldUp;
			}
			else if (_down)
			{
				Camera.Position -= Speed * Camera.WorldUp;
			}
		}

		public override void MouseMove(object sender, MouseEventArgs e)
		{
			if (MouseLook)
			{
				var view = sender as View;

				var centerScreen = new Point(view.PointToScreen(view.Bounds.Center));
				var locationScreen = view.PointToScreen(e.Location);

				var delta = (locationScreen - centerScreen) * MouseSensitivity;

				if (delta.X == 0.0f && delta.Y == 0.0f)
				{
					return;
				}

				if (InvertMouseX)
				{
					Camera.Yaw -= delta.X;
				}
				else
				{
					Camera.Yaw += delta.X;
				}

				if (InvertMouseY)
				{
					Camera.Pitch += delta.Y;
				}
				else
				{
					Camera.Pitch -= delta.Y;
				}

				OpenTK.Input.Mouse.SetPosition(centerScreen.X, centerScreen.Y);
			}
		}
	}
}
