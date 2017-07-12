using System;
using Eto.Forms;
using Eto.Drawing;
using Temblor.Controls;

namespace Temblor
{
	public partial class MainForm
	{
		public MainForm()
		{
			InitializeComponent();

			var panel = new Panel() { BackgroundColor = Colors.Aquamarine };

			panel.Content = new Viewport();

			Content = panel;
		}
	}
}
