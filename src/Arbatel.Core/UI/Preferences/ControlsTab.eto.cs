using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbatel.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		private CheckBox CbxInvertX = new CheckBox() { Text = "Invert X" };
		private CheckBox CbxInvertY = new CheckBox() { Text = "Invert Y" };

		private TabPage BuildControlsTab()
		{
			var tblInvert = new TableLayout(1, 2) { };

			tblInvert.Add(CbxInvertX, 0, 0);
			tblInvert.Add(CbxInvertY, 0, 1);

			var tblMouse = new TableLayout(3, 3);

			tblMouse.Add(tblInvert, 1, 1);

			tblMouse.SetRowScale(0, true);
			tblMouse.SetRowScale(1, false);
			tblMouse.SetRowScale(2, true);

			tblMouse.SetColumnScale(0, true);
			tblMouse.SetColumnScale(1, false);
			tblMouse.SetColumnScale(2, true);

			var gbxMouse = new GroupBox() { Text = "Mouse", Content = tblMouse, Padding = MasterPadding };

			return new TabPage { Padding = MasterPadding, Text = "Controls", Content = gbxMouse };
		}
	}
}
