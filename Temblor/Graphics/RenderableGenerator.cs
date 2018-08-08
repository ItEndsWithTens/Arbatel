using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;

namespace Temblor.Graphics
{
	public class RenderableGenerator
	{
		public RenderableGenerator()
		{
		}

		virtual public Renderable Generate()
		{
			return new Renderable();
		}
	}
}
