using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Arbatel
{
	public static partial class Core
	{
		public static string Name
		{
			get
			{
				Assembly assembly = Assembly.GetAssembly(typeof(UI.MainForm));
				var attribute = (AssemblyProductAttribute)assembly.GetCustomAttribute(typeof(AssemblyProductAttribute));

				return attribute.Product;
			}
		}
	}
}
