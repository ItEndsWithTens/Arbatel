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
		public List<IUpdateFromSettings> Updatables { get; private set; } = new List<IUpdateFromSettings>();

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

			Local.AfterSave += Local_AfterSave;
			Roaming.AfterSave += Roaming_AfterSave;
		}

		private void Local_AfterSave(string destinition)
		{
			foreach (IUpdateFromSettings updatable in Updatables)
			{
				updatable.UpdateFromSettings(this);
			}
		}

		private void Roaming_AfterSave(string destinition)
		{
			foreach (IUpdateFromSettings updatable in Updatables)
			{
				updatable.UpdateFromSettings(this);
			}
		}
	}
}
