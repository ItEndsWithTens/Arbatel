using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arbatel.Graphics;
using Arbatel.Utilities;

namespace Arbatel.Formats
{
	public class Solid
	{
		public List<Side> Sides;

		public Solid()
		{
			Sides = new List<Side>();
		}
		public Solid(List<Side> sides) : this()
		{
			Sides = sides;
		}
	}
}
