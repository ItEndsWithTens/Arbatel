using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor
{
	public class Camera
	{
		public bool MouseLook = false;

		// Flightstick: false, true
		// Spotlight: true, true
		// Goldeneye: false, false
		public bool InvertMouseX = false;
		public bool InvertMouseY = false;

		public float MouseSensitivity = 0.25f;

		public float Speed = 0.05f;

		public Vector3 Position = new Vector3(0.0f, 0.0f, 3.0f);
		public Vector3 Target = new Vector3(0.0f, 0.0f, 0.0f);
		public Vector3 Front = new Vector3(0.0f, 0.0f, -1.0f);
		// TODO: Implement Z-up world coordinates! Y-up is for weirdos.
		public Vector3 WorldUp = new Vector3(0.0f, 1.0f, 0.0f);

		public Vector3 Direction;
		public Vector3 Right;
		public Vector3 Up;

		private float _pitch;
		public float Pitch
		{
			get { return _pitch; }
			set
			{
				_pitch = value;

				if (_pitch > 89.0f)
				{
					_pitch = 89.0f;
				}

				if (_pitch < -89.0f)
				{
					_pitch = -89.0f;
				}
			}
		}

		// TODO: Initialize these based on the starting camera orientation. -90.0f assumes the camera's
		// front vector is 0, 0, -1 in right-handed coordinates.
		private float _yaw = -90.0f;
		public float Yaw
		{
			get { return _yaw; }
			set
			{
				// Bring value into the range (-360.0, 360.0).
				var signedRange = value % 360.0f;

				// Bring signedRange into the range [0.0, 360.0).
				var unsignedRange = (signedRange + 360.0f) % 360.0f;

				_yaw = unsignedRange;
			}
		}

		public Camera()
		{
			Direction = Vector3.Normalize(Position - Target);
			Right = Vector3.Normalize(Vector3.Cross(WorldUp, Direction));
			Up = Vector3.Normalize(Vector3.Cross(Direction, Right));
		}

		public void Rotate()
		{
			var yawRad = MathHelper.DegreesToRadians(Yaw);
			var pitchRad = MathHelper.DegreesToRadians(Pitch);

			Front.X = Convert.ToSingle(Math.Cos(pitchRad) * Math.Cos(yawRad));
			Front.Y = Convert.ToSingle(Math.Sin(pitchRad));
			Front.Z = Convert.ToSingle(Math.Cos(pitchRad) * Math.Sin(yawRad));

			Front.Normalize();
		}

		// FIXME: Should this be called "Fly"? And have a separate method called "Translate" that accepts
		// any position to instantly move the camera?
		//
		// Yes, I think it should. Let's go with that for now.
		public void Fly(bool forward, bool backward, bool left, bool right, bool up, bool down)
		{
			if (forward)
			{
				Position += Front * Speed;
			}

			if (backward)
			{
				Position -= Front * Speed;
			}

			if (left)
			{
				Position -= Vector3.Normalize(Vector3.Cross(Front, Up)) * Speed;
			}

			if (right)
			{
				Position += Vector3.Normalize(Vector3.Cross(Front, Up)) * Speed;
			}

			if (up)
			{
				Position += Up * Speed;
			}

			if (down)
			{
				Position -= Up * Speed;
			}
		}
	}
}
