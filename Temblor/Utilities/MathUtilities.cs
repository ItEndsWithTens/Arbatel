using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Utilities
{
	public class MathUtilities
	{
		/// <summary>
		/// Get all unique combinations of a set.
		/// </summary>
		/// <param name="count">The number of items to combine.</param>
		/// <param name="size">Combinations must be exactly this length.</param>
		/// <returns></returns>
		public static List<List<int>> Combinations(int count, int size)
		{
			var items = new List<int>();
			for (var i = 0; i < count; i++)
			{
				items.Add(i);
			}

			return Combinations(items, size);
		}
		/// <summary>
		/// Get all unique combinations of a set.
		/// </summary>
		/// <param name="items">The items to combine.</param>
		/// <param name="size">Combinations must be exactly this length.</param>
		/// <returns></returns>
		public static List<List<int>> Combinations(List<int> items, int size)
		{
			var permutations = Permutations(items, size);
			var combinations = new List<List<int>>() { permutations[0] };

			for (var i = 1; i < permutations.Count; i++)
			{
				var permutation = permutations[i];

				var permutationIsInCombinations = false;
				foreach (var combination in combinations)
				{
					foreach (var item in permutation)
					{
						if (combination.Contains(item))
						{
							permutationIsInCombinations = true;
						}
						else
						{
							permutationIsInCombinations = false;
							break;
						}
					}

					if (permutationIsInCombinations)
					{
						break;
					}
				}

				if (!permutationIsInCombinations)
				{
					combinations.Add(permutation);
				}
			}

			return combinations;
		}

		/// <summary>
		/// Get all unique permutations of a set.
		/// </summary>
		/// <param name="items">The items to combine.</param>
		/// <param name="size">Permutations must be exactly this length.</param>
		/// <returns></returns>
		public static List<List<int>> Permutations(List<int> items, int size)
		{
			var permutations = new List<List<int>>();

			if (size == 1)
			{
				foreach (var item in items)
				{
					permutations.Add(new List<int>() { item });
				}

				return permutations;
			}

			foreach (var item in items)
			{
				var others = new List<int>(items);

				// Remove only removes the first instance of an element from the
				// list, which means it's perfect for duplicates; if a duplicate
				// is in the input, it shouldn't be collapsed into just one, it
				// should be allowed to hang around. Basically, assume users who
				// provide lists with duplicate items know what they're doing.
				others.Remove(item);

				var remainingPerms = Permutations(others, size - 1);

				foreach (var perm in remainingPerms)
				{
					var output = new List<int>() { item };
					output.AddRange(perm);
					permutations.Add(output);
				}
			}

			return permutations;
		}

		/// <summary>
		/// Bring 'angle' into the range (-360.0, 360.0).
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static float ModAngleToCircleSigned(float angle)
		{
			return angle % 360.0f;
		}

		/// <summary>
		/// Bring 'angle' into the range [0.0, 360.0).
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static float ModAngleToCircleUnsigned(float angle)
		{
			return (ModAngleToCircleSigned(angle) + 360.0f) % 360.0f;
		}
	}
}
