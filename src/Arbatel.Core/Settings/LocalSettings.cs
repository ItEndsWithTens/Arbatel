using Arbatel.UI;
using nucs.JsonSettings;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Arbatel
{
	/// <summary>
	/// Settings that are per-user, but machine dependent.
	/// </summary>
	public class LocalSettings : JsonSettings
	{
		public override string FileName { get; set; }

		public List<string> DefinitionDictionaryPaths { get; } = new List<string>();
		public List<string> TextureDictionaryPaths { get; } = new List<string>();

		public Uri LastCustomPalette { get; set; }
		public bool UsingCustomPalette { get; set; } = false;

		public Uri LastMapDirectory { get; set; }
		public Uri LastFgdDirectory { get; set; }
		public Uri LastWadDirectory { get; set; }
		public Uri LastSaveCollapsedAsDirectory { get; set; }

		/// <summary>
		/// Mouse sensitivity, as an arbitrary integer.
		/// </summary>
		/// <remarks>
		/// Look input sensitivity depends on input hardware, driver settings,
		/// and similar variables that likely differ from one system to another,
		/// so this needs to be a per-machine setting.
		/// </remarks>
		public int MouseSensitivity { get; set; } = 50;

		public LocalSettings()
		{
			var assembly = Assembly.GetAssembly(typeof(MainForm));
			var attribute = (AssemblyProductAttribute)assembly.GetCustomAttribute(typeof(AssemblyProductAttribute));

			LastMapDirectory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
			LastFgdDirectory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
			LastWadDirectory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
			LastSaveCollapsedAsDirectory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
		}
	}
}
