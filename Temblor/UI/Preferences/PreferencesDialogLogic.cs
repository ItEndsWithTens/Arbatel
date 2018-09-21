using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using System.IO;
using Temblor.UI.Settings;
using nucs.JsonSettings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Temblor.Formats;
using System.Linq;

namespace Temblor.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		public LocalSettings LocalSettings { get; private set; }

		public RoamingSettings RoamingSettings { get; private set; }

		public PreferencesDialog()
		{
			InitializeComponent();

			InitializeCommands();

			string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			Assembly assembly = Assembly.GetAssembly(typeof(MainForm));
			var attribute = (AssemblyProductAttribute)assembly.GetCustomAttribute(typeof(AssemblyProductAttribute));

			string localPath = Path.Combine(localAppData, attribute.Product);
			string roamingPath = Path.Combine(roamingAppData, attribute.Product);

			LocalSettings = JsonSettings.Load<LocalSettings>(Path.Combine(localPath, "LocalSettings.json"));
			RoamingSettings = JsonSettings.Load<RoamingSettings>(Path.Combine(roamingPath, "RoamingSettings.json"));

			RefreshDisplayedDefinitions();
			RefreshDisplayedTextures();

			CommitChanges();
		}

		private void CommitChanges()
		{
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
