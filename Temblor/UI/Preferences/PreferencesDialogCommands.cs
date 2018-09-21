using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;
using Temblor.Utilities;

namespace Temblor.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		private Command CmdAddFgd = new Command();
		private Command CmdAddWad = new Command();
		private Command CmdRemoveFgd = new Command();
		private Command CmdRemoveWad = new Command();

		private bool ShouldCommitChanges = false;

		public void InitializeCommands()
		{
			CmdAddFgd.Executed += CmdAddFgd_Executed;
			CmdAddWad.Executed += CmdAddWad_Executed;
			CmdRemoveFgd.Executed += CmdRemoveFgd_Executed;
			CmdRemoveWad.Executed += CmdRemoveWad_Executed;

			CmdOK.Executed += CmdOK_Executed;
			CmdCancel.Executed += CmdCancel_Executed;

			// The preview pane offered by Eto is useful for designing forms and
			// dialogs quickly, but it only previews code directly contained in
			// a .eto.cs file. Since these Commands are defined in a separate
			// file, the preview will show an error when attempting to assign
			// them to the Command property of a Button. As such, the only way
			// to both see a preview of the dialog in the .eto.cs file, and keep
			// code organized by splitting it into separate documents, is to
			// give each button an ID and search for it to hook up the Command.
			//
			// An unfortunate side effect is that there's no compile time check
			// for the presence of all these buttons. Forcefully checking it by
			// hand helps alleviate that. Using DebugAssertChildPresence, which
			// is a custom extension method tagged with [Conditional("DEBUG")],
			// means that Release builds won't suffer any extra overhead, and
			// doing the check here instead of in a unit test means it happens
			// every time a debugging session is started.
			this.DebugAssertChildPresence(BtnAddFgdName);
			this.DebugAssertChildPresence(BtnAddWadName);
			this.DebugAssertChildPresence(BtnRemoveFgdName);
			this.DebugAssertChildPresence(BtnRemoveWadName);

			FindChild<Button>(BtnAddFgdName).Command = CmdAddFgd;
			FindChild<Button>(BtnAddWadName).Command = CmdAddWad;
			FindChild<Button>(BtnRemoveFgdName).Command = CmdRemoveFgd;
			FindChild<Button>(BtnRemoveWadName).Command = CmdRemoveWad;
		}

		private void CmdAddFgd_Executed(object sender, EventArgs e)
		{
			var dlgAddFgd = new OpenFileDialog() { Directory = LocalSettings.LastFgdDirectory };
			dlgAddFgd.Filters.Add(new FileFilter("Quake FGD", ".fgd"));
			dlgAddFgd.Filters.Add(new FileFilter("All files", ".*"));
			dlgAddFgd.CurrentFilterIndex = 0;

			dlgAddFgd.ShowDialog(this);

			if (dlgAddFgd.FileName.Length == 0)
			{
				return;
			}

			var lbxFgd = FindChild<ListBox>(LbxFgdName);

			if (!lbxFgd.Items.Any(item => item.Text == dlgAddFgd.FileName))
			{
				lbxFgd.Items.Add(dlgAddFgd.FileName);
			}

			LocalSettings.LastFgdDirectory = new Uri(Path.GetDirectoryName(dlgAddFgd.FileName));
			LocalSettings.Save();
		}

		private void CmdAddWad_Executed(object sender, EventArgs e)
		{
			var dlgAddWad = new OpenFileDialog() { Directory = LocalSettings.LastWadDirectory };

			dlgAddWad.ShowDialog(this);

			if (dlgAddWad.FileName.Length == 0)
			{
				return;
			}

			var lbxWad = FindChild<ListBox>(LbxWadName);

			if (!lbxWad.Items.Any(item => item.Text == dlgAddWad.FileName))
			{
				lbxWad.Items.Add(dlgAddWad.FileName);
			}

			LocalSettings.LastWadDirectory = new Uri(Path.GetDirectoryName(dlgAddWad.FileName));
			LocalSettings.Save();
		}

		private void CmdCancel_Executed(object sender, EventArgs e)
		{
			ShouldCommitChanges = false;

			Close();
		}

		private void CmdOK_Executed(object sender, EventArgs e)
		{
			ShouldCommitChanges = true;

			Close();
		}

		private void CmdRemoveFgd_Executed(object sender, EventArgs e)
		{
			var lbxFgd = FindChild<ListBox>(LbxFgdName);

			if (lbxFgd.Items.Count > 0)
			{
				lbxFgd.Items.RemoveAt(lbxFgd.SelectedIndex);

				if (lbxFgd.SelectedIndex + 1 <= lbxFgd.Items.Count - 1)
				{
					lbxFgd.SelectedIndex++;
				}
			}
		}

		private void CmdRemoveWad_Executed(object sender, EventArgs e)
		{
			var lbxWad = FindChild<ListBox>(LbxWadName);

			if (lbxWad.Items.Count > 0)
			{
				lbxWad.Items.RemoveAt(lbxWad.SelectedIndex);

				if (lbxWad.SelectedIndex + 1 <= lbxWad.Items.Count - 1)
				{
					lbxWad.SelectedIndex++;
				}
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			if (ShouldCommitChanges)
			{
				CommitChanges();
			}

			base.OnClosed(e);
		}
	}
}
