using NUnit.Framework;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arbatel;
using Arbatel.Formats;
using Arbatel.Graphics;
using Arbatel.Utilities;

namespace ArbatelTest.Core.MathUtilitiesTest
{
	public class MathUtilitiesTest
	{
		public const float Epsilon = 0.001f;

		[TestFixture]
		public class PermutationsTest
		{
			List<List<int>> results;
			List<int> items;

			[TestCase]
			public void OneItem()
			{
				items = new List<int>() { 1 };
				results = MathUtilities.Permutations(items, 1);
				Assert.That(results.Count, Is.EqualTo(1));
				Assert.That(results[0], Contains.Item(1));
			}

			[TestCase]
			public void TwoItemsSizeOne()
			{
				items = new List<int>() { 1, 2 };
				results = MathUtilities.Permutations(items, 1);
				Assert.That(results.Count, Is.EqualTo(2));
				Assert.That(results[0], Is.EqualTo(new List<int>() { 1 }));
				Assert.That(results[1], Is.EqualTo(new List<int>() { 2 }));
			}

			[TestCase]
			public void TwoItemsSizeTwo()
			{
				items = new List<int>() { 1, 2 };
				results = MathUtilities.Permutations(items, 2);
				Assert.That(results.Count, Is.EqualTo(2));
				Assert.That(results[0], Is.EqualTo(new List<int>() { 1, 2 }));
				Assert.That(results[1], Is.EqualTo(new List<int>() { 2, 1 }));
			}

			[TestCase]
			public void ThreeItemsSizeOne()
			{
				items = new List<int>() { 1, 2, 3 };
				results = MathUtilities.Permutations(items, 1);
				Assert.That(results.Count, Is.EqualTo(3));
				Assert.That(results[0], Is.EqualTo(new List<int>() { 1 }));
				Assert.That(results[1], Is.EqualTo(new List<int>() { 2 }));
				Assert.That(results[2], Is.EqualTo(new List<int>() { 3 }));
			}

			[TestCase]
			public void ThreeItemsSizeTwo()
			{
				items = new List<int>() { 1, 2, 3 };
				results = MathUtilities.Permutations(items, 2);
				Assert.That(results.Count, Is.EqualTo(6));
				Assert.That(results[0], Is.EqualTo(new List<int>() { 1, 2 }));
				Assert.That(results[1], Is.EqualTo(new List<int>() { 1, 3 }));
				Assert.That(results[2], Is.EqualTo(new List<int>() { 2, 1 }));
				Assert.That(results[3], Is.EqualTo(new List<int>() { 2, 3 }));
				Assert.That(results[4], Is.EqualTo(new List<int>() { 3, 1 }));
				Assert.That(results[5], Is.EqualTo(new List<int>() { 3, 2 }));
			}

			[TestCase]
			public void ThreeItemsSizeThree()
			{
				items = new List<int>() { 1, 2, 3 };
				results = MathUtilities.Permutations(items, 3);
				Assert.That(results.Count, Is.EqualTo(6));
				Assert.That(results[0], Is.EqualTo(new List<int>() { 1, 2, 3 }));
				Assert.That(results[1], Is.EqualTo(new List<int>() { 1, 3, 2 }));
				Assert.That(results[2], Is.EqualTo(new List<int>() { 2, 1, 3 }));
				Assert.That(results[3], Is.EqualTo(new List<int>() { 2, 3, 1 }));
				Assert.That(results[4], Is.EqualTo(new List<int>() { 3, 1, 2 }));
				Assert.That(results[5], Is.EqualTo(new List<int>() { 3, 2, 1 }));
			}
			
			[TestCase]
			public void NinetySixItemsSizeThree()
			{
				items = new List<int>();

				for (var i = 0; i < 96; i++)
				{
					items.Add(i);
				}

				var sw = Stopwatch.StartNew();
				var permutations = MathUtilities.Permutations(items, 3);
				sw.Stop();

				Assert.That(permutations.Count, Is.EqualTo(857280));

				long average = sw.ElapsedMilliseconds;

				sw.Reset();

				for (var i = 0; i < 9; i++)
				{
					sw.Start();
					permutations = MathUtilities.Permutations(items, 3);
					sw.Stop();

					average = (average + sw.ElapsedMilliseconds) / 2;

					sw.Reset();
				}

				Assert.That(average, Is.LessThan(1500));
			}
		}

		[TestFixture]
		public class CombinationsTest
		{
			List<List<int>> results;
			List<int> items;

			[TestCase]
			public void OneItem()
			{
				items = new List<int>() { 1 };
				results = MathUtilities.Combinations(items, 1);
				Assert.That(results.Count, Is.EqualTo(1));
				Assert.That(results[0], Contains.Item(1));
			}

