using Eto;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbatel.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		private TabPage BuildTexturesTab()
		{
			var lbxWad = new ListBox() { AllowDrop = true, ID = LbxWadName };

			var btnAddWad = new Button { Text = "Add...", Command = CmdAddWad };
			var btnRemoveWad = new Button { Text = "Remove", Command = CmdRemoveWad };

			var tblWadAddRemove = new TableLayout(2, 1)
			{
				Spacing = new Size(MasterPadding, 0)
			};

			tblWadAddRemove.Add(btnAddWad, 0, 0);
			tblWadAddRemove.Add(btnRemoveWad, 1, 0);

			var stkWad = new StackLayout
			{
				Spacing = MasterPadding,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				Items =
				{
					tblWadAddRemove
				}
			};

			var btnBuiltInPalette = new RadioButton { Text = "Built-in", Checked = true, ID = "btnBuiltInPalette" };
			btnBuiltInPalette.CheckedChanged += (sender, e) =>
			{
				var drpPalette = FindChild<DropDown>("drpPalette");

				if (btnBuiltInPalette.Checked)
				{
					drpPalette.Enabled = true;
				}
				else
				{
					drpPalette.Enabled = false;
				}
			};

			var btnCustomPalette = new RadioButton(btnBuiltInPalette) { Text = "Custom", ID = "btnCustomPalette" };
			btnCustomPalette.CheckedChanged += (sender, e) =>
			{
				var fpkPalette = FindChild<FilePicker>("fpkPalette");

				if (btnCustomPalette.Checked)
				{
					fpkPalette.Enabled = true;
				}
				else
				{
					fpkPalette.Enabled = false;
				}
			};

			var tblBuiltInPalette = new TableLayout(2, 1)
			{
				Spacing = new Size(MasterPadding, 0)
			};
			tblBuiltInPalette.Add(btnBuiltInPalette, 0, 0);
			tblBuiltInPalette.Add(new DropDown() { Items = { "Quake" }, SelectedIndex = 0, ID = "drpPalette" }, 1, 0);

			var tblCustomPalette = new TableLayout(2, 1)
			{
				Spacing = new Size(MasterPadding, 0)
			};
			tblCustomPalette.Add(btnCustomPalette, 0, 0);
			tblCustomPalette.Add(new FilePicker() { FileAction = FileAction.OpenFile, Enabled = false, ID = "fpkPalette" }, 1, 0);

			var stkPalette = new StackLayout
			{
				Orientation = Orientation.Vertical,
				Spacing = MasterPadding,
				Items =
				{
					new Label {Text = "Palette:" },
					tblBuiltInPalette,
					tblCustomPalette
				}
			};

			var tblWad = new TableLayout(1, 3) { Spacing = new Size(0, MasterPadding) };

			tblWad.Add(lbxWad, 0, 0);
			tblWad.Add(stkWad, 0, 1);
			tblWad.Add(stkPalette, 0, 2);

			tblWad.SetRowScale(0, true);
			tblWad.SetRowScale(1, false);
			tblWad.SetRowScale(2, false);

			return new TabPage { Padding = MasterPadding, Text = "Textures", Content = tblWad };
		}
	}
}
