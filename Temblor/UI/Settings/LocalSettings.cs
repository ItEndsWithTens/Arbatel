using Newtonsoft.Json;
using nucs.JsonSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;

namespace Temblor.UI.Settings
{
	/// <summary>
	/// Settings that are per-user, but machine dependent.
	/// </summary>
	public class LocalSettings : JsonSettings
	{
		public override string FileName { get; set; }

		public List<string> DefinitionDictionaryPaths { get; set; } = new List<string>();
		public List<string> TextureDictionaryPaths { get; } = new List<string>();

		public Uri LastMapDirectory { get; set; }
		public Uri LastFgdDirectory { get; set; }
		public Uri LastWadDirectory { get; set; }
		public Uri LastSaveCollapsedAsDirectory { get; set; }

		public LocalSettings()
		{
			LastMapDirectory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
			LastFgdDirectory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
			LastWadDirectory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
			LastSaveCollapsedAsDirectory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
		}
	}
}