			[TestCase]
			public void TwoItemsSizeOne()
			{
				items = new List<int>() { 1, 2 };
				results = MathUtilities.Combinations(items, 1);
				Assert.That(results.Count, Is.EqualTo(2));
				Assert.That(results[0], Is.EqualTo(new List<int>() { 1 }));
				Assert.That(results[1], Is.EqualTo(new List<int>() { 2 }));
			}

			[TestCase]
			public void TwoItemsSizeTwo()
			{
				items = new List<int>() { 1, 2 };
				results = MathUtilities.Combinations(items, 2);
				Assert.That(results.Count, Is.EqualTo(1));
				Assert.That(results[0], Is.EqualTo(new List<int>() { 1, 2 }));
			}

			[TestCase]
			public void ThreeItemsSizeOne()
			{
				items = new List<int>() { 1, 2, 3 };
				results = MathUtilities.Combinations(items, 1);
				Assert.That(results.Count, Is.EqualTo(3));
				Assert.That(results[0], Is.EqualTo(new List<int>() { 1 }));
				Assert.That(results[1], Is.EqualTo(new List<int>() { 2 }));
				Assert.That(results[2], Is.EqualTo(new List<int>() { 3 }));
			}

			[TestCase]
			public void ThreeItemsSizeTwo()
			{
				items = new List<int>() { 1, 2, 3 };
				results = MathUtilities.Combinations(items, 2);
				Assert.That(results.Count, Is.EqualTo(3));
				Assert.That(results[0], Is.EqualTo(new List<int>() { 1, 2 }));
				Assert.That(results[1], Is.EqualTo(new List<int>() { 1, 3 }));
				Assert.That(results[2], Is.EqualTo(new List<int>() { 2, 3 }));
			}

			[TestCase]
			public void ThreeItemsSizeThree()
			{
				items = new List<int>() { 1, 2, 3 };
				results = MathUtilities.Combinations(items, 3);
				Assert.That(results.Count, Is.EqualTo(1));
				Assert.That(results[0], Is.EqualTo(new List<int>() { 1, 2, 3 }));
			}

			[TestCase]
			public void SixItemsSizeOne()
			{
				items = new List<int>() { 1, 2, 3, 4, 5, 6 };
				results = MathUtilities.Combinations(items, 1);
				Assert.That(results.Count, Is.EqualTo(6));
				Assert.That(results[0], Is.EqualTo(new List<int>() { 1 }));
				Assert.That(results[1], Is.EqualTo(new List<int>() { 2 }));
				Assert.That(results[2], Is.EqualTo(new List<int>() { 3 }));
				Assert.That(results[3], Is.EqualTo(new List<int>() { 4 }));
				Assert.That(results[4], Is.EqualTo(new List<int>() { 5 }));
				Assert.That(results[5], Is.EqualTo(new List<int>() { 6 }));
			}

			[TestCase]
			public void SixItemsSizeTwo()
			{
				items = new List<int>() { 1, 2, 3, 4, 5, 6 };
				results = MathUtilities.Combinations(items, 2);
				Assert.That(results.Count, Is.EqualTo(15));
				Assert.That(results[0], Is.EqualTo(new List<int>() { 1, 2 }));
				Assert.That(results[1], Is.EqualTo(new List<int>() { 1, 3 }));
				Assert.That(results[2], Is.EqualTo(new List<int>() { 1, 4 }));
				Assert.That(results[3], Is.EqualTo(new List<int>() { 1, 5 }));
				Assert.That(results[4], Is.EqualTo(new List<int>() { 1, 6 }));
				Assert.That(results[5], Is.EqualTo(new List<int>() { 2, 3 }));
				Assert.That(results[6], Is.EqualTo(new List<int>() { 2, 4 }));
				Assert.That(results[7], Is.EqualTo(new List<int>() { 2, 5 }));
				Assert.That(results[8], Is.EqualTo(new List<int>() { 2, 6 }));
				Assert.That(results[9], Is.EqualTo(new List<int>() { 3, 4 }));
				Assert.That(results[10], Is.EqualTo(new List<int>() { 3, 5 }));
				Assert.That(results[11], Is.EqualTo(new List<int>() { 3, 6 }));
				Assert.That(results[12], Is.EqualTo(new List<int>() { 4, 5 }));
				Assert.That(results[13], Is.EqualTo(new List<int>() { 4, 6 }));
				Assert.That(results[14], Is.EqualTo(new List<int>() { 5, 6 }));
			}

			[TestCase(Category = "Performance")]
			public void NinetySixItemsSizeThree()
			{
				items = new List<int>();

				for (var i = 0; i < 96; i++)
				{
					items.Add(i);
				}

				var sw = Stopwatch.StartNew();
				var combinations = MathUtilities.Combinations(items, 3);
				sw.Stop();

				Assert.That(combinations.Count, Is.EqualTo(142880));

				long average = sw.ElapsedMilliseconds;

				sw.Reset();

				for (var i = 0; i < 9; i++)
				{
					sw.Start();
					combinations = MathUtilities.Combinations(items, 3);
					sw.Stop();

					average = (average + sw.ElapsedMilliseconds) / 2;

					sw.Reset();
				}

				Assert.That(average, Is.LessThan(1000));
			}
		}

