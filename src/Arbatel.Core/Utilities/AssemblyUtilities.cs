using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Arbatel.Utilities
{
	public static class AssemblyExtensions
	{
		public static Stream GetResourceStream(this Assembly assembly, string name)
		{
			var attribute = (AssemblyTitleAttribute)assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute));

			return assembly.GetManifestResourceStream($"{attribute.Title}.res.{name}");
		}
	}
}
