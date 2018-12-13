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
		private TabPage BuildDefinitionsTab()
		{
			var lbxFgd = new ListBox() { AllowDrop = true, ID = LbxFgdName };

			var btnAddFgd = new Button { Text = "Add...", Command = CmdAddFgd };
			var btnRemoveFgd = new Button { Text = "Remove", Command = CmdRemoveFgd };

			var btnFgdCombineStack = new RadioButton { Text = "Stack", Checked = true, ID = BtnFgdCombineStackName };
			var btnFgdCombineBlend = new RadioButton(btnFgdCombineStack) { Text = "Blend", ID = BtnFgdCombineBlendName };

			var layoutFgdAddRemove = new TableLayout(2, 1)
			{
				Spacing = new Size(MasterPadding, 0)
			};

			layoutFgdAddRemove.Add(btnAddFgd, 0, 0);
			layoutFgdAddRemove.Add(btnRemoveFgd, 1, 0);

			var tblFgdCombine = new TableLayout(2, 1)
			{
				Spacing = new Size(MasterPadding, 0)
			};

			tblFgdCombine.Add(btnFgdCombineStack, 0, 0);
			tblFgdCombine.Add(btnFgdCombineBlend, 1, 0);

			var stkFgd = new StackLayout
			{
				Spacing = MasterPadding,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				Items =
				{
					layoutFgdAddRemove,
					tblFgdCombine
				}
			};

			var tblFgd = new TableLayout(1, 2) { Spacing = new Size(0, MasterPadding) };

			tblFgd.Add(lbxFgd, 0, 0);
			tblFgd.Add(stkFgd, 0, 1);

			tblFgd.SetRowScale(0, true);
			tblFgd.SetRowScale(1, false);

			return new TabPage { Padding = MasterPadding, Text = "Definitions", Content = tblFgd };
		}
	}
}