		[TestFixture]
		public class AngleTest
		{
			[TestCase]
			public void GetClockwiseAngleWholeNumbers()
			{
				var a = new Vector3(0, -48, 32);
				var b = new Vector3(0, 48, -32);
				var normal = new Vector3(1, 0, 0);

				double angle = MathUtilities.GetClockwiseAngle(a, b, normal);

				Assert.That(angle, Is.EqualTo(180.0).Within(Epsilon));
			}

			[TestCase]
			public void GetClockwiseAngleRoundingErrorInInput()
			{
				var a = new Vector3(0, -48, 32.00001f);
				var b = new Vector3(0, 48, -32);
				var normal = new Vector3(1, 0, 0);

				double angle = MathUtilities.GetClockwiseAngle(a, b, normal);

				Assert.That(angle, Is.EqualTo(-180.0).Within(Epsilon));
			}

			[TestCase]
			public void SignedAngleBetweenVectors()
			{
				var a = new Vector3(1, 0, 0);
				var b = new Vector3(0, 1, 0);
				var normal = new Vector3(0, 0, 1);

				var result = MathUtilities.SignedAngleBetweenVectors(a, b, normal);

				Assert.That(result, Is.EqualTo(90.0).Within(Epsilon));

				a = new Vector3(0, 1, 0);
				b = new Vector3(1, 0, 0);
				normal = new Vector3(0, 0, 1);

				result = MathUtilities.SignedAngleBetweenVectors(a, b, normal);

				Assert.That(result, Is.EqualTo(-90.0).Within(Epsilon));

				a = new Vector3(1, 0, 0);
				b = new Vector3(0, 1, 0);
				normal = new Vector3(0, 0, -1);

				result = MathUtilities.SignedAngleBetweenVectors(a, b, normal);

				Assert.That(result, Is.EqualTo(-90.0).Within(Epsilon));

				a = new Vector3(0, 1, 0);
				b = new Vector3(1, 0, 0);
				normal = new Vector3(0, 0, -1);

				result = MathUtilities.SignedAngleBetweenVectors(a, b, normal);

				Assert.That(result, Is.EqualTo(90.0).Within(Epsilon));

				a = new Vector3(1, 0, 0);
				b = new Vector3(-1, 0, 0);
				normal = new Vector3(0, 0, 1);

				result = MathUtilities.SignedAngleBetweenVectors(a, b, normal);

				Assert.That(result, Is.EqualTo(180.0).Within(Epsilon));
			}
		}

		[TestCase]
		public void SortVerticesCcw()
		{
			var side = new Side();

			side.Plane = new Plane(new Vector3(-256, 0, 0), new Vector3(-256, 512, 0), new Vector3(-256, 512, 512), Winding.Ccw);

			side.Vertices.Add(new Vertex(-256, 0, 0));
			side.Vertices.Add(new Vertex(-256, 0, 512));
			side.Vertices.Add(new Vertex(-256, 512, 0));
			side.Vertices.Add(new Vertex(-256, 512, 512));

			side.Vertices = MathUtilities.SortVertices(side.Vertices, side.Plane.Normal, Winding.Ccw);

			// No need for comparing floats with an epsilon here, since the
			// values going in are exact, and this only tests reordering the
			// list of vertices. None of their 3D coordinates are changed.
			Assert.That(side.Vertices[0], Is.EqualTo(new Vertex(-256, 0, 0)));
			Assert.That(side.Vertices[1], Is.EqualTo(new Vertex(-256, 512, 0)));
			Assert.That(side.Vertices[2], Is.EqualTo(new Vertex(-256, 512, 512)));
			Assert.That(side.Vertices[3], Is.EqualTo(new Vertex(-256, 0, 512)));
		}

		[TestCase]
		public void SortVerticesCw()
		{
			var side = new Side();

			side.Plane = new Plane(
				new Vector3(-256, 0, 0),
				new Vector3(-256, 512, 0),
				new Vector3(-256, 512, 512),
				Winding.Ccw);

			side.Vertices.Add(new Vertex(-256, 0, 0));
			side.Vertices.Add(new Vertex(-256, 0, 512));
			side.Vertices.Add(new Vertex(-256, 512, 0));
			side.Vertices.Add(new Vertex(-256, 512, 512));

			side.Vertices = MathUtilities.SortVertices(side.Vertices, side.Plane.Normal, Winding.Cw);

			Assert.That(side.Vertices[0], Is.EqualTo(new Vertex(-256, 0, 0)));
			Assert.That(side.Vertices[1], Is.EqualTo(new Vertex(-256, 0, 512)));
			Assert.That(side.Vertices[2], Is.EqualTo(new Vertex(-256, 512, 512)));
			Assert.That(side.Vertices[3], Is.EqualTo(new Vertex(-256, 512, 0)));
		}
	}
}
