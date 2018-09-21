using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Utilities
{
	public static class UIExtensions
	{
		[Conditional("DEBUG")]
		public static void DebugAssertChildPresence(this Container container, string name)
		{
			var message = "Child control with ID \"" + name + "\" not found!";

			Debug.Assert(container.FindChild(name) != null, message);
		}
	}
}
