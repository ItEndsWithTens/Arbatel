using Eto.Drawing;
using nucs.JsonSettings;
using System;
using System.Collections.Generic;

namespace Arbatel
{
	/// <summary>
	/// Settings that are per-user and machine independent.
	/// </summary>
	public class RoamingSettings : JsonSettings
	{
		public override string FileName { get; set; }

		public string LastBuiltInPalette { get; set; } = "";

		public bool InvertMouseX { get; set; } = false;
		public bool InvertMouseY { get; set; } = false;

		/// <summary>
		/// The camera's movement speed, in units per frame.
		/// </summary>
		public int MovementSpeed { get; set; } = 64;

		public Dictionary<string, Dictionary<string, Color>> ColorSchemes { get; } = new Dictionary<string, Dictionary<string, Color>>();
		public string CurrentColorScheme { get; set; }

		public RoamingSettings()
		{
			AfterLoad += RoamingSettings_AfterLoad;
			BeforeSave += RoamingSettings_BeforeSave;
		}

		private void RoamingSettings_AfterLoad()
		{
			RestoreDefaults();
		}

		private void RoamingSettings_BeforeSave(ref string destination)
		{
			RestoreDefaults();
		}

		private void RestoreDefaults()
		{
			// If users have customized the colors for the 'Default' or 'Debug'
			// schemes, use those customizations. Otherwise, add preset default
			// values to accommodate accidental deletion or intentional reset.

			string defaultKey = "Default";
			if (!ColorSchemes.ContainsKey(defaultKey) || ColorSchemes[defaultKey] == null)
			{
				// Vanity, to be sure, but a pleasant shade nonetheless.
				var tensLogoPurple = new Color(0.4f, 0.3607843f, 0.7960784f, 1.0f);

				ColorSchemes[defaultKey] = new Dictionary<string, Color>
				{
					{ "3D Wireframe", tensLogoPurple },
					{ "3D Flat", tensLogoPurple },
					{ "3D Textured", tensLogoPurple }
				};
			}

			string neutralKey = "Neutral";
			if (!ColorSchemes.ContainsKey(neutralKey) || ColorSchemes[neutralKey] == null)
			{
				var gray = new Color(0.5f, 0.5f, 0.5f, 1.0f);

				ColorSchemes[neutralKey] = new Dictionary<string, Color>
				{
					{ "3D Wireframe", gray },
					{ "3D Flat", gray },
					{ "3D Textured", gray }
				};
			}

			string debugKey = "Debug";
			if (!ColorSchemes.ContainsKey(debugKey) || ColorSchemes[debugKey] == null)
			{
				ColorSchemes[debugKey] = new Dictionary<string, Color>
				{
					{ "3D Wireframe", new Color(1.0f, 0.0f, 0.0f, 1.0f) },
					{ "3D Flat", new Color(0.0f, 1.0f, 0.0f, 1.0f) },
					{ "3D Textured", new Color(0.0f, 0.0f, 1.0f, 1.0f) }
				};
			}

			if (String.IsNullOrEmpty(CurrentColorScheme))
			{
				CurrentColorScheme = "Default";
			}
		}
	}
}
