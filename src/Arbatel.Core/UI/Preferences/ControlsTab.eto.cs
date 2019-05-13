using Eto.Drawing;
using Eto.Forms;

namespace Arbatel.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		private CheckBox CbxInvertX = new CheckBox { Text = "Invert X" };
		private CheckBox CbxInvertY = new CheckBox { Text = "Invert Y" };

		private Slider SldSensitivity = new Slider
		{
			MinValue = 1,
			MaxValue = 100,
			Orientation = Orientation.Horizontal
		};

		private Slider SldMovementSpeed = new Slider
		{
			MinValue = 1,
			MaxValue = 256,
			Orientation = Orientation.Horizontal
		};

		private TextBox TxtSensitivity = new TextBox { ToolTip = "Mouse sensitivity" };
		private TextBox TxtMovementSpeed = new TextBox { ToolTip = "Movement speed in units per frame" };

		private TabPage BuildControlsTab()
		{
			SldSensitivity.Bind(s => s.Value, Settings.Local, l => l.MouseSensitivity);
			TxtSensitivity.Bind<int>("Text", SldSensitivity, "Value");

			SldMovementSpeed.Bind(s => s.Value, Settings.Roaming, r => r.MovementSpeed);
			TxtMovementSpeed.Bind<int>("Text", SldMovementSpeed, "Value");

			var tblAxes = new TableLayout(1, 2) { Spacing = MasterSpacing / 2 };
			tblAxes.Add(CbxInvertX, 0, 0);
			tblAxes.Add(CbxInvertY, 0, 1);

			var stkAxes = new StackLayout
			{
				Orientation = Orientation.Horizontal,
				VerticalContentAlignment = VerticalAlignment.Center,
				Spacing = MasterPadding,
				Items =
				{
					new Label { Text = "Axes" },
					tblAxes
				}
			};

			var tblSensitivity = new TableLayout(3, 1) { Spacing = MasterSpacing };
			tblSensitivity.Add(new Label { Text = "Sensitivity" }, 0, 0);
			tblSensitivity.Add(SldSensitivity, 1, 0);
			tblSensitivity.Add(TxtSensitivity, 2, 0);

			tblSensitivity.SetColumnScale(0, false);
			tblSensitivity.SetColumnScale(1, true);
			tblSensitivity.SetColumnScale(2, false);

			var tblMouse = new TableLayout(1, 2) { Spacing = MasterSpacing };
			tblMouse.Add(stkAxes, 0, 0);
			tblMouse.Add(tblSensitivity, 0, 1);

			tblMouse.SetRowScale(0, true);
			tblMouse.SetRowScale(1, true);

			tblMouse.SetColumnScale(0, true);

			var gbxMouse = new GroupBox { Text = "Mouse", Content = tblMouse, Padding = MasterPadding };

			var tblMovementSpeed = new TableLayout(3, 3) { Spacing = MasterSpacing };
			tblMovementSpeed.Add(new Label { Text = "Movement speed" }, 0, 1);
			tblMovementSpeed.Add(SldMovementSpeed, 1, 1);
			tblMovementSpeed.Add(TxtMovementSpeed, 2, 1);

			tblMovementSpeed.SetColumnScale(0, false);
			tblMovementSpeed.SetColumnScale(1, true);
			tblMovementSpeed.SetColumnScale(2, false);

			tblMovementSpeed.SetRowScale(0, true);
			tblMovementSpeed.SetRowScale(1, false);
			tblMovementSpeed.SetRowScale(2, true);

			var gbxKeyboard = new GroupBox { Text = "Keyboard", Content = tblMovementSpeed, Padding = MasterPadding };

			var tblGroups = new TableLayout(1, 2);
			tblGroups.Add(gbxMouse, 0, 0);
			tblGroups.Add(gbxKeyboard, 0, 1);

			tblGroups.SetRowScale(0, true);
			tblGroups.SetRowScale(1, true);

			return new TabPage { Padding = MasterPadding, Text = "Controls", Content = tblGroups };
		}
	}
}
