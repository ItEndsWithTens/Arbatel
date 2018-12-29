using Arbatel.Graphics;
using NUnit.Framework;
using OpenTK;
using System;
using System.Collections.Generic;

namespace ArbatelTest.Core.GraphicsTest
{
	public class GraphicsTest
	{
		public const float Tolerance = 0.01f;

		public class PlaneTest
		{
			[TestFixture]
			public class DistanceTest
			{
				[TestCase]
				public void PositiveDistanceFacingAwayFromOriginCw()
				{
					var a = new Vector3(256, 0, 0);
					var b = new Vector3(256, 0, 256);
					var c = new Vector3(256, 256, 0);
					var plane = new Plane(a, b, c, Winding.Cw);

					Assert.That(plane.Normal.X, Is.EqualTo(1).Within(Tolerance));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.DistanceFromOrigin, Is.EqualTo(256).Within(Tolerance));
				}

				[TestCase]
				public void PositiveDistanceFacingAwayFromOriginCcw()
				{
					var a = new Vector3(256, 256, 0);
					var b = new Vector3(256, 0, 256);
					var c = new Vector3(256, 0, 0);
					var plane = new Plane(a, b, c, Winding.Ccw);

					Assert.That(plane.Normal.X, Is.EqualTo(1).Within(Tolerance));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.DistanceFromOrigin, Is.EqualTo(256).Within(Tolerance));
				}

				// When facing toward the origin, distance will always be
				// negative; from (0, 0, 0), facing along the plane's normal,
				// one must move X relative units to reach the plane.
				[TestCase]
				public void PositiveDistanceFacingTowardOriginCw()
				{
					var a = new Vector3(256, 0, 0);
					var b = new Vector3(256, 256, 0);
					var c = new Vector3(256, 0, 256);
					var plane = new Plane(a, b, c, Winding.Cw);

					Assert.That(plane.Normal.X, Is.EqualTo(-1).Within(Tolerance));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.DistanceFromOrigin, Is.EqualTo(-256).Within(Tolerance));
				}

