using nucs.JsonSettings;
using System;
using System.Collections.Generic;
using System.IO;

namespace Arbatel
{
	public interface IUpdateFromSettings
	{
		void UpdateFromSettings(Settings settings);
	}

	public class Settings
	{
		public LocalSettings Local { get; private set; }
		public RoamingSettings Roaming { get; private set; }

		/// <summary>
		/// Any objects that may be affected by these settings.
		/// </summary>
		public List<IUpdateFromSettings> Updatables { get; } = new List<IUpdateFromSettings>();

		private static void ConfigSettings(JsonSettings settings)
		{
			// Work around a bug in JsonSettings on macOS.
			settings.BeforeLoad += (ref string destination) => destination = destination.Replace("\\", "/");
			settings.BeforeSave += (ref string destination) => destination = destination.Replace("\\", "/");
		}

		public Settings()
		{
			string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			string localPath = Path.Combine(localAppData, Core.Name, "LocalSettings.json");
			string roamingPath = Path.Combine(roamingAppData, Core.Name, "RoamingSettings.json");

			Local = JsonSettings.Load<LocalSettings>(localPath, ConfigSettings);
			Roaming = JsonSettings.Load<RoamingSettings>(roamingPath, ConfigSettings);
		}

		public void Save()
		{
			Local.Save();
			Roaming.Save();

			foreach (IUpdateFromSettings updatable in Updatables)
			{
				updatable.UpdateFromSettings(this);
			}
		}
	}
}
