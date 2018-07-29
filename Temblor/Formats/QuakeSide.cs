using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Temblor.Graphics;
using Temblor.Utilities;

namespace Temblor.Formats
{
	public class QuakeSide : Side
	{
		public QuakeSide(string _raw)
		{
			// Regex seems a bit heavy to drag into this, but in principle this
			// should be able to handle tabs and other whitespace. This approach
			// wouldn't work for key/val lines, since it might remove desirable
			// whitespace found in values, but for sides it's perfect.
			var split = Regex.Split(_raw, @"\s");

			var planePointA = new Vertex();
			var planePointB = new Vertex();
			var planePointC = new Vertex();

			float.TryParse(split[1], out planePointA.Position.X);
			float.TryParse(split[2], out planePointA.Position.Y);
			float.TryParse(split[3], out planePointA.Position.Z);

			float.TryParse(split[6], out planePointB.Position.X);
			float.TryParse(split[7], out planePointB.Position.Y);
			float.TryParse(split[8], out planePointB.Position.Z);

			float.TryParse(split[11], out planePointC.Position.X);
			float.TryParse(split[12], out planePointC.Position.Y);
			float.TryParse(split[13], out planePointC.Position.Z);

			Plane = new Plane(planePointA, planePointB, planePointC, Winding.Cw);

			TextureName = split[15];

			var basisU = new Vector3();
			var basisV = new Vector3();

			// Valve 220
			if (split.Contains("["))
			{
				float.TryParse(split[17], out basisU.X);
				float.TryParse(split[18], out basisU.Y);
				float.TryParse(split[19], out basisU.Z);
				float.TryParse(split[20], out TextureOffset.X);

				float.TryParse(split[23], out basisV.X);
				float.TryParse(split[24], out basisV.Y);
				float.TryParse(split[25], out basisV.Z);
				float.TryParse(split[26], out TextureOffset.Y);

				float.TryParse(split[28], out TextureRotation);

				float.TryParse(split[29], out TextureScale.X);
				float.TryParse(split[30], out TextureScale.Y);
			}
			// QuakeEd
			else
			{
				// TODO: Find the closest matching cardinal plane and use that
				// instead; that's how the real thing works. See Quinstance for
				// sample code to do what I hope is the right thing.
				basisU.X = 1.0f;
				basisU.Y = 0.0f;
				basisU.Z = 0.0f;
				float.TryParse(split[16], out TextureOffset.X);

				basisV.X = 0.0f;
				basisV.Y = 1.0f;
				basisV.Z = 0.0f;
				float.TryParse(split[17], out TextureOffset.Y);

				float.TryParse(split[18], out TextureRotation);

				float.TryParse(split[19], out TextureScale.X);
				float.TryParse(split[20], out TextureScale.Y);
			}

			TextureBasis.Add(basisU);
			TextureBasis.Add(basisV);
		}
	}
}
