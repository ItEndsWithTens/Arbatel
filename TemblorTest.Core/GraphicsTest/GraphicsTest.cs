using OpenTK;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor;
using Temblor.Graphics;

namespace TemblorTest.Core.GraphicsTest
{
	public class GraphicsTest
	{
		public const float Epsilon = 0.001f;

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
					var plane = new Plane(a, b, c, Winding.CW);

					Assert.That(plane.Normal.X, Is.EqualTo(1).Within(Epsilon));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.DistanceFromOrigin, Is.EqualTo(256).Within(Epsilon));
				}

				[TestCase]
				public void PositiveDistanceFacingAwayFromOriginCcw()
				{
					var a = new Vector3(256, 256, 0); 
					var b = new Vector3(256, 0, 256);
					var c = new Vector3(256, 0, 0);
					var plane = new Plane(a, b, c, Winding.CCW);

					Assert.That(plane.Normal.X, Is.EqualTo(1).Within(Epsilon));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.DistanceFromOrigin, Is.EqualTo(256).Within(Epsilon));
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
					var plane = new Plane(a, b, c, Winding.CW);

					Assert.That(plane.Normal.X, Is.EqualTo(-1).Within(Epsilon));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.DistanceFromOrigin, Is.EqualTo(-256).Within(Epsilon));
				}

				[TestCase]
				public void PositiveDistanceFacingTowardOriginCcw()
				{
					var a = new Vector3(256, 256, 0);
					var b = new Vector3(256, 0, 0);
					var c = new Vector3(256, 0, 256);
					var plane = new Plane(a, b, c, Winding.CCW);

					Assert.That(plane.Normal.X, Is.EqualTo(-1).Within(Epsilon));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.DistanceFromOrigin, Is.EqualTo(-256).Within(Epsilon));
				}

				[TestCase]
				public void NegativeDistanceFacingAwayFromOriginCw()
				{
					var a = new Vector3(-256, 256, 0); 
					var b = new Vector3(-256, 0, 256);
					var c = new Vector3(-256, 0, 0);
					var plane = new Plane(a, b, c, Winding.CW);

					Assert.That(plane.Normal.X, Is.EqualTo(-1).Within(Epsilon));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.DistanceFromOrigin, Is.EqualTo(256).Within(Epsilon));
				}

				[TestCase]
				public void NegativeDistanceFacingAwayFromOriginCcw()
				{
					var a = new Vector3(-256, 0, 0);
					var b = new Vector3(-256, 0, 256);
					var c = new Vector3(-256, 256, 0);
					var plane = new Plane(a, b, c, Winding.CCW);

					Assert.That(plane.Normal.X, Is.EqualTo(-1).Within(Epsilon));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.DistanceFromOrigin, Is.EqualTo(256).Within(Epsilon));
				}
			}

			[TestFixture]
			public class IntersectionTest
			{
				// With apologies to Bruce Dawson.
				const float Epsilon = 0.001f;

				[TestCase]
				public void PlanesDoNotIntersect()
				{
					var a = new Vector3(0.0f, 0.0f, 0.0f);
					var b = new Vector3(0.0f, 32.0f, 0.0f);
					var c = new Vector3(32.0f, 0.0f, 0.0f);
					var planeA = new Plane(a, b, c, Winding.CW);

					a = new Vector3(0.0f, 0.0f, 16.0f);
					b = new Vector3(0.0f, 32.0f, 16.0f);
					c = new Vector3(32.0f, 32.0f, 16.0f);
					var planeB = new Plane(a, b, c, Winding.CW);

					a = new Vector3(0.0f, 0.0f, 64.0f);
					b = new Vector3(0.0f, 32.0f, 64.0f);
					c = new Vector3(32.0f, 32.0f, 64.0f);
					var planeC = new Plane(a, b, c, Winding.CW);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(float.NaN));
					Assert.That(intersection.Y, Is.EqualTo(float.NaN));
					Assert.That(intersection.Z, Is.EqualTo(float.NaN));
				}

				[TestCase]
				public void PlanesIntersectAtOriginCw()
				{
					var a = new Vector3(0.0f, 0.0f, 0.0f);
					var b = new Vector3(0.0f, 32.0f, 0.0f);
					var c = new Vector3(32.0f, 0.0f, 0.0f);
					var planeA = new Plane(a, b, c, Winding.CW);

					a = new Vector3(0.0f, 0.0f, 0.0f);
					b = new Vector3(32.0f, 0.0f, 0.0f);
					c = new Vector3(0.0f, 0.0f, 32.0f);
					var planeB = new Plane(a, b, c, Winding.CW);

					a = new Vector3(0.0f, 0.0f, 0.0f);
					b = new Vector3(0.0f, 0.0f, 32.0f);
					c = new Vector3(0.0f, 32.0f, 0.0f);
					var planeC = new Plane(a, b, c, Winding.CW);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(0.0f).Within(Epsilon));
					Assert.That(intersection.Y, Is.EqualTo(0.0f).Within(Epsilon));
					Assert.That(intersection.Z, Is.EqualTo(0.0f).Within(Epsilon));
				}

				[TestCase]
				public void PlanesIntersectAtOriginCcw()
				{
					var a = new Vector3(0.0f, 0.0f, 0.0f);
					var b = new Vector3(0.0f, 32.0f, 0.0f);
					var c = new Vector3(32.0f, 0.0f, 0.0f);
					var planeA = new Plane(a, b, c, Winding.CCW);

					a = new Vector3(0.0f, 0.0f, 0.0f);
					b = new Vector3(32.0f, 0.0f, 0.0f);
					c = new Vector3(0.0f, 0.0f, 32.0f);
					var planeB = new Plane(a, b, c, Winding.CCW);

					a = new Vector3(0.0f, 0.0f, 0.0f);
					b = new Vector3(0.0f, 0.0f, 32.0f);
					c = new Vector3(0.0f, 32.0f, 0.0f);
					var planeC = new Plane(a, b, c, Winding.CCW);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(0.0f).Within(Epsilon));
					Assert.That(intersection.Y, Is.EqualTo(0.0f).Within(Epsilon));
					Assert.That(intersection.Z, Is.EqualTo(0.0f).Within(Epsilon));
				}

				[TestCase]
				public void PlanesIntersectCw()
				{
					var a = new Vector3(-256.0f, 512.0f, 512.0f); 
					var b = new Vector3(-256.0f, 512.0f, 0.0f);
					var c = new Vector3(-256.0f, 0.0f, 0.0f);
					var planeA = new Plane(a, b, c, Winding.CW);

					a = new Vector3(-768.0f, 512.0f, 512.0f); 
					b = new Vector3(-256.0f, 512.0f, 512.0f);
					c = new Vector3(-256.0f, 0.0f, 512.0f);
					var planeB = new Plane(a, b, c, Winding.CW);

					a = new Vector3(-256.0f, 0.0f, 512.0f); 
					b = new Vector3(-768.0f, 0.0f, 0.0f);
					c = new Vector3(-768.0f, 0.0f, 512.0f);
					var planeC = new Plane(a, b, c, Winding.CW);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(-256.0f).Within(Epsilon));
					Assert.That(intersection.Y, Is.EqualTo(0.0f).Within(Epsilon));
					Assert.That(intersection.Z, Is.EqualTo(512.0f).Within(Epsilon));
				}

				[TestCase]
				public void PlanesIntersectCcw()
				{
					var a = new Vector3(-256.0f, 0.0f, 0.0f);
					var b = new Vector3(-256.0f, 512.0f, 0.0f);
					var c = new Vector3(-256.0f, 512.0f, 512.0f);
					var planeA = new Plane(a, b, c, Winding.CCW);

					a = new Vector3(-256.0f, 0.0f, 512.0f);
					b = new Vector3(-256.0f, 512.0f, 512.0f);
					c = new Vector3(-768.0f, 512.0f, 512.0f);
					var planeB = new Plane(a, b, c, Winding.CCW);

					a = new Vector3(-768.0f, 0.0f, 512.0f);
					b = new Vector3(-768.0f, 0.0f, 0.0f);
					c = new Vector3(-256.0f, 0.0f, 512.0f);
					var planeC = new Plane(a, b, c, Winding.CCW);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(-256.0f).Within(Epsilon));
					Assert.That(intersection.Y, Is.EqualTo(0.0f).Within(Epsilon));
					Assert.That(intersection.Z, Is.EqualTo(512.0f).Within(Epsilon));
				}

				[TestCase]
				public void PlanesIntersectPyramidCw()
				{
					var a = new Vector3(0, 0, 512);
					var b = new Vector3(256, 256, 0);
					var c = new Vector3(256, -256, 0);
					var planeA = new Plane(a, b, c, Winding.CW);

					a = new Vector3(0, 0, 512);
					b = new Vector3(256, -256, 0);
					c = new Vector3(-256, -256, 0);
					var planeB = new Plane(a, b, c, Winding.CW);

					a = new Vector3(0, 0, 512);
					b = new Vector3(-256, 256, 0);
					c = new Vector3(256, 256, 0);
					var planeC = new Plane(a, b, c, Winding.CW);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(0.0f).Within(Epsilon));
					Assert.That(intersection.Y, Is.EqualTo(0.0f).Within(Epsilon));
					Assert.That(intersection.Z, Is.EqualTo(512.0f).Within(Epsilon));
				}
			}

			[TestFixture]
			public class NormalTest
			{
				[TestCase]
				public void Clockwise()
				{
					var points = new List<Vertex>() { new Vertex(-256, 512, 512), new Vertex(-256, 512, 0), new Vertex(-256, 0, 512) };
					var plane = new Plane(points, Winding.CW);

					Assert.That(plane.Normal.X, Is.EqualTo(1).Within(Epsilon));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Epsilon));
				}

				[TestCase]
				public void Counterclockwise()
				{
					var points = new List<Vertex>() { new Vertex(-256, 0, 0), new Vertex(-256, 512, 0), new Vertex(-256, 512, 512) };
					var plane = new Plane(points, Winding.CCW);

					Assert.That(plane.Normal.X, Is.EqualTo(1).Within(Epsilon));
					Assert.That(plane.Normal.Y, Is.EqualTo(0).Within(Epsilon));
					Assert.That(plane.Normal.Z, Is.EqualTo(0).Within(Epsilon));
				}
			}
		}
	}
}
