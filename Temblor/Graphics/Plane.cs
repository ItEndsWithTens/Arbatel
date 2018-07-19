using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Graphics
{
	public class Plane
	{
		public readonly Vertex A;
		public readonly Vertex B;
		public readonly Vertex C;

		public readonly Vector3 Normal;

		public readonly float DistanceFromOrigin;

		public Plane(Vector3 _a, Vector3 _b, Vector3 _c) : this(new Vertex(_a), new Vertex(_b), new Vertex(_c))
		{
		}
		public Plane(Vertex _a, Vertex _b, Vertex _c)
		{
			A = _a;
			B = _b;
			C = _c;

			Normal = Vector3.Cross(B - A, C - A);
			Normal.Normalize();

			DistanceFromOrigin = (-Vector3.Dot(Normal, A.Position)) / Normal.Length;
		}

		public static Vector3 Intersect(Plane a, Plane b, Plane c)
		{
			var intersection = new Vector3();

			var denominator = Vector3.Dot(a.Normal, Vector3.Cross(b.Normal, c.Normal));

			// Planes do not intersect.
			if (MathHelper.ApproximatelyEqualEpsilon(denominator, 0.0f, 0.0001f))
			{
				return new Vector3(float.NaN, float.NaN, float.NaN);
			}

			var crossAB = Vector3.Cross(a.Normal, b.Normal);
			crossAB *= c.DistanceFromOrigin;

			var crossBC = Vector3.Cross(b.Normal, c.Normal);
			crossBC *= a.DistanceFromOrigin;

			var crossCA = Vector3.Cross(c.Normal, a.Normal);
			crossCA *= b.DistanceFromOrigin;

			intersection = -crossBC - crossCA - crossAB;

			return intersection / denominator;
		}
	}
}
