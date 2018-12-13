using nucs.JsonSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbatel.UI.Settings
{
	/// <summary>
	/// Settings that are per-user and machine independent.
	/// </summary>
	public class RoamingSettings : JsonSettings
	{
		public override string FileName { get; set; }

		public string LastBuiltInPalette { get; set; } = "";
	}
}
