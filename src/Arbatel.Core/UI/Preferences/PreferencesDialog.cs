using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;

namespace Arbatel.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		private Settings Settings;

		public PreferencesDialog(Settings settings)
		{
			Settings = settings;

			InitializeComponent();

			InitializeCommands();

			CbxInvertX.Checked = Settings.Roaming.InvertMouseX;
			CbxInvertY.Checked = Settings.Roaming.InvertMouseY;

			DropDown drpPalette = FindChild<DropDown>("drpPalette");
			FilePicker fpkPalette = FindChild<FilePicker>("fpkPalette");

			if (Settings.Roaming.LastBuiltInPalette.Length > 0)
			{
				drpPalette.SelectedKey = Settings.Roaming.LastBuiltInPalette;
			}

			if (Settings.Local.LastCustomPalette?.LocalPath.Length > 0)
			{
				fpkPalette.FilePath = Settings.Local.LastCustomPalette.LocalPath;
			}

			if (Settings.Local.UsingCustomPalette)
			{
				FindChild<RadioButton>("btnCustomPalette").Checked = true;
			}
			else
			{
				FindChild<RadioButton>("btnBuiltInPalette").Checked = true;
			}

			RefreshDisplayedDefinitions();
			RefreshDisplayedTextures();
		}

		private void CommitChanges()
		{
			Settings.Roaming.LastBuiltInPalette = FindChild<DropDown>("drpPalette").SelectedKey;

			Settings.Roaming.InvertMouseX = CbxInvertX.Checked ?? false;
			Settings.Roaming.InvertMouseY = CbxInvertY.Checked ?? false;

			FilePicker fpkPalette = FindChild<FilePicker>("fpkPalette");
			if (fpkPalette.Enabled)
			{
				if (fpkPalette.FilePath.Length > 0)
				{
					Settings.Local.LastCustomPalette = new Uri(fpkPalette.FilePath);
				}
				Settings.Local.UsingCustomPalette = true;
			}
			else
			{
				Settings.Local.UsingCustomPalette = false;
			}

			RefreshStoredDefinitions();
			RefreshStoredTextures();

			Settings.Roaming.ColorSchemes.Clear();
			foreach (KeyValuePair<string, Dictionary<string, Color>> scheme in NewColorSchemes)
			{
				Settings.Roaming.ColorSchemes.Add(scheme.Key, scheme.Value);
			}
			Settings.Roaming.CurrentColorScheme = NewCurrentColorScheme;

			Settings.Save();

			ShouldCommitChanges = false;
		}

		private void RefreshDisplayedDefinitions()
		{
			ListBox lbxFgd = FindChild<ListBox>(LbxFgdName);

			lbxFgd.Items.Clear();

			foreach (string path in Settings.Local.DefinitionDictionaryPaths)
			{
				lbxFgd.Items.Add(path);
			}
		}

		private void RefreshDisplayedTextures()
		{
			ListBox lbxWad = FindChild<ListBox>(LbxWadName);

			lbxWad.Items.Clear();

			foreach (string path in Settings.Local.TextureDictionaryPaths)
			{
				lbxWad.Items.Add(path);
			}
		}

		private void RefreshStoredDefinitions()
		{
			ListBox lbxFgd = FindChild<ListBox>(LbxFgdName);

			Settings.Local.DefinitionDictionaryPaths.Clear();

			foreach (IListItem item in lbxFgd.Items)
			{
				Settings.Local.DefinitionDictionaryPaths.Add(item.Text);
			}
		}

		private void RefreshStoredTextures()
		{
			ListBox lbxWad = FindChild<ListBox>(LbxWadName);

			Settings.Local.TextureDictionaryPaths.Clear();

			foreach (IListItem item in lbxWad.Items)
			{
				Settings.Local.TextureDictionaryPaths.Add(item.Text);
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
