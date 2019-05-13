using Eto.Drawing;
using Eto.Forms;

namespace Arbatel.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		const string BtnFgdCombineStackName = "btnFgdCombineStack";
		const string BtnFgdCombineBlendName = "btnFgdCombineBlend";
		const string BtnOKName = "btnOK";
		const string BtnCancelName = "btnCancel";
		const string LbxFgdName = "lbxFgd";
		const string LbxWadName = "lbxWad";

		const int DefaultPadding = 10;
		private int MasterPadding = DefaultPadding;
		private Size MasterSpacing = new Size(DefaultPadding, DefaultPadding);

		private Command CmdAddFgd = new Command();
		private Command CmdAddWad = new Command();
		private Command CmdRemoveFgd = new Command();
		private Command CmdRemoveWad = new Command();
		private Command CmdOK = new Command();
		private Command CmdCancel = new Command();

		void InitializeComponent()
		{
			Title = "Preferences";
			Padding = MasterPadding;
			Resizable = true;

			Width = (int)(Screen.PrimaryScreen.Bounds.Width / 3.25f);
			Height = (int)(Screen.PrimaryScreen.Bounds.Height / 2.5f);

			Content = new TabControl
			{
				BackgroundColor = SystemColors.ControlBackground,
				Pages =
				{
					BuildControlsTab(),
					BuildDefinitionsTab(),
					BuildTexturesTab(),
					BuildColorsTab()
				}
			};

			PositiveButtons.Add(new Button { Text = "OK", ID = BtnOKName, Command = CmdOK });
			NegativeButtons.Add(new Button { Text = "Cancel", ID = BtnCancelName, Command = CmdCancel });
		}
	}
}
