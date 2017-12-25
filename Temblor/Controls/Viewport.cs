using Eto;
using Eto.Drawing;
using Eto.Forms;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Controls
{
	public class Viewport : PixelLayout
	{
		public Label Label = new Label() { BackgroundColor = Colors.Black, TextColor = Colors.White };

		public DropDown DropDown = new DropDown() { BackgroundColor = Colors.Lavender };

		private Dictionary<int, string> _viewNames = new Dictionary<int, string>
		{
			{ 0, "Tree" },
			{ 1, "3D Wireframe" },
			{ 2, "3D Flat" },
			{ 3, "3D Textured" }
		};

		public string ViewName
		{
			get { return _viewNames[_view]; }
		}

		private int _view = 0;
		public int View
		{
			get { return _view; }
			set
			{
				_view = (value + _viewNames.Count) % _viewNames.Count;

				var control = Views[_view];
				control.Size = ClientSize;

				RemoveAll();

				DropDown.SelectedValue = _viewNames[_view];
				Add(control, new Point(0, 0));
				Add(DropDown, new Point(0, 0));
			}
		}

		public Dictionary<int, Control> Views = new Dictionary<int, Control>();

		private TreeGridView _modeTree = new TreeGridView() { BackgroundColor = Colors.Cyan };
		private Mode3d _mode3dWire = new Mode3d() { ClearColor = new Color4(1.0f, 0.0f, 0.0f, 1.0f) };
		private Mode3d _mode3dFlat = new Mode3d() { ClearColor = new Color4(0.0f, 1.0f, 0.0f, 1.0f) };
		private Mode3d _mode3dTex = new Mode3d() { ClearColor = new Color4(0.0f, 0.0f, 1.0f, 1.0f) };

		public Viewport()
		{
			BackgroundColor = Colors.Crimson;

			Label.Text = _viewNames[View];
			DropDown.DataStore = _viewNames.Values;
			DropDown.SelectedValueChanged += (sender, e) => { View = DropDown.SelectedIndex; };

			Views.Add(0, _modeTree);
			Views.Add(1, _mode3dWire);
			Views.Add(2, _mode3dFlat);
			Views.Add(3, _mode3dTex);

			LoadComplete += Viewport_LoadComplete;
		}

		private void Viewport_LoadComplete(object sender, EventArgs e)
		{
			View = 0;
		}
	}
}
