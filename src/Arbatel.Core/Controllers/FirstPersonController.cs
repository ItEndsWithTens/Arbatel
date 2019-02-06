using Arbatel.Graphics;
using OpenTK;

namespace Arbatel.Controllers
{
	public class FirstPersonController : Controller
	{
		public Camera Camera { get; set; }

		public FirstPersonController(Camera camera)
		{
			Camera = camera;

			Camera.MaxPitch = 89.0f;
			Camera.MinPitch = -89.0f;
		}

		public override void Update()
		{
			UpdateKeyboard();

			UpdateMouse();
		}

		public override void UpdateKeyboard()
		{
			Camera.TranslateRelative(Forward, Backward, Left, Right, Up, Down, Speed);
		}

		private OpenTK.Input.MouseState CurrentMouseState;
		private OpenTK.Input.MouseState PreviousMouseState;
		private Vector2 MouseDelta = new Vector2();

		public override void UpdateMouse()
		{
			CurrentMouseState = OpenTK.Input.Mouse.GetState();

			if (MouseLook)
			{
				if (CurrentMouseState != PreviousMouseState)
				{
					MouseDelta.X = CurrentMouseState.X - PreviousMouseState.X;
					MouseDelta.Y = CurrentMouseState.Y - PreviousMouseState.Y;

					MouseDelta *= MouseSensitivity;

					if (InvertMouseX)
					{
						Camera.Yaw -= MouseDelta.X;
					}
					else
					{
						Camera.Yaw += MouseDelta.X;
					}

					if (InvertMouseY)
					{
						Camera.Pitch += MouseDelta.Y;
					}
					else
					{
						Camera.Pitch -= MouseDelta.Y;
					}
				}
			}

			PreviousMouseState = CurrentMouseState;
		}
	}
}
