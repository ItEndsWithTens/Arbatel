using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Formats
{
	public class Side
	{
		public List<Vector3> Plane;

		public List<Vector3> TextureBasis;
		public string TextureName;
		public Vector2 TextureOffset;
		public float TextureRotation;
		public Vector2 TextureScale;

		public Side()
		{
			Plane = new List<Vector3>();

			TextureBasis = new List<Vector3>();
			TextureOffset = new Vector2();
			TextureScale = new Vector2();
		}
	}
}
