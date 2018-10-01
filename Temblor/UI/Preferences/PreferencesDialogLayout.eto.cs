using System;
using Eto;
using Eto.Forms;
using Eto.Drawing;

namespace Temblor.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		private int MasterPadding = 10;

		private Command CmdAddFgd = new Command();
		private Command CmdAddWad = new Command();
		private Command CmdRemoveFgd = new Command();
		private Command CmdRemoveWad = new Command();

		public string BtnFgdCombineStackName = "btnFgdCombineStack";
		public string BtnFgdCombineBlendName = "btnFgdCombineBlend";

		private string LbxFgdName = "lbxFgd";
		private string LbxWadName = "lbxWad";

		public Command CmdOK = new Command();
		public Command CmdCancel = new Command();

		private string BtnOKName = "btnOK";
		private string BtnCancelName = "btnCancel";

		void InitializeComponent()
		{
			Title = "Preferences";
			Padding = MasterPadding;
			Resizable = true;

			var lbxFgd = new ListBox() { AllowDrop = true, ID = LbxFgdName };

			var btnAddFgd = new Button { Text = "Add...", Command = CmdAddFgd };
			var btnRemoveFgd = new Button { Text = "Remove", Command = CmdRemoveFgd };

			var btnFgdCombineStack = new RadioButton { Text = "Stack", Checked = true, ID = BtnFgdCombineStackName };
			var btnFgdCombineBlend = new RadioButton(btnFgdCombineStack) { Text = "Blend", ID = BtnFgdCombineBlendName };

			var layoutFgdAddRemove = new TableLayout(2, 1)
			{
				Spacing = new Size(MasterPadding, 0)
			};

			layoutFgdAddRemove.Add(btnAddFgd, 0, 0);
			layoutFgdAddRemove.Add(btnRemoveFgd, 1, 0);

			var tblFgdCombine = new TableLayout(2, 1)
			{
				Spacing = new Size(MasterPadding, 0)
			};

			tblFgdCombine.Add(btnFgdCombineStack, 0, 0);
			tblFgdCombine.Add(btnFgdCombineBlend, 1, 0);

			var stkFgd = new StackLayout
			{
				Spacing = MasterPadding,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				Items =
				{
					layoutFgdAddRemove,
					tblFgdCombine
				}
			};

			var tblFgd = new TableLayout(1, 2) { Spacing = new Size(0, MasterPadding) };

			tblFgd.Add(lbxFgd, 0, 0);
			tblFgd.Add(stkFgd, 0, 1);

			tblFgd.SetRowScale(0, true);
			tblFgd.SetRowScale(1, false);

			var gbxFgd = new GroupBox
			{
				Padding = new Padding(MasterPadding, MasterPadding, MasterPadding, 0),
				Text = "Entity definition files",
				Content = tblFgd,
				ID = "gbxFgd"
			};

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
					LocalSettings.UsingCustomPalette = false;
				}
				else
				{
					drpPalette.Enabled = false;
					LocalSettings.UsingCustomPalette = true;
				}
			};

			var btnCustomPalette = new RadioButton(btnBuiltInPalette) { Text = "Custom", ID = "btnCustomPalette" };
			btnCustomPalette.CheckedChanged += (sender, e) => 
			{
				var fpkPalette = FindChild<FilePicker>("fpkPalette");

				if (btnCustomPalette.Checked)
				{
					fpkPalette.Enabled = true;
					LocalSettings.UsingCustomPalette = true;
				}
				else
				{
					fpkPalette.Enabled = false;
					LocalSettings.UsingCustomPalette = false;
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

			var gbxWad = new GroupBox
			{
				Padding = new Padding(MasterPadding, MasterPadding, MasterPadding, 0),
				Text = "Texture collections",
				Content = tblWad,
				ID = "gbxWad"
			};

			var tblMaster = new TableLayout(1, 2);

			tblMaster.Add(gbxFgd, 0, 0);
			tblMaster.Add(gbxWad, 0, 1);

			tblMaster.SetRowScale(0, true);
			tblMaster.SetRowScale(1, true);

			Content = new TabControl
			{
				BackgroundColor = SystemColors.ControlBackground,
				Pages =
				{
					new TabPage { Padding = MasterPadding, Text = "Resources", Content = tblMaster },
					new TabPage { Text = "Controls" }
				}
			};

			PositiveButtons.Add(new Button { Text = "OK", ID = BtnOKName, Command = CmdOK });
			NegativeButtons.Add(new Button { Text = "Cancel", ID = BtnCancelName, Command = CmdCancel });
		}

		protected override void OnLoadComplete(EventArgs e)
		{
			base.OnLoadComplete(e);

			// Once the dialog finishes loading, its automatically defined size
			// is only enough to hold its controls. The dialog will presently be
			// resized for aesthetic reasons, but this makes a good minimum.
			MinimumSize = Size;

			Width = (int)(Screen.PrimaryScreen.Bounds.Width / 4.5f);
			Height = (int)(Screen.PrimaryScreen.Bounds.Height / 2.5f);
		}
	}
}
