using System;
using Eto.Forms;
using Eto.Drawing;
using Arbatel.UI.Preferences;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Arbatel.UI
{
	public partial class MainForm : Form
	{
		private PreferencesDialog DlgPreferences = new PreferencesDialog();

		void InitializeComponent()
		{
			Assembly assembly = Assembly.GetAssembly(typeof(MainForm));
			var attribute = (AssemblyProductAttribute)assembly.GetCustomAttribute(typeof(AssemblyProductAttribute));
			Title = attribute.Product;

			var screen = Screen.PrimaryScreen;

			Size = (Size)(screen.Bounds.Size / 1.5f);
			Location = (Point)(screen.WorkingArea.Center - (Size / 2));

			var instanceHidden = new RadioMenuItem { Text = "Hidden" };
			var instanceTinted = new RadioMenuItem(instanceHidden) { Text = "Tinted", Checked = true };
			var instanceNormal = new RadioMenuItem(instanceHidden) { Text = "Normal" };

			Menu = new MenuBar
			{
				Items =
				{
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
				ApplicationItems =
				{
					CmdOpen
				},
				QuitItem = CmdQuit,
				AboutItem = CmdAbout
			};
		}
	}
}
