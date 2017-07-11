using System;
using Eto.Forms;
using Eto.Drawing;

namespace Temblor
{
	partial class MainForm : Form
	{
		void InitializeComponent()
		{
			Title = "Temblor";
			ClientSize = new Size(1280, 720);

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About" };
			aboutCommand.Executed += (sender, e) => MessageBox.Show(this, "Temblor version 0.0.0");

			// create menu
			Menu = new MenuBar
			{
				Items =
					{
					    // File submenu
					    //new ButtonMenuItem { Text = "&File", Items = { clickMe } },
					    // new ButtonMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
					    // new ButtonMenuItem { Text = "&View", Items = { /* commands/items */ } },
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