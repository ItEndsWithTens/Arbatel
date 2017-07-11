using System;
using Eto.Forms;
using Eto.Drawing;
using Eto.Gl;

namespace Temblor
{
	public partial class MainForm
	{
		public MainForm()
		{
			InitializeComponent();

			var panel = new Panel() { BackgroundColor = Colors.Aquamarine };

			Content = panel;
		}
	}
}