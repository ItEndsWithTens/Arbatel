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

			Content = new TabControl
			{
				BackgroundColor = SystemColors.ControlBackground,
				Pages =
				{
					BuildControlsTab(),
					BuildDefinitionsTab(),
					BuildTexturesTab()
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
