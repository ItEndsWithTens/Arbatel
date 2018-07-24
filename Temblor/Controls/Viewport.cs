using Eto;
using Eto.Drawing;
using Eto.Forms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;
using Temblor.Utilities;

namespace Temblor.Controls
{
	public class Viewport : PixelLayout
	{
		private Dictionary<int, string> _viewNames = new Dictionary<int, string>
		{
			{ 0, "Text" },
			{ 1, "Tree" },
			{ 2, "3D Wireframe" },
			{ 3, "3D Flat" },
			{ 4, "3D Textured" }
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

				RemoveAll();

				DropDown.SelectedValue = _viewNames[_view];
				Add(control, new Point(0, 0));
				Add(DropDown, new Point(0, 0));

				// Can't do this before using Add, since setting the Size leads
				// to the View's Refresh method being called, which expects to
				// have a Parent in order to do its work. The View won't have
				// said Parent until it's added to the Viewport.
				control.Size = ClientSize;
			}
		}

		public Dictionary<int, Control> Views = new Dictionary<int, Control>();

		public DropDown DropDown = new DropDown() { BackgroundColor = Colors.Lavender };
		public Label Label = new Label() { BackgroundColor = Colors.Black, TextColor = Colors.White };

		private TextArea _viewText = new TextArea() { BackgroundColor = Colors.Yellow, TextReplacements = TextReplacements.None };
		private TreeGridView _viewTree = new TreeGridView() { BackgroundColor = Colors.Cyan };
		private View3d _view3dWire;
		private View3d _view3dFlat;
		private View3d _view3dTex;

		private Map _map;
		public Map Map
		{
			get { return _map; }
			set
			{
				_map = value;

				foreach (var view in Views)
				{
					if (view.Value is View)
					{
						(view.Value as View).Map = _map;
					}
				}
			}
		}

		public Viewport()
		{
			BackgroundColor = Colors.Crimson;

			Label.Text = _viewNames[View];
			DropDown.DataStore = _viewNames.Values;
			DropDown.SelectedValueChanged += (sender, e) => { View = DropDown.SelectedIndex; };

			// To avoid interrupting Tab cycling until users actually want to edit
			// something, defocus controls by default and focus this Viewport instead.
			_viewText.Shown += (sender, e) => { Focus(); };
			_viewText.MouseLeave += (sender, e) => { Focus(); };

			_viewTree.Shown += (sender, e) => { Focus(); };
			_viewTree.MouseLeave += (sender, e) => { Focus(); };

			_view3dWire = new View3d() { ClearColor = new Color4(1.0f, 0.0f, 0.0f, 1.0f), PolygonMode = PolygonMode.Line };
			_view3dFlat = new View3d() { ClearColor = new Color4(0.0f, 1.0f, 0.0f, 1.0f) };
			_view3dTex = new View3d() { ClearColor = new Color4(0.0f, 0.0f, 1.0f, 1.0f) };

			Views.Add(0, _viewText);
			Views.Add(1, _viewTree);
			Views.Add(2, _view3dWire);
			Views.Add(3, _view3dFlat);
			Views.Add(4, _view3dTex);

			LoadComplete += Viewport_LoadComplete;
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			Views[View].Size = ClientSize;
		}

		private void Viewport_LoadComplete(object sender, EventArgs e)
		{
			View = 0;
		}
	}
}
