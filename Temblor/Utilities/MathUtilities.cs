using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Graphics;

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

		public static List<List<int>> Permutations(int count, int size)
		{
			var items = new List<int>();
			for (var i = 0; i < count; i++)
			{
				items.Add(i);
			}

			return Permutations(items, size);
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

		public static double SignedAngleBetweenVectors(Vector3 a, Vector3 b, Vector3 normal)
		{
			var dot1 = Vector3.Dot(normal, Vector3.Cross(a, b));
			var dot2 = Vector3.Dot(a, b);

			return MathHelper.RadiansToDegrees(Math.Atan2(dot1, dot2));
		}

		public static List<Vertex> SortVerticesCCW(List<Vector3> vectors, Vector3 normal)
		{
			var vertices = new List<Vertex>();

			foreach (var vector in vectors)
			{
				vertices.Add(new Vertex(vector));
			}

			return SortVerticesCCW(vertices, normal);
		}
		public static List<Vertex> SortVerticesCCW(List<Vertex> vertices, Vector3 normal)
		{
			List<List<int>> pairs = Permutations(vertices.Count, 2);

			var angles = new Dictionary<List<int>, double>();

			var min = new Vector3(vertices[0].Position);
			var max = new Vector3(vertices[0].Position);
			foreach (var vertex in vertices)
			{
				if (vertex.Position.X < min.X)
				{
					min.X = vertex.Position.X;
				}
				if (vertex.Position.X > max.X)
				{
					max.X = vertex.Position.X;
				}

				if (vertex.Position.Y < min.Y)
				{
					min.Y = vertex.Position.Y;
				}
				if (vertex.Position.Y > max.Y)
				{
					max.Y = vertex.Position.Y;
				}

				if (vertex.Position.Z < min.Z)
				{
					min.Z = vertex.Position.Z;
				}
				if (vertex.Position.Z > max.Z)
				{
					max.Z = vertex.Position.Z;
				}
			}

			Vector3 center = min + ((max - min) / 2.0f);

			foreach (var pair in pairs)
			{
				var a = vertices[pair[0]].Position - center;
				var b = vertices[pair[1]].Position - center;

				double angle = SignedAngleBetweenVectors(a, b, normal);

				// Only add counterclockwise angles.
				if (angle > 0.0)
				{
					angles.Add(pair, angle);
				}
			}

			var sorted = new List<Vertex>() { vertices[0] };

			var currentIndex = 0;
			for (var i = 0; i < vertices.Count - 1; i++)
			{
				var previousAngle = 181.0;
				var nextIndex = 0;
				foreach (var candidate in angles)
				{
					if (candidate.Key[0] != currentIndex)
					{
						continue;
					}

					if (Math.Abs(candidate.Value) < previousAngle)
					{
						nextIndex = candidate.Key[1];
						previousAngle = Math.Abs(candidate.Value);
					}
				}

				sorted.Add(vertices[nextIndex]);
				currentIndex = nextIndex;
			}

			return sorted;
		}
	}
}
