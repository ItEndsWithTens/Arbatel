using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor;
using Temblor.Utilities;

namespace TemblorTest.Core
{
	public class MathUtilitiesTest
	{
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
		}
	}
}
