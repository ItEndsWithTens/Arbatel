using OpenTK;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor;
using Temblor.Graphics;

namespace TemblorTest.Core
{
	public class GraphicsTest
	{
		public class PlaneTest
		{
			[TestFixture]
			public class IntersectionTest
			{
				// With apologies to Bruce Dawson.
				const float epsilon = 0.0001f;

				[TestCase]
				public void PlanesDoNotIntersect()
				{
					var a = new Vector3(0.0f, 0.0f, 0.0f);
					var b = new Vector3(0.0f, 32.0f, 0.0f);
					var c = new Vector3(32.0f, 0.0f, 0.0f);
					var planeA = new Plane(a, b, c);

					a = new Vector3(0.0f, 0.0f, 16.0f);
					b = new Vector3(32.0f, 0.0f, 16.0f);
					c = new Vector3(32.0f, 32.0f, 16.0f);
					var planeB = new Plane(a, b, c);

					a = new Vector3(0.0f, 0.0f, 64.0f);
					b = new Vector3(32.0f, 0.0f, 64.0f);
					c = new Vector3(32.0f, 32.0f, 64.0f);
					var planeC = new Plane(a, b, c);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(float.NaN));
					Assert.That(intersection.Y, Is.EqualTo(float.NaN));
					Assert.That(intersection.Z, Is.EqualTo(float.NaN));
				}

				[TestCase]
				public void PlanesIntersectAtOrigin()
				{
					var a = new Vector3(0.0f, 0.0f, 0.0f);
					var b = new Vector3(0.0f, 32.0f, 0.0f);
					var c = new Vector3(32.0f, 0.0f, 0.0f);
					var planeA = new Plane(a, b, c);

					a = new Vector3(0.0f, 0.0f, 0.0f);
					b = new Vector3(32.0f, 0.0f, 0.0f);
					c = new Vector3(0.0f, 0.0f, 32.0f);
					var planeB = new Plane(a, b, c);

					a = new Vector3(0.0f, 0.0f, 0.0f);
					b = new Vector3(0.0f, 0.0f, 32.0f);
					c = new Vector3(0.0f, 32.0f, 0.0f);
					var planeC = new Plane(a, b, c);

					Vector3 intersection = Plane.Intersect(planeA, planeB, planeC);

					Assert.That(intersection.X, Is.EqualTo(0.0f).Within(epsilon));
					Assert.That(intersection.Y, Is.EqualTo(0.0f).Within(epsilon));
					Assert.That(intersection.Z, Is.EqualTo(0.0f).Within(epsilon));
				}
			}
		}
	}
}
