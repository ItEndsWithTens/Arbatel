using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Temblor.Graphics;
using Temblor.Utilities;

namespace Temblor.Formats.Quake
{
	public enum QuakeSideFormat
	{
		QuakeEd,
		Valve220
	}

	public class QuakeSide : Side
	{
		public QuakeSide()
		{
		}
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

			var basisS = new Vector3();
			var basisT = new Vector3();

			// Valve 220
			if (split.Contains("["))
			{
				float.TryParse(split[17], out basisS.X);
				float.TryParse(split[18], out basisS.Y);
				float.TryParse(split[19], out basisS.Z);
				float.TryParse(split[20], out TextureOffset.X);

				float.TryParse(split[23], out basisT.X);
				float.TryParse(split[24], out basisT.Y);
				float.TryParse(split[25], out basisT.Z);
				float.TryParse(split[26], out TextureOffset.Y);

				float.TryParse(split[28], out TextureRotation);

				float.TryParse(split[29], out TextureScale.X);
				float.TryParse(split[30], out TextureScale.Y);
			}
			// QuakeEd
			else
			{
				var cardinals = new List<Plane>()
				{
					new Plane(new Vector3(0, 0, 0), new Vector3(1, 0 ,0), new Vector3(0, 1, 0), Winding.Cw),
					new Plane(new Vector3(0, 0, 0), new Vector3(1, 0 ,0), new Vector3(0, 1, 0), Winding.Ccw),
					new Plane(new Vector3(0, 0, 0), new Vector3(0, 1 ,0), new Vector3(0, 0, 1), Winding.Cw),
					new Plane(new Vector3(0, 0, 0), new Vector3(0, 1 ,0), new Vector3(0, 0, 1), Winding.Ccw),
					new Plane(new Vector3(0, 0, 0), new Vector3(1, 0 ,0), new Vector3(0, 0, 1), Winding.Cw),
					new Plane(new Vector3(0, 0, 0), new Vector3(1, 0 ,0), new Vector3(0, 0, 1), Winding.Ccw)
				};

				float smallestAngle = 180.0f;
				Plane closestPlane = cardinals[0];
				foreach (Plane cardinal in cardinals)
				{
					float angle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(Plane.Normal, cardinal.Normal));
					if (angle < smallestAngle)
					{
						closestPlane = cardinal;
						smallestAngle = angle;
					}
				}

				basisS.X = closestPlane.Points[1].Position.X;
				basisS.Y = closestPlane.Points[1].Position.Y;
				basisS.Z = closestPlane.Points[1].Position.Z;
				float.TryParse(split[16], out TextureOffset.X);

				// Positive T runs downward from the texture coordinate system's
				// origin, as opposed to the world coordinates, where +Z is up.
				basisT.X = -closestPlane.Points[2].Position.X;
				basisT.Y = -closestPlane.Points[2].Position.Y;
				basisT.Z = -closestPlane.Points[2].Position.Z;
				float.TryParse(split[17], out TextureOffset.Y);

				float.TryParse(split[18], out TextureRotation);

				Matrix3 matrix;
				if (Math.Abs(closestPlane.Normal.X) == 1.0f)
				{
					Matrix3.CreateRotationX(MathHelper.DegreesToRadians(TextureRotation), out matrix);
				}
				else if (Math.Abs(closestPlane.Normal.Y) == 1.0f)
				{
					// The CreateRotation methods expect the counter-clockwise
					// angle of rotation, but don't ask for a normal, so it's
					// the user's reponsibility to figure that out. OpenTK also
					// uses right-handed Z-up coordinates, apparently, so it's
					// only necessary to subtract from 360 for this axis.
					Matrix3.CreateRotationY(MathHelper.DegreesToRadians(360.0f - TextureRotation), out matrix);
				}
				else
				{
					Matrix3.CreateRotationZ(MathHelper.DegreesToRadians(TextureRotation), out matrix);
				}

				basisS *= matrix;
				basisT *= matrix;

				float.TryParse(split[19], out TextureScale.X);
				float.TryParse(split[20], out TextureScale.Y);
			}

			TextureBasis.Add(basisS);
			TextureBasis.Add(basisT);
		}

		public override string ToString()
		{
			return ToString(QuakeSideFormat.Valve220);
		}
		public string ToString(QuakeSideFormat format)
		{
			var sb = new StringBuilder();

			foreach (var point in Plane.Points)
			{
				sb.Append("(");

				for (int i = 0; i < 3; i++)
				{
					float coord = point.Position[i];
					sb.Append(" " + coord + " ");
				}

				sb.Append(") ");
			}

			sb.Append(TextureName + " ");

			if (format == QuakeSideFormat.Valve220)
			{
				for (var i = 0; i < 2; i++)
				{
					Vector3 vec = TextureBasis[i];

					sb.Append("[ ");
					sb.Append(vec.X + " " + vec.Y + " " + vec.Z + " " + TextureOffset[i]);
					sb.Append(" ] ");
				}
			}
			else
			{
				sb.Append(TextureOffset.X + " ");
				sb.Append(TextureOffset.Y + " ");
			}

			sb.Append(TextureRotation + " ");
			sb.Append(TextureScale.X + " ");
			sb.Append(TextureScale.Y);

			return sb.ToString();
		}
	}
}
