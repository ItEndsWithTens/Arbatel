using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using System.IO;
using Arbatel.UI.Settings;
using nucs.JsonSettings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Arbatel.Formats;
using System.Linq;

namespace Arbatel.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		public LocalSettings LocalSettings { get; private set; }

		public RoamingSettings RoamingSettings { get; private set; }

		private static void ConfigSettings(JsonSettings settings)
		{
			// Work around a bug in JsonSettings on macOS.
			settings.BeforeLoad += (ref string destination) => destination = destination.Replace("\\", "/");
			settings.BeforeSave += (ref string destination) => destination = destination.Replace("\\", "/");
		}

		public PreferencesDialog()
		{
			InitializeComponent();

			InitializeCommands();

			string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			string localPath = Path.Combine(localAppData, Core.Name, "LocalSettings.json");
			string roamingPath = Path.Combine(roamingAppData, Core.Name, "RoamingSettings.json");

			LocalSettings = JsonSettings.Load<LocalSettings>(localPath, ConfigSettings);
			RoamingSettings = JsonSettings.Load<RoamingSettings>(roamingPath, ConfigSettings);

			CbxInvertX.Checked = RoamingSettings.InvertMouseX;
			CbxInvertY.Checked = RoamingSettings.InvertMouseY;

			var drpPalette = FindChild<DropDown>("drpPalette");
			var fpkPalette = FindChild<FilePicker>("fpkPalette");

			if (RoamingSettings.LastBuiltInPalette.Length > 0)
			{
				drpPalette.SelectedKey = RoamingSettings.LastBuiltInPalette;
			}

			if (LocalSettings.LastCustomPalette?.LocalPath.Length > 0)
			{
				fpkPalette.FilePath = LocalSettings.LastCustomPalette.LocalPath;
			}

			if (LocalSettings.UsingCustomPalette)
			{
				FindChild<RadioButton>("btnCustomPalette").Checked = true;
			}
			else
			{
				FindChild<RadioButton>("btnBuiltInPalette").Checked = true;
			}

			RefreshDisplayedDefinitions();
			RefreshDisplayedTextures();

			CommitChanges();
		}

		private void CommitChanges()
		{
			RoamingSettings.LastBuiltInPalette = FindChild<DropDown>("drpPalette").SelectedKey.ToString();

			RoamingSettings.InvertMouseX = CbxInvertX.Checked ?? false;
			RoamingSettings.InvertMouseY = CbxInvertY.Checked ?? false;

			var fpkPalette = FindChild<FilePicker>("fpkPalette");
			if (fpkPalette.Enabled)
			{
				if (fpkPalette.FilePath.Length > 0)
				{
					LocalSettings.LastCustomPalette = new Uri(fpkPalette.FilePath);
				}
				LocalSettings.UsingCustomPalette = true;
			}
			else
			{
				LocalSettings.UsingCustomPalette = false;
			}

			RefreshStoredDefinitions();
			RefreshStoredTextures();

			LocalSettings.Save();
			RoamingSettings.Save();

			ShouldCommitChanges = false;
		}

		private void RefreshDisplayedDefinitions()
		{
			var lbxFgd = FindChild<ListBox>(LbxFgdName);

			lbxFgd.Items.Clear();

			foreach (var path in LocalSettings.DefinitionDictionaryPaths)
			{
				lbxFgd.Items.Add(path);
			}
		}

		private void RefreshDisplayedTextures()
		{
			var lbxWad = FindChild<ListBox>(LbxWadName);

			lbxWad.Items.Clear();

			foreach (var path in LocalSettings.TextureDictionaryPaths)
			{
				lbxWad.Items.Add(path);
			}
		}

		private void RefreshStoredDefinitions()
		{
			var lbxFgd = FindChild<ListBox>(LbxFgdName);

			LocalSettings.DefinitionDictionaryPaths.Clear();

			foreach (var item in lbxFgd.Items)
			{
				LocalSettings.DefinitionDictionaryPaths.Add(item.Text);
			}
		}

		private void RefreshStoredTextures()
		{
			var lbxWad = FindChild<ListBox>(LbxWadName);

			LocalSettings.TextureDictionaryPaths.Clear();

			foreach (var item in lbxWad.Items)
			{
				LocalSettings.TextureDictionaryPaths.Add(item.Text);
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			RefreshDisplayedDefinitions();
			RefreshDisplayedTextures();
		}
	}
}