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

		public Vector2 TexCoords;

		public Vertex(Vector3 _position) : this(_position.X, _position.Y, _position.Z)
		{
		}
		public Vertex(float _x, float _y, float _z) :
			this(new Vector3(_x, _y, _z), new Vector3(0.0f, 0.0f, 1.0f), Color4.White, new Vector2(0.0f, 0.0f))
		{
		}
		public Vertex(Vector3 _position, Vector3 _normal, Color4 _color, Vector2 _texcoords)
		{
			Position = _position;
			Normal = _normal;
			Color = _color;
			TexCoords = _texcoords;
		}

		public static Vector3 operator +(Vertex lhs, Vertex rhs)
		{
			return lhs.Position + rhs.Position;
		}
		public static Vector3 operator -(Vertex lhs, Vertex rhs)
		{
			return lhs.Position - rhs.Position;
		}

		public override string ToString()
		{
			return Position.ToString();
		}
	}
}
