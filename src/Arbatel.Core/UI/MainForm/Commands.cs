using Arbatel.Controls;
using Arbatel.Formats;
using Arbatel.Formats.Quake;
using Arbatel.UI.Preferences;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Arbatel.UI
{
	public partial class MainForm : Form
	{
		private Command CmdAbout = new Command() { MenuText = "About" };

		private Command CmdClose = new Command
		{
			MenuText = "&Close",
			Shortcut = Application.Instance.CommonModifier | Keys.W
		};

		private Command CmdFullScreen = new Command()
		{
			MenuText = "&Full Screen",
			Shortcut = Keys.F11
		};

		private Command CmdOpen = new Command()
		{
			MenuText = "&Open...",
			Shortcut = Application.Instance.CommonModifier | Keys.O
		};

		private Command CmdPreferences = new Command()
		{
			MenuText = "&Preferences",
			Shortcut = Application.Instance.CommonModifier | Keys.Comma
		};

		private Command CmdQuit = new Command()
		{
			MenuText = "&Quit",
			Shortcut = Application.Instance.CommonModifier | Keys.Q
		};

		private Command CmdSaveCollapsedAs = new Command()
		{
			MenuText = "&Save collapsed as...",
			Shortcut = Application.Instance.CommonModifier | Application.Instance.AlternateModifier | Keys.S
		};

		private bool IsFullscreen { get; set; } = false;
		private Point OldLocation { get; set; }
		private Size OldSize { get; set; }

		public void InitializeCommands()
		{
			Assembly assembly = Assembly.GetAssembly(typeof(MainForm));

			Version version = assembly.GetName().Version;
			int major = version.Major;
			int minor = version.Minor;
			int build = version.Build;
			int revision = version.Revision;

			CmdAbout.Executed += (sender, e) => MessageBox.Show(this,
				"Arbatel " + major + "." + minor + "\n\n" +
				"build " + build + "\n" +
				"revision " + revision);

			CmdClose.Executed += CmdClose_Executed;

			CmdFullScreen.Executed += CmdFullScreen_Executed;

			CmdOpen.Executed += CmdOpen_Executed;

			CmdPreferences.Executed += CmdPreferences_Executed;

			CmdSaveCollapsedAs.Executed += CmdSaveCollapsedAs_Executed;

			CmdQuit.Executed += (sender, e) => { Application.Instance.Quit(); };
		}

		private void CmdClose_Executed(object sender, EventArgs e)
		{
			CloseMap();
		}

		private void CmdFullScreen_Executed(object sender, EventArgs e)
		{
			if (Content is Viewport viewport)
			{
				if (viewport.Views[viewport.View].Control is View view)
				{
					if (IsFullscreen)
					{
						WindowStyle = WindowStyle.Default;

						Location = OldLocation;
						Size = OldSize;

						IsFullscreen = false;

						view.Invalidate();
					}
					else
					{
						// These need to be set before the window style is
						// changed, or they won't account for the UI chrome.
						OldLocation = Location;
						OldSize = Size;

						WindowStyle = WindowStyle.None;

						Location = (Point)Screen.Bounds.Location;
						Size = (Size)Screen.Bounds.Size;

						IsFullscreen = true;

						view.Invalidate();
					}
				}
			}
		}

		private void CmdOpen_Executed(object sender, EventArgs e)
		{
			var dlgOpenFile = new OpenFileDialog()
			{
				MultiSelect = false,
				Directory = Settings.Local.LastMapDirectory,
				Filters =
				{
					new FileFilter("Quake map", ".map"),
					new FileFilter("All files", ".*")
				},
				CurrentFilterIndex = 0
			};

			dlgOpenFile.ShowDialog(this);

			if (dlgOpenFile.FileName.Length == 0)
			{
				return;
			}

			CloseMap();

			Settings.Local.LastMapDirectory = new Uri(Path.GetDirectoryName(dlgOpenFile.FileName));

			using (FileStream stream = File.OpenRead(dlgOpenFile.FileName))
			{
				string ext = Path.GetExtension(dlgOpenFile.FileName);

				if (ext.ToLower() != ".map")
				{
					throw new InvalidDataException("Unrecognized map format!");
				}

				var definitions = new Dictionary<string, DefinitionDictionary>();

				foreach (string path in Settings.Local.DefinitionDictionaryPaths)
				{
					definitions.Add(path, Loader.LoadDefinitionDictionary(path));
				}

				Map = new QuakeMap(stream, definitions.Values.ToList().Stack());
			}

			Settings.Updatables.Add(Map);
			Settings.Save();

			BackEnd.InitTextures(Map.Textures);

			GetAllThisNonsenseReady();
		}

		private void CmdPreferences_Executed(object sender, EventArgs e)
		{
			using (var dialog = new PreferencesDialog(Settings))
			{
				dialog.ShowModal(this);
			}
		}

		private void CmdSaveCollapsedAs_Executed(object sender, EventArgs e)
		{
			var dlgSaveCollapsedAs = new SaveFileDialog()
			{
				Directory = Settings.Local.LastSaveCollapsedAsDirectory
			};

			dlgSaveCollapsedAs.ShowDialog(this);

			// User cancelled dialog.
			if (dlgSaveCollapsedAs.FileName.Length == 0)
			{
				return;
			}

			string finalPath = dlgSaveCollapsedAs.FileName;

			// If the selected file exists, use whatever filename was chosen,
			// since by this point users have already confirmed that they want
			// to overwrite that exact file. Otherwise check the extension.
			if (!File.Exists(finalPath))
			{
				if (!finalPath.EndsWith(".map"))
				{
					finalPath += ".map";
				}
			}

			using (var sw = new StreamWriter(finalPath))
			{
				sw.Write(Map.Collapse().ToString());
			}

			Settings.Local.LastSaveCollapsedAsDirectory = new Uri(Path.GetDirectoryName(dlgSaveCollapsedAs.FileName));
			Settings.Local.Save();
		}
	}
}
