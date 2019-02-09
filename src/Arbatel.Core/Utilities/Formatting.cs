using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbatel.Utilities
{
	public static class StringExtensions
	{
		public static string[] SplitAndKeepDelimiters(this string raw, params string[] delimiters)
		{
			var hits = new List<(string, int)>();

			foreach (string d in delimiters)
			{
				int position = 0;
				while (position < raw.Length)
				{
					int hit = raw.IndexOf(d, position, StringComparison.OrdinalIgnoreCase);

					if (hit >= 0)
					{
						hits.Add((d, hit));
						position = hit + d.Length;
					}
					else
					{
						position = raw.Length;
					}
				}
			}
			hits.RemoveAll(h => h.Item2 == -1);
			hits.Sort((lhs, rhs) => lhs.Item2.CompareTo(rhs.Item2));

			var split = new List<string>();

			int start = 0;
			foreach ((string d, int i) in hits)
			{
				split.Add(raw.Substring(start, i - start));
				split.Add(raw.Substring(i, d.Length));

				start = i + d.Length;
			}

			return split.ToArray();
		}

		public static Vector3 ToVector3(this string s)
		{
			return s.ToVector3(' ');
		}
		public static Vector3 ToVector3(this string s, char delimiter)
		{
			return ToVector3(s.Split(delimiter).ToList());
		}
		public static Vector3 ToVector3(this List<string> s)
		{
			if (s.Count != 3)
			{
				throw new ArgumentException("Input list must have 3 items!");
			}

			return ToVector3(s[0], s[1], s[2]);
		}
		public static Vector3 ToVector3(string x, string y, string z)
		{
			var vector = new Vector3();

			bool gotX = Single.TryParse(x, out vector.X);
			bool gotY = Single.TryParse(y, out vector.Y);
			bool gotZ = Single.TryParse(z, out vector.Z);

			if (!(gotX && gotY && gotZ))
			{
				throw new ArgumentException("Input couldn't be converted to Vector3!");
			}

			return vector;
		}
	}
}
