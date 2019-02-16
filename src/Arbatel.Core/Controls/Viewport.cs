using Arbatel.Formats;
using Arbatel.Graphics;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;

namespace Arbatel.Controls
{
	public class Viewport : PixelLayout
	{
		public BackEnd BackEnd { get; set; }

		private int _view = 0;
		public int View
		{
			get { return _view; }
			set
			{
				_view = (value + Views.Count) % Views.Count;

				foreach (KeyValuePair<int, (Control Control, string Name, Action<Control> SetUp)> view in Views)
				{
					Control c = view.Value.Control;

					if (view.Key == _view)
					{
						c.Size = ClientSize;

						c.Enabled = true;
						c.Visible = true;

						view.Value.SetUp.Invoke(c);
					}
					// Some values of View use the same Control as others, with
					// only the SetUp Action differing, so it's important to not
					// hide a View if it's still in use.
					else if (!ReferenceEquals(Views[_view].Control, c))
					{
						c.Enabled = false;
						c.Visible = false;
					}
				}
			}
		}

		public Dictionary<int, (Control Control, string Name, Action<Control> SetUp)> Views { get; } = new Dictionary<int, (Control, string, Action<Control>)>();
		public Dictionary<int, Command> ViewCommands { get; } = new Dictionary<int, Command>();

		private Map _map;
		public Map Map
		{
			get { return _map; }
			set
			{
				_map = value;

				foreach (KeyValuePair<int, (Control Control, string Name, Action<Control> SetUp)> view in Views)
				{
					if (view.Value.Control is View v)
					{
						v.Map = _map;
					}
				}
			}
		}

		public Viewport(BackEnd backend)
		{
			BackEnd = backend;

			BackgroundColor = Colors.Crimson;

			var text = new TextArea()
			{
				BackgroundColor = Colors.Yellow,
				TextReplacements = TextReplacements.None,
				Enabled = false,
				Visible = false
			};
			// To avoid interrupting Tab cycling until users actually want to edit
			// something, defocus controls by default and focus this Viewport instead.
			text.EnabledChanged += (sender, e) => { Focus(); };
			text.MouseLeave += (sender, e) => { Focus(); };

			var tree = new TreeGridView()
			{
				BackgroundColor = Colors.Cyan,
				Enabled = false,
				Visible = false
			};
			tree.EnabledChanged += (sender, e) => { Focus(); };
			tree.MouseLeave += (sender, e) => { Focus(); };

			var oglView = new OpenGLView3d()
			{
				BackEnd = BackEnd,
				Enabled = false,
				Visible = false
			};

			Views.Add(0, (text, "Text", (v) => { }));
			Views.Add(1, (tree, "Tree", (v) => { }));
			Views.Add(2, (oglView, "3D Wireframe", OpenGLView.SetUpWireframe));
			Views.Add(3, (oglView, "3D Flat", OpenGLView.SetUpFlat));
			Views.Add(4, (oglView, "3D Textured", OpenGLView.SetUpTextured));

			foreach (KeyValuePair<int, (Control Control, string Name, Action<Control> SetUp)> view in Views)
			{
				Add(view.Value.Control, 0, 0);

				var command = new Command { MenuText = view.Value.Name };
				command.Executed += (sender, e) => { View = view.Key; };

				ViewCommands.Add(view.Key, command);
			}

			KeyDown += Viewport_KeyDown;
			LoadComplete += Viewport_LoadComplete;
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			Views[View].Control.Size = ClientSize;
		}

		private void Viewport_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Keys.Tab)
			{
				if (e.Modifiers == Keys.Shift)
				{
					View--;
				}
				else
				{
					View++;
				}

				e.Handled = true;
			}
		}

		private void Viewport_LoadComplete(object sender, EventArgs e)
		{
			View = 4;
		}
	}
}
