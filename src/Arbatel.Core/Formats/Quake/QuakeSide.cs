using Arbatel.Graphics;
using Arbatel.Utilities;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arbatel.Formats.Quake
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
		public QuakeSide(string raw)
		{
			var split = raw.Split(' ', '\t').ToList();
			split.RemoveAll(s => String.IsNullOrEmpty(s.Trim()));

			var pointA = split.GetRange(1, 3).ToVector3();
			var pointB = split.GetRange(6, 3).ToVector3();
			var pointC = split.GetRange(11, 3).ToVector3();

			var planePointA = new Vertex { Position = pointA };
			var planePointB = new Vertex { Position = pointB };
			var planePointC = new Vertex { Position = pointC };

			Plane = new Plane(planePointA, planePointB, planePointC, Winding.Cw);

			TextureName = split[15];

			var basisS = new Vector3();
			var basisT = new Vector3();

			// Valve 220
			if (split.Contains("["))
			{
				basisS = split.GetRange(17, 3).ToVector3();
				Single.TryParse(split[20], out TextureOffset.X);

				basisT = split.GetRange(23, 3).ToVector3();
				Single.TryParse(split[26], out TextureOffset.Y);

				Single.TryParse(split[28], out TextureRotation);

				Single.TryParse(split[29], out TextureScale.X);
				Single.TryParse(split[30], out TextureScale.Y);
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

				basisS = closestPlane.Points[1].Position;
				Single.TryParse(split[16], out TextureOffset.X);

				// Positive T runs downward from the texture coordinate system's
				// origin, as opposed to the world coordinates, where +Z is up.
				basisT = -closestPlane.Points[2].Position;
				Single.TryParse(split[17], out TextureOffset.Y);

				Single.TryParse(split[18], out TextureRotation);

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

				Single.TryParse(split[19], out TextureScale.X);
				Single.TryParse(split[20], out TextureScale.Y);
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

			foreach (Vertex point in Plane.Points)
			{
				sb.Append("( ");

				for (int i = 0; i < 3; i++)
				{
					float coord = point.Position[i];
					sb.Append(coord + " ");
				}

				sb.Append(") ");
			}

			sb.Append(TextureName + " ");

			if (format == QuakeSideFormat.Valve220)
			{
				for (int i = 0; i < 2; i++)
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
