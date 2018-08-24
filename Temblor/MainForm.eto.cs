using System;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;

namespace Temblor
{
	partial class MainForm : Form
	{
		void InitializeComponent()
		{
			Title = "Temblor";
			ClientSize = new Size(Screen.PrimaryScreen.WorkingArea.Size / 1.5f);

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About" };

			Assembly assembly = Assembly.GetAssembly(typeof(Temblor.MainForm));

			Version version = assembly.GetName().Version;
			int major = version.Major;
			int minor = version.Minor;
			int build = version.Build;
			int revision = version.Revision;

			aboutCommand.Executed += (sender, e) => MessageBox.Show(this,
				"Temblor " + major + "." + minor + "\n\n" +
				"build " + build + "\n" +
				"revision " + revision);

			// create menu
			Menu = new MenuBar
			{
				Items =
					{
					    // File submenu
					    new ButtonMenuItem { Text = "&File", Items = { } },
					    // new ButtonMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
					    // new ButtonMenuItem { Text = "&View", Items = { /* commands/items */ } },
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
										new CheckMenuItem { Text = "Hidden", Checked = false },
										new CheckMenuItem { Text = "Tinted", Checked = true },
										new CheckMenuItem { Text = "Normal", Checked = false  }
									}
								},
								new SeparatorMenuItem { },
								new ButtonMenuItem { Text ="Save collapsed as..." }
							}
						}
				    },
				ApplicationItems =
					{
					    // application (OS X) or file menu (others)
					    //new ButtonMenuItem { Text = "&Preferences..." },
					},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};
		}
	}
}