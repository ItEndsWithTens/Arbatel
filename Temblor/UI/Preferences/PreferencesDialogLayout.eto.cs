using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.ObjectModel;

namespace Temblor.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		private int MasterPadding = 10;

		private string BtnAddFgdName = "btnAddFgd";
		private string BtnAddWadName = "btnAddWad";
		private string BtnRemoveFgdName = "btnRemoveFgd";
		private string BtnRemoveWadName = "btnRemoveWad";

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

			var btnAddFgd = new Button { Text = "Add...", ID = BtnAddFgdName };
			var btnRemoveFgd = new Button { Text = "Remove", ID = BtnRemoveFgdName };

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

			var btnAddWad = new Button { Text = "Add...", ID = BtnAddWadName };
			var btnRemoveWad = new Button { Text = "Remove", ID = BtnRemoveWadName };

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

			var tblWad = new TableLayout(1, 2) { Spacing = new Size(0, MasterPadding) };

			tblWad.Add(lbxWad, 0, 0);
			tblWad.Add(stkWad, 0, 1);

			tblWad.SetRowScale(0, true);
			tblWad.SetRowScale(1, false);

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

			Content = tblMaster;

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
