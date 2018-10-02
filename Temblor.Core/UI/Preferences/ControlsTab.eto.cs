using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		private TabPage BuildControlsTab()
		{
			return new TabPage { Text = "Controls" };
		}
}
}
