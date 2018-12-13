using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbatel.Utilities
{
	public class Formatting
	{
		public static Vector3 StringToVector3(string value)
		{
			var vector = new Vector3();

			string[] split = value.Split(' ');
			float.TryParse(split[0], out vector.X);
			float.TryParse(split[1], out vector.Y);
			float.TryParse(split[2], out vector.Z);

			return vector;
		}
	}
}
