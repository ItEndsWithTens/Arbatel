using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbatel.UI.Preferences
{
	public partial class PreferencesDialog : Dialog
	{
		private GroupBox GbxBackgroundColors = new GroupBox { Text = "Background" };

		private Dictionary<string, Dictionary<string, Color>> NewColorSchemes = new Dictionary<string, Dictionary<string, Color>>();
		private string NewCurrentColorScheme;

		private TabPage BuildColorsTab()
		{
			GbxBackgroundColors.Padding = MasterPadding;

			RoamingSettings r = Settings.Roaming;

			NewColorSchemes.Clear();
			foreach (KeyValuePair<string, Dictionary<string, Color>> scheme in r.ColorSchemes)
			{
				NewColorSchemes.Add(scheme.Key, scheme.Value);
			}
			NewCurrentColorScheme = r.CurrentColorScheme;

			var lblScheme = new Label { Text = "Color scheme" };

			var drpCurrentScheme = new DropDown();
			foreach (string key in NewColorSchemes.Keys)
			{
				drpCurrentScheme.Items.Add(key);
			}

			drpCurrentScheme.SelectedKeyChanged += (sender, e) =>
			{
				NewCurrentColorScheme = drpCurrentScheme.SelectedKey;
			};
			drpCurrentScheme.SelectedKeyChanged += UpdateColorBox;
			drpCurrentScheme.SelectedKey = NewCurrentColorScheme;

			var btnAddScheme = new Button { Text = "Add" };
			btnAddScheme.Click += (sender, e) =>
			{
				var txtSchemeName = new TextBox { PlaceholderText = "Name" };

				var dlgName = new Dialog
				{
					Title = "New color scheme",
					Resizable = true,
					Padding = MasterPadding,
					Content = txtSchemeName
				};

				var btnOK = new Button { Text = "OK", Enabled = false };
				var btnCancel = new Button { Text = "Cancel" };

				txtSchemeName.TextChanged += (source, args) =>
				{
					btnOK.Enabled = txtSchemeName.Text.Trim().Length > 0;
				};

				bool enteredName = false;

				btnOK.Click += (source, args) =>
				{
					// The OK button can only be clicked if it's enabled, and as
					// per the anonymous method attached to TextChanged above,
					// that only happens if the TextBox has text in it.
					enteredName = true;
					dlgName.Close();
				};
				btnCancel.Click += (source, args) =>
				{
					enteredName = false;
					dlgName.Close();
				};

				dlgName.PositiveButtons.Add(btnOK);
				dlgName.NegativeButtons.Add(btnCancel);

				dlgName.ShowModal(this);

				if (enteredName)
				{
					NewColorSchemes.Add(
						txtSchemeName.Text.Trim(),
						new Dictionary<string, Color>
						{
							{ "3D Wireframe", Colors.Black },
							{ "3D Flat", Colors.Black },
							{ "3D Textured", Colors.Black }
						});

					drpCurrentScheme.Items.Clear();
					foreach (string key in NewColorSchemes.Keys)
					{
						drpCurrentScheme.Items.Add(key);
					}

					drpCurrentScheme.SelectedIndex = drpCurrentScheme.Items.Count - 1;
				}
			};

			var btnCopyScheme = new Button { Text = "Copy" };
			btnCopyScheme.Click += (sender, e) =>
			{
				NewColorSchemes.Add(
					$"{drpCurrentScheme.SelectedKey} - Copy",
					new Dictionary<string, Color>(NewColorSchemes[NewCurrentColorScheme]));

				drpCurrentScheme.Items.Clear();
				foreach (string key in NewColorSchemes.Keys)
				{
					drpCurrentScheme.Items.Add(key);
				}

				drpCurrentScheme.SelectedIndex = drpCurrentScheme.Items.Count - 1;
			};

			var btnRenameScheme = new Button { Text = "Rename" };
			btnRenameScheme.Click += (sender, e) =>
			{
				var txtNewName = new TextBox { PlaceholderText = "New color scheme name" };

				var dlgName = new Dialog
				{
					Title = "New name",
					Resizable = true,
					Padding = MasterPadding,
					Content = txtNewName
				};

				var btnOK = new Button { Text = "OK", Enabled = false };
				var btnCancel = new Button { Text = "Cancel" };

				txtNewName.TextChanged += (source, args) =>
				{
					btnOK.Enabled = txtNewName.Text.Trim().Length > 0;
				};

				bool enteredName = false;

				btnOK.Click += (source, args) =>
				{
					// The OK button can only be clicked if it's enabled, and as
					// per the anonymous method attached to TextChanged above,
					// that only happens if the TextBox has text in it.
					enteredName = true;
					dlgName.Close();
				};
				btnCancel.Click += (source, args) =>
				{
					enteredName = false;
					dlgName.Close();
				};

				dlgName.PositiveButtons.Add(btnOK);
				dlgName.NegativeButtons.Add(btnCancel);

				dlgName.ShowModal(this);

				if (enteredName)
				{
					Dictionary<string, Color> scheme = NewColorSchemes[drpCurrentScheme.SelectedKey];

					int index = 0;
					for (int i = 0; i < NewColorSchemes.Count; i++)
					{
						if (NewColorSchemes.ElementAt(i).Key == drpCurrentScheme.SelectedKey)
						{
							index = i;
							break;
						}
					}

					NewColorSchemes.Remove(drpCurrentScheme.SelectedKey);
					NewColorSchemes.Add(txtNewName.Text.Trim(), scheme);

					drpCurrentScheme.Items.Clear();
					foreach (string key in NewColorSchemes.Keys)
					{
						drpCurrentScheme.Items.Add(key);
					}

					drpCurrentScheme.SelectedIndex = drpCurrentScheme.Items.Count - 1;
				}
			};

			var btnRemoveScheme = new Button { Text = "Remove" };
			btnRemoveScheme.Click += (sender, e) =>
			{
				if (NewColorSchemes.Count == 0)
				{
					return;
				}

				int newIndex = 0;
				for (int i = 0; i < NewColorSchemes.Count; i++)
				{
					KeyValuePair<string, Dictionary<string, Color>> scheme = NewColorSchemes.ElementAt(i);

					if (scheme.Key == drpCurrentScheme.SelectedKey)
					{
						newIndex = i - 1;
						break;
					}
				}

				NewColorSchemes.Remove(drpCurrentScheme.SelectedKey);

				if (NewColorSchemes.Count == 0)
				{
					newIndex = -1;
				}
				else if (newIndex < 0)
				{
					newIndex = 0;
				}

				drpCurrentScheme.Items.Clear();
				foreach (string key in NewColorSchemes.Keys)
				{
					drpCurrentScheme.Items.Add(key);
				}

				drpCurrentScheme.SelectedIndex = newIndex;

				NewCurrentColorScheme = drpCurrentScheme.SelectedKey;

				UpdateColorBox(drpCurrentScheme, EventArgs.Empty);
			};

			var stkSchemes = new StackLayout
			{
				Orientation = Orientation.Horizontal,
				VerticalContentAlignment = VerticalAlignment.Center,
				Spacing = MasterPadding,
				Items =
				{
					lblScheme,
					drpCurrentScheme,
					btnAddScheme,
					btnCopyScheme,
					btnRenameScheme,
					btnRemoveScheme
				}
			};

			var stkColors = new StackLayout
			{
				Orientation = Orientation.Vertical,
				Spacing = MasterPadding,
				Items =
				{
					stkSchemes,
					GbxBackgroundColors
				}
			};

			return new TabPage { Padding = MasterPadding, Text = "Colors", Content = stkColors };
		}

		private void UpdateColorBox(object sender, EventArgs e)
		{
			RoamingSettings r = Settings.Roaming;

			var drpCurrentScheme = (DropDown)sender;

			GbxBackgroundColors.Content = null;

			if (drpCurrentScheme.SelectedIndex == -1)
			{
				return;
			}

			Dictionary<string, Color> scheme = NewColorSchemes[drpCurrentScheme.SelectedKey];

			var tblColors = new TableLayout(2, scheme.Count)
			{
				Spacing = MasterSpacing
			};

			for (int i = 0; i < scheme.Count; i++)
			{
				KeyValuePair<string, Color> color = scheme.ElementAt(i);

				var label = new Label { Text = color.Key };
				var picker = new ColorPicker { Value = color.Value, AllowAlpha = false };

				// True data binding isn't necessary here, since the color
				// collection won't change behind the scenes while the
				// preferences dialog is open. It's also impossible, since
				// said collection is currently not an ObservableCollection.
				picker.ValueChanged += (source, args) =>
				{
					scheme[color.Key] = picker.Value;
				};

				tblColors.Add(label, 0, i);
				tblColors.Add(picker, 1, i);
			}

			GbxBackgroundColors.Content = tblColors;
		}
	}
}
