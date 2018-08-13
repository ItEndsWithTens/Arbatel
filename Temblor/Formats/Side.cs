using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Graphics;
using Temblor.Utilities;

namespace Temblor.Formats
{
	public class Solid
	{
		public List<Side> Sides;

		public Solid()
		{
			Sides = new List<Side>();
		}
		public Solid(List<Side> sides) : this()
		{
			Sides = sides;
		}
	}

	public class Side
	{
		public Plane Plane;

		public List<Vector3> TextureBasis;
		public string TextureName;
		public Vector2 TextureOffset;
		public float TextureRotation;
		public Vector2 TextureScale;

		public List<Vertex> Vertices;
		public List<int> Indices;

		public Side()
		{
			TextureBasis = new List<Vector3>();
			TextureOffset = new Vector2();
			TextureScale = new Vector2();

			Vertices = new List<Vertex>();
			Indices = new List<int>();
		}
	}
}
