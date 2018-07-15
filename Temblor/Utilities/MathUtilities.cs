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
