using System.IO;
using System.Reflection;

namespace Arbatel
{
	public static partial class Core
	{
		public static Assembly Assembly => Assembly.GetAssembly(typeof(UI.MainForm));

		public static string Location => Directory.GetParent(Assembly.Location).FullName;

		public static string Name
		{
			get
			{
				var attribute = (AssemblyProductAttribute)Assembly.GetCustomAttribute(typeof(AssemblyProductAttribute));

				return attribute.Product;
			}
		}
	}
}
