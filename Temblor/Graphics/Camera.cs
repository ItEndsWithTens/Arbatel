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
	public class Frustum
	{
		public Vector3 NearTopLeft;
		public Vector3 NearTopRight;
		public Vector3 NearBottomLeft;
		public Vector3 NearBottomRight;

		public Vector3 FarTopLeft;
		public Vector3 FarTopRight;
		public Vector3 FarBottomLeft;
		public Vector3 FarBottomRight;

		public Frustum(Vector3 position, Vector3 front, Vector3 right, Vector3 up, float fov, float aspect, float near, float far)
		{
			NearTopLeft = new Vector3();
			NearTopRight = new Vector3();
			NearBottomLeft = new Vector3();
			NearBottomRight = new Vector3();

			FarTopLeft = new Vector3();
			FarTopRight = new Vector3();
			FarBottomLeft = new Vector3();
			FarBottomRight = new Vector3();

			Update(position, front, right, up, fov, aspect, near, far);
		}

		public bool Contains(Renderable r)
		{
			var contains = true;

			var rBounds = new AABB(r.Vertices);

			// Note the component swap! Not the swap of Y and Z, that's only due
			// to the difference in up axis between objects in world space and
			// the camera's coordinate system. The swap of Max.Y and Min.Y for
			// the new min/max, however, is thanks to switching left-handed 
			// coordinates to right-handed; that flips the sign of the camera's
			// lens axis, so the old min and max become the new max and min.
			var minCamera = new Vector3(rBounds.Min.X, rBounds.Min.Z, -rBounds.Max.Y);
			var maxCamera = new Vector3(rBounds.Max.X, rBounds.Max.Z, -rBounds.Min.Y);

			var thing = new List<Vector3>()
			{
				NearTopLeft,
				NearTopRight,
				NearBottomLeft,
				NearBottomRight,
				FarTopLeft,
				FarTopRight,
				FarBottomLeft,
				FarBottomRight
			};

			var frustumBounds = new AABB(thing);

			bool outsideX = maxCamera.X < frustumBounds.Min.X || minCamera.X > frustumBounds.Max.X;
			bool outsideY = maxCamera.Y < frustumBounds.Min.Y || minCamera.Y > frustumBounds.Max.Y;
			bool outsideZ = maxCamera.Z < frustumBounds.Min.Z || minCamera.Z > frustumBounds.Max.Z;

			if (outsideX || outsideY || outsideZ)
			{
				contains = false;
			}

			return contains;
		}

		public void Update(Vector3 position, Vector3 front, Vector3 right, Vector3 up, float fov, float aspect, float near, float far)
		{
			Vector3 nearTarget = position + (front * near);
			Vector3 farTarget = position + (front * far);

			float halfHorizontalAngle = fov / 2.0f;

			// The angle where the view direction hits the clip planes is 90
			// degrees, and a triangle's angles add up to 180.
			float remainingHorizontalAngle = 90.0f - halfHorizontalAngle;

			float nearHorizontalFactor = near / (float)Math.Sin(MathHelper.DegreesToRadians(remainingHorizontalAngle));
			float nearHalfWidth = (float)Math.Sin(MathHelper.DegreesToRadians(halfHorizontalAngle)) * nearHorizontalFactor;

			float farHorizontalFactor = far / (float)Math.Sin(MathHelper.DegreesToRadians(remainingHorizontalAngle));
			float farHalfWidth = (float)Math.Sin(MathHelper.DegreesToRadians(halfHorizontalAngle)) * farHorizontalFactor;

			
			float verticalFov = fov / aspect;

			float halfVerticalAngle = verticalFov / 2.0f;

			float remainingVerticalAngle = 90.0f - halfVerticalAngle;

			float nearVerticalFactor = near / (float)Math.Sin(MathHelper.DegreesToRadians(remainingVerticalAngle));
			float nearHalfHeight = (float)Math.Sin(MathHelper.DegreesToRadians(halfVerticalAngle)) * nearVerticalFactor;

			float farVerticalFactor = far / (float)Math.Sin(MathHelper.DegreesToRadians(remainingVerticalAngle));
			float farHalfHeight = (float)Math.Sin(MathHelper.DegreesToRadians(halfVerticalAngle)) * farVerticalFactor;

			Vector3 nearEdgeHorizontal = right * nearHalfWidth;
			Vector3 nearEdgeVertical = up * nearHalfHeight;

			NearTopLeft = (nearTarget - nearEdgeHorizontal) + nearEdgeVertical;
			NearTopRight = (nearTarget + nearEdgeHorizontal) + nearEdgeVertical;
			NearBottomLeft = (nearTarget - nearEdgeHorizontal) - nearEdgeVertical;
			NearBottomRight = (nearTarget + nearEdgeHorizontal) - nearEdgeVertical;

			Vector3 farEdgeHorizontal = right * farHalfWidth;
			Vector3 farEdgeVertical = up * farHalfHeight;

			FarTopLeft = (farTarget - farEdgeHorizontal) + farEdgeVertical;
			FarTopRight = (farTarget + farEdgeHorizontal) + farEdgeVertical;
			FarBottomLeft = (farTarget - farEdgeHorizontal) - farEdgeVertical;
			FarBottomRight = (farTarget + farEdgeHorizontal) - farEdgeVertical;
		}
	}

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

		public Frustum Frustum;

		public Camera()
		{
			AspectRatio = 16.0f / 9.0f;
			Fov = 95.0f;

			MaxPitch = 89.0f;
			MinPitch = -89.0f;

			NearClip = 1.0f;
			FarClip = 4096.0f;

			WorldUp = new Vector3(0.0f, 1.0f, 0.0f);
			Position = new Vector3(0.0f, 0.0f, 0.0f);

			Pitch = 0.0f;
			Yaw = -90.0f;
			Roll = 0.0f;

			Frustum = new Frustum(Position, Front, Right, Up, Fov, AspectRatio, NearClip, FarClip);
		}

		public bool CanSee(Renderable r)
		{
			//var canSee = true;

			//var yUpRightHand = new Vector3(r.Position.X, r.Position.Z, -r.Position.Y);

			//float distance = (yUpRightHand - Position).Length;

			// Simple radius culling for now.
			//if (distance > FarClip)
			//{
			//	canSee = false;
			//}

			//return canSee;

			return Frustum.Contains(r);
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
			ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov / AspectRatio), AspectRatio, NearClip, FarClip);

			if (Frustum != null)
			{
				Frustum.Update(Position, Front, Right, Up, Fov, AspectRatio, NearClip, FarClip);
			}
		}
	}
}
