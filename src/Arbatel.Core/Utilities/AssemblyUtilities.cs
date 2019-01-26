using System;
using System.IO;
using System.Reflection;

namespace Arbatel.Utilities
{
	public static class AssemblyUtilities
	{
		public static Stream GetResourceStream(this Assembly assembly, string name)
		{
			var attribute = (AssemblyTitleAttribute)assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute));

			return assembly.GetManifestResourceStream($"{attribute.Title}.res.{name}");
		}

		public static Type GetTypeFromName(string name)
		{
			Type type = null;

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				type = assembly.GetType(name);

				if (type != null)
				{
					break;
				}
			}

			return type;
		}
	}
}
