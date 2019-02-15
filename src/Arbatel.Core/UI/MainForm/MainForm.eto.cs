using Arbatel.UI.Preferences;
using Eto.Drawing;
using Eto.Forms;
using System.Reflection;

namespace Arbatel.UI
{
	public partial class MainForm : Form
	{
		void InitializeComponent()
		{
			Assembly assembly = Assembly.GetAssembly(typeof(MainForm));
			var attribute = (AssemblyProductAttribute)assembly.GetCustomAttribute(typeof(AssemblyProductAttribute));
			Title = attribute.Product;

			Screen screen = Screen.PrimaryScreen;

			Size = (Size)(screen.Bounds.Size / 1.5f);
			Location = (Point)(screen.WorkingArea.Center - (Size / 2));

			var instanceHidden = new RadioMenuItem { Text = "Hidden", Command = CmdShowInstancesHidden };
			var instanceTinted = new RadioMenuItem(instanceHidden) { Text = "Tinted", Command = CmdShowInstancesTinted, Checked = true };
			var instanceNormal = new RadioMenuItem(instanceHidden) { Text = "Normal", Command = CmdShowInstancesNormal };

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
									instanceHidden,
									instanceTinted,
									instanceNormal
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