				[TestCase]
				public void PositiveDistanceFacingTowardOriginCcw()
				{
					var a = new Vector3(256, 256, 0);
					var b = new Vector3(256, 0, 0);
					var c = new Vector3(256, 0, 256);
					var plane = new Plane(a, b, c, Winding.Ccw);

					Assert.That(plane.Normal.X, Is.EqualTo(-1).Within(Tolerance));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.DistanceFromOrigin, Is.EqualTo(-256).Within(Tolerance));
				}

				[TestCase]
				public void NegativeDistanceFacingAwayFromOriginCw()
				{
					var a = new Vector3(-256, 256, 0);
					var b = new Vector3(-256, 0, 256);
					var c = new Vector3(-256, 0, 0);
					var plane = new Plane(a, b, c, Winding.Cw);

					Assert.That(plane.Normal.X, Is.EqualTo(-1).Within(Tolerance));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.DistanceFromOrigin, Is.EqualTo(256).Within(Tolerance));
				}

				[TestCase]
				public void NegativeDistanceFacingAwayFromOriginCcw()
				{
					var a = new Vector3(-256, 0, 0);
					var b = new Vector3(-256, 0, 256);
					var c = new Vector3(-256, 256, 0);
					var plane = new Plane(a, b, c, Winding.Ccw);

					Assert.That(plane.Normal.X, Is.EqualTo(-1).Within(Tolerance));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.DistanceFromOrigin, Is.EqualTo(256).Within(Tolerance));
				}
			}

			[TestFixture]
			public class IntersectionTest
			{
				// With apologies to Bruce Dawson.
				private const float Tolerance = 0.02f;

				[TestCase]
				public void PlanesDoNotIntersect()
				{
					var a = new Vector3(0.0f, 0.0f, 0.0f);
					var b = new Vector3(0.0f, 32.0f, 0.0f);
					var c = new Vector3(32.0f, 0.0f, 0.0f);
					var planeA = new Plane(a, b, c, Winding.Cw);

					a = new Vector3(0.0f, 0.0f, 16.0f);
					b = new Vector3(0.0f, 32.0f, 16.0f);
					c = new Vector3(32.0f, 32.0f, 16.0f);
					var planeB = new Plane(a, b, c, Winding.Cw);

					a = new Vector3(0.0f, 0.0f, 64.0f);
					b = new Vector3(0.0f, 32.0f, 64.0f);
					c = new Vector3(32.0f, 32.0f, 64.0f);
					var planeC = new Plane(a, b, c, Winding.Cw);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(Single.NaN));
					Assert.That(intersection.Y, Is.EqualTo(Single.NaN));
					Assert.That(intersection.Z, Is.EqualTo(Single.NaN));
				}

				[TestCase]
				public void PlanesIntersectAtOriginCw()
				{
					var a = new Vector3(0.0f, 0.0f, 0.0f);
					var b = new Vector3(0.0f, 32.0f, 0.0f);
					var c = new Vector3(32.0f, 0.0f, 0.0f);
					var planeA = new Plane(a, b, c, Winding.Cw);

					a = new Vector3(0.0f, 0.0f, 0.0f);
					b = new Vector3(32.0f, 0.0f, 0.0f);
					c = new Vector3(0.0f, 0.0f, 32.0f);
					var planeB = new Plane(a, b, c, Winding.Cw);

					a = new Vector3(0.0f, 0.0f, 0.0f);
					b = new Vector3(0.0f, 0.0f, 32.0f);
					c = new Vector3(0.0f, 32.0f, 0.0f);
					var planeC = new Plane(a, b, c, Winding.Cw);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(intersection.Y, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(intersection.Z, Is.EqualTo(0.0f).Within(Tolerance));
				}

				[TestCase]
				public void PlanesIntersectAtOriginCcw()
				{
					var a = new Vector3(0.0f, 0.0f, 0.0f);
					var b = new Vector3(0.0f, 32.0f, 0.0f);
					var c = new Vector3(32.0f, 0.0f, 0.0f);
					var planeA = new Plane(a, b, c, Winding.Ccw);

					a = new Vector3(0.0f, 0.0f, 0.0f);
					b = new Vector3(32.0f, 0.0f, 0.0f);
					c = new Vector3(0.0f, 0.0f, 32.0f);
					var planeB = new Plane(a, b, c, Winding.Ccw);

					a = new Vector3(0.0f, 0.0f, 0.0f);
					b = new Vector3(0.0f, 0.0f, 32.0f);
					c = new Vector3(0.0f, 32.0f, 0.0f);
					var planeC = new Plane(a, b, c, Winding.Ccw);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(intersection.Y, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(intersection.Z, Is.EqualTo(0.0f).Within(Tolerance));
				}

				[TestCase]
				public void PlanesIntersectCw()
				{
					var a = new Vector3(-256.0f, 512.0f, 512.0f);
					var b = new Vector3(-256.0f, 512.0f, 0.0f);
					var c = new Vector3(-256.0f, 0.0f, 0.0f);
					var planeA = new Plane(a, b, c, Winding.Cw);

					a = new Vector3(-768.0f, 512.0f, 512.0f);
					b = new Vector3(-256.0f, 512.0f, 512.0f);
					c = new Vector3(-256.0f, 0.0f, 512.0f);
					var planeB = new Plane(a, b, c, Winding.Cw);

					a = new Vector3(-256.0f, 0.0f, 512.0f);
					b = new Vector3(-768.0f, 0.0f, 0.0f);
					c = new Vector3(-768.0f, 0.0f, 512.0f);
					var planeC = new Plane(a, b, c, Winding.Cw);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(-256.0f).Within(Tolerance));
					Assert.That(intersection.Y, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(intersection.Z, Is.EqualTo(512.0f).Within(Tolerance));
				}

				[TestCase]
				public void PlanesIntersectCcw()
				{
					var a = new Vector3(-256.0f, 0.0f, 0.0f);
					var b = new Vector3(-256.0f, 512.0f, 0.0f);
					var c = new Vector3(-256.0f, 512.0f, 512.0f);
					var planeA = new Plane(a, b, c, Winding.Ccw);

					a = new Vector3(-256.0f, 0.0f, 512.0f);
					b = new Vector3(-256.0f, 512.0f, 512.0f);
					c = new Vector3(-768.0f, 512.0f, 512.0f);
					var planeB = new Plane(a, b, c, Winding.Ccw);

					a = new Vector3(-768.0f, 0.0f, 512.0f);
					b = new Vector3(-768.0f, 0.0f, 0.0f);
					c = new Vector3(-256.0f, 0.0f, 512.0f);
					var planeC = new Plane(a, b, c, Winding.Ccw);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(-256.0f).Within(Tolerance));
					Assert.That(intersection.Y, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(intersection.Z, Is.EqualTo(512.0f).Within(Tolerance));
				}

				[TestCase]
				public void PlanesIntersectPyramidCw()
				{
					var a = new Vector3(0, 0, 512);
					var b = new Vector3(256, 256, 0);
					var c = new Vector3(256, -256, 0);
					var planeA = new Plane(a, b, c, Winding.Cw);

					a = new Vector3(0, 0, 512);
					b = new Vector3(256, -256, 0);
					c = new Vector3(-256, -256, 0);
					var planeB = new Plane(a, b, c, Winding.Cw);

					a = new Vector3(0, 0, 512);
					b = new Vector3(-256, 256, 0);
					c = new Vector3(256, 256, 0);
					var planeC = new Plane(a, b, c, Winding.Cw);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(intersection.Y, Is.EqualTo(0.0f).Within(Tolerance));
					Assert.That(intersection.Z, Is.EqualTo(512.0f).Within(Tolerance));
				}

				[TestCase]
				public void ShallowTetrahedronAllCombosIntersect()
				{
					// Sample plane points taken from a thin rock face brush in
					// xmasjam_tens.map, "Mae'hu Maechrimma" from Xmas Jam 2017.

					var a = new Vector3(768, 32, 440);
					var b = new Vector3(760, 24, 440);
					var c = new Vector3(760, 40, 512);
					var planeA = new Plane(a, b, c, Winding.Cw);

					a = new Vector3(760, 24, 440);
					b = new Vector3(768, 32, 440);
					c = new Vector3(704, -8, 512);
					var planeB = new Plane(a, b, c, Winding.Cw);

					a = new Vector3(768, 32, 440);
					b = new Vector3(760, 40, 512);
					c = new Vector3(704, -8, 512);
					var planeC = new Plane(a, b, c, Winding.Cw);

					a = new Vector3(760, 40, 512);
					b = new Vector3(760, 24, 440);
					c = new Vector3(704, -8, 512);
					var planeD = new Plane(a, b, c, Winding.Cw);

					Vector3 intersectionABC = Plane.Intersect(planeA, planeB, planeC);
					Vector3 intersectionABD = Plane.Intersect(planeA, planeB, planeD);
					Vector3 intersectionACD = Plane.Intersect(planeA, planeC, planeD);
					Vector3 intersectionBCD = Plane.Intersect(planeB, planeC, planeD);

					Assert.That(intersectionABC.X, Is.EqualTo(768).Within(Tolerance));
					Assert.That(intersectionABC.Y, Is.EqualTo(32).Within(Tolerance));
					Assert.That(intersectionABC.Z, Is.EqualTo(440).Within(Tolerance));

					Assert.That(intersectionABD.X, Is.EqualTo(760).Within(Tolerance));
					Assert.That(intersectionABD.Y, Is.EqualTo(24).Within(Tolerance));
					Assert.That(intersectionABD.Z, Is.EqualTo(440).Within(Tolerance));

					Assert.That(intersectionACD.X, Is.EqualTo(760).Within(Tolerance));
					Assert.That(intersectionACD.Y, Is.EqualTo(40).Within(Tolerance));
					Assert.That(intersectionACD.Z, Is.EqualTo(512).Within(Tolerance));

					Assert.That(intersectionBCD.X, Is.EqualTo(704).Within(Tolerance));
					Assert.That(intersectionBCD.Y, Is.EqualTo(-8).Within(Tolerance));
					Assert.That(intersectionBCD.Z, Is.EqualTo(512).Within(Tolerance));
				}
			}

			[TestFixture]
			public class NormalTest
			{
				[TestCase]
				public void Clockwise()
				{
					var points = new List<Vertex>() { new Vertex(-256, 512, 512), new Vertex(-256, 512, 0), new Vertex(-256, 0, 512) };
					var plane = new Plane(points, Winding.Cw);

					Assert.That(plane.Normal.X, Is.EqualTo(1).Within(Tolerance));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Tolerance));
				}

				[TestCase]
				public void Counterclockwise()
				{
					var points = new List<Vertex>() { new Vertex(-256, 0, 0), new Vertex(-256, 512, 0), new Vertex(-256, 512, 512) };
					var plane = new Plane(points, Winding.Ccw);

					Assert.That(plane.Normal.X, Is.EqualTo(1).Within(Tolerance));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Tolerance));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Tolerance));
				}
			}
		}
	}
}
