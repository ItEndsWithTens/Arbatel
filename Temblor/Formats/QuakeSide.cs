using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Formats
{
	public class QuakeSide : Side
	{
		public QuakeSide(string _raw)
		{
			var split = _raw.Split(' ');

			var planePointA = new Vector3();
			var planePointB = new Vector3();
			var planePointC = new Vector3();

			float.TryParse(split[1], out planePointA.X);
			float.TryParse(split[2], out planePointA.Y);
			float.TryParse(split[3], out planePointA.Z);

			float.TryParse(split[6], out planePointB.X);
			float.TryParse(split[7], out planePointB.Y);
			float.TryParse(split[8], out planePointB.Z);

			float.TryParse(split[11], out planePointC.X);
			float.TryParse(split[12], out planePointC.Y);
			float.TryParse(split[13], out planePointC.Z);

			Plane.Add(planePointA);
			Plane.Add(planePointB);
			Plane.Add(planePointC);

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
