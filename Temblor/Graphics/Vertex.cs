using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Graphics
{
	public struct Vertex
	{
		public Vector3 Position;

		public Vector3 Normal;

		public Color4 Color;

		public Vertex(Vertex _vertex) : this(_vertex.Position, _vertex.Normal, _vertex.Color)
		{
		}
		public Vertex(Vector3 _position) : this(_position.X, _position.Y, _position.Z)
		{
		}
		public Vertex(float _x, float _y, float _z) :
			this(new Vector3(_x, _y, _z), new Vector3(0.0f, 0.0f, 1.0f), Color4.White)
		{
		}
		public Vertex(float _x, float _y, float _z, Color4 _color) :
			this(new Vector3(_x, _y, _z), new Vector3(0.0f, 0.0f, 1.0f), _color)
		{
		}
		public Vertex(Vector3 _position, Color4 _color) :
			this(_position, new Vector3(0.0f, 0.0f, 1.0f), _color)
		{
		}
		public Vertex(Vector3 _position, Vector3 _normal, Color4 _color)
		{
			Position = _position;
			Normal = _normal;
			Color = _color;
		}

		public static Vertex Rotate(Vertex vertex, float pitch, float yaw, float roll)
		{
			if (pitch < 0.0f)
			{
				pitch = Math.Abs(pitch);
			}
			else
			{
				pitch = 360.0f - pitch;
			}

			// Assumes that objects are  pointing toward +X; thereby pitch
			// represents rotation around world Y (camera Z), yaw is world
			// Z (camera Y), and roll is world/camera X.
			Matrix4 rotZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(pitch));
			Matrix4 rotY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(yaw));
			Matrix4 rotX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(roll));

			Matrix4 rotation = rotZ * rotY * rotX;

			var v = new Vertex(vertex);

			var yUpRightHand = new Vector4(v.Position.X, v.Position.Z, -v.Position.Y, 1.0f);
			Vector4 rotated = yUpRightHand * rotation;
			var zUpLeftHand = new Vector3(rotated.X, -rotated.Z, rotated.Y);
			v.Position = zUpLeftHand;

			yUpRightHand = new Vector4(v.Normal.X, v.Normal.Z, -v.Normal.Y, 1.0f);
			rotated = yUpRightHand * rotation;
			zUpLeftHand = new Vector3(rotated.X, -rotated.Z, rotated.Y);
			v.Normal = zUpLeftHand;

			return v;
		}

		public static Vertex TranslateRelative(Vertex v, Vector3 diff)
		{
			return new Vertex(v)
			{
				Position = new Vector3(v.Position + diff)
			};
		}
		public static Vertex TranslateRelative(Vertex v, float diffX, float diffY, float diffZ)
		{
			return TranslateRelative(v, new Vector3(diffX, diffY, diffZ));
		}

		public static Vector3 operator +(Vertex lhs, Vertex rhs)
		{
			return lhs.Position + rhs.Position;
		}
		public static Vector3 operator -(Vertex lhs, Vertex rhs)
		{
			return lhs.Position - rhs.Position;
		}

		public static implicit operator Vector3(Vertex vertex)
		{
			return new Vector3(vertex.Position);
		}

		public override string ToString()
		{
			return Position.ToString();
		}
	}
}
