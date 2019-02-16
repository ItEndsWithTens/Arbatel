using Eto.Drawing;
using Eto.Forms;

namespace Arbatel.UI
{
	public partial class MainForm : Form
	{
		private RadioMenuItem rdoInstanceHidden;
		private RadioMenuItem rdoInstanceTinted;
		private RadioMenuItem rdoInstanceNormal;

		void InitializeComponent()
		{
			Title = Core.Name;

			Screen screen = Screen.PrimaryScreen;

			Size = (Size)(screen.Bounds.Size / 1.5f);
			Location = (Point)(screen.WorkingArea.Center - (Size / 2));

			rdoInstanceHidden = new RadioMenuItem { Text = "Hidden", Command = CmdShowInstancesHidden };
			rdoInstanceTinted = new RadioMenuItem(rdoInstanceHidden) { Text = "Tinted", Command = CmdShowInstancesTinted, Checked = true };
			rdoInstanceNormal = new RadioMenuItem(rdoInstanceHidden) { Text = "Normal", Command = CmdShowInstancesNormal };

			Menu = new MenuBar
			{
				Items =
				{
					new ButtonMenuItem { Text = "&File", Items = { CmdOpen, CmdClose } },
					new ButtonMenuItem { Text = "&Edit", Items = { CmdPreferences } },
					new ButtonMenuItem { Text = "&View", Items = { CmdFullScreen } },
					new ButtonMenuItem
					{
						Text = "&Instancing",
						Items =
						{
							new ButtonMenuItem
							{
								Text = "View",
								Items =
								{
									rdoInstanceHidden,
									rdoInstanceTinted,
									rdoInstanceNormal
								}
							},
							new SeparatorMenuItem { },
							CmdSaveCollapsedAs
						}
					}
				},
				QuitItem = CmdQuit,
				AboutItem = CmdAbout
			};
		}
	}
}
