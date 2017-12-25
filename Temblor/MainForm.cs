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

			var viewport = new Viewport() { ID = "topLeft" };

			Content = viewport;

			KeyDown += MainForm_KeyDown;
		}

		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Keys.Tab)
			{
				var viewport = FindChild("topLeft") as Viewport;

				if (e.Modifiers == Keys.Shift)
				{
					viewport.View--;
				}
				else
				{
					viewport.View++;
				}
			}
		}
	}
}
