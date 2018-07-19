using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Utilities;

namespace Temblor.Graphics
{
	public class Camera
	{
		public float AspectRatio;
		public float Fov;

		private float _maxPitch;
		public float MaxPitch
		{
			get { return _maxPitch; }
			set { _maxPitch = MathUtilities.ModAngleToCircleSigned(value); }
		}

		private float _minPitch;
		public float MinPitch
		{
			get { return _minPitch; }
			set { _minPitch = MathUtilities.ModAngleToCircleSigned(value); }
		}

		public float NearClip;
		public float FarClip;

		private Vector3 _position;
		public Vector3 Position
		{
			get { return _position; }
			set
			{
				_position = value;
				Update();
			}
		}

		public Vector3 WorldUp;

		public Vector3 Front;
		public Vector3 Right;
		public Vector3 Up;

		public Matrix4 ViewMatrix;
		public Matrix4 ProjectionMatrix;

		private float _pitch;
		public float Pitch
		{
			get { return _pitch; }
			set
			{
				_pitch = MathUtilities.ModAngleToCircleSigned(value);

				if (_pitch > MaxPitch)
				{
					_pitch = MaxPitch;
				}
				else if (_pitch < MinPitch)
				{
					_pitch = MinPitch;
				}

				Update();
			}
		}

		private float _yaw;
		public float Yaw
		{
			get { return _yaw; }
			set
			{
				_yaw = MathUtilities.ModAngleToCircleUnsigned(value);

				Update();
			}
		}

		private float _roll;
		public float Roll
		{
			get { return _roll; }
			set
			{
				_roll = MathUtilities.ModAngleToCircleSigned(value);

				Update();
			}
		}

		public Camera()
		{
			AspectRatio = 16.0f / 9.0f;
			Fov = 75.0f;

			MaxPitch = 89.0f;
			MinPitch = -89.0f;

			NearClip = 0.001f;
			FarClip = 10000.0f;

			WorldUp = new Vector3(0.0f, 1.0f, 0.0f);
			Position = new Vector3(0.0f, 0.0f, 0.0f);

			Pitch = 0.0f;
			Yaw = -90.0f;
			Roll = 0.0f;
		}

		public void Update()
		{
			// TODO: Hook up Roll. Not critical, but nice to have.
			var yawRad = MathHelper.DegreesToRadians(Yaw);
			var pitchRad = MathHelper.DegreesToRadians(Pitch);

			Front.X = Convert.ToSingle(Math.Cos(pitchRad) * Math.Cos(yawRad));
			Front.Y = Convert.ToSingle(Math.Sin(pitchRad));
			Front.Z = Convert.ToSingle(Math.Cos(pitchRad) * Math.Sin(yawRad));

			Front.Normalize();

			Right = Vector3.Normalize(Vector3.Cross(Front, WorldUp));
			Up = Vector3.Normalize(Vector3.Cross(Right, Front));

			ViewMatrix = Matrix4.LookAt(Position, Position + Front, Up);
			ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), AspectRatio, NearClip, FarClip);
		}
	}
}
