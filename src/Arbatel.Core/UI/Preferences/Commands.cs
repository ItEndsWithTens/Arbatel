using Eto.Forms;
using System;
using System.IO;
using System.Linq;

namespace Arbatel.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		private bool ShouldCommitChanges = false;

		public void InitializeCommands()
		{
			CmdAddFgd.Executed += CmdAddFgd_Executed;
			CmdRemoveFgd.Executed += CmdRemoveFgd_Executed;

			CmdAddWad.Executed += CmdAddWad_Executed;
			CmdRemoveWad.Executed += CmdRemoveWad_Executed;

			CmdOK.Executed += CmdOK_Executed;
			CmdCancel.Executed += CmdCancel_Executed;
		}

		private void CmdAddFgd_Executed(object sender, EventArgs e)
		{
			var dlgAddFgd = new OpenFileDialog
			{
				Directory = Settings.Local.LastFgdDirectory,
				Filters =
				{
					new FileFilter("Quake FGD", ".fgd"),
					new FileFilter("All files", ".*")
				},
				CurrentFilterIndex = 0,
				CheckFileExists = true
			};

			DialogResult result = dlgAddFgd.ShowDialog(this);

			if (result == DialogResult.Cancel)
			{
				return;
			}

			ListBox lbxFgd = FindChild<ListBox>(LbxFgdName);

			if (!lbxFgd.Items.Any(item => item.Text == dlgAddFgd.FileName))
			{
				lbxFgd.Items.Add(dlgAddFgd.FileName);
			}

			Settings.Local.LastFgdDirectory = new Uri(Path.GetDirectoryName(dlgAddFgd.FileName));
		}

		private void CmdAddWad_Executed(object sender, EventArgs e)
		{
			var dlgAddWad = new OpenFileDialog
			{
				Directory = Settings.Local.LastWadDirectory,
				Filters =
				{
					new FileFilter("Quake WAD", ".wad"),
					new FileFilter("All files", ".*")
				},
				CurrentFilterIndex = 0,
				CheckFileExists = true
			};

			DialogResult result = dlgAddWad.ShowDialog(this);

			if (result == DialogResult.Cancel)
			{
				return;
			}

			ListBox lbxWad = FindChild<ListBox>(LbxWadName);

			if (!lbxWad.Items.Any(item => item.Text == dlgAddWad.FileName))
			{
				lbxWad.Items.Add(dlgAddWad.FileName);
			}

			Settings.Local.LastWadDirectory = new Uri(Path.GetDirectoryName(dlgAddWad.FileName));
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
			ListBox lbxFgd = FindChild<ListBox>(LbxFgdName);

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
			ListBox lbxWad = FindChild<ListBox>(LbxWadName);

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
