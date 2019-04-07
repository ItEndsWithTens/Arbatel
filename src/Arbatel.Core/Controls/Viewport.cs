using Arbatel.Formats;
using Arbatel.Graphics;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbatel.Controls
{
	public class Viewport : PixelLayout
	{
		public static int DefaultView { get; set; } = 4;

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

						// Eto 2.4.1's MacView class, the basis for a number of
						// controls on the Mac platforms, doesn't support the
						// EnabledChanged event. This is a suitable workaround.
						if (c is View v)
						{
							v.Focus();
						}
						else
						{
							// To avoid interrupting Tab cycling until users
							// actually want to edit something, defocus controls
							// by default and focus this Viewport instead.
							Focus();
						}
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
			text.MouseLeave += (sender, e) =>
			{
				if (text.Enabled)
				{
					Focus();
				}
			};

			var tree = new TreeGridView()
			{
				BackgroundColor = Colors.Cyan,
				Enabled = false,
				Visible = false
			};
			tree.MouseLeave += (sender, e) =>
			{
				if (tree.Enabled)
				{
					Focus();
				}
			};

			Views.Add(0, (text, "Text", (v) => { }));
			Views.Add(1, (tree, "Tree", (v) => { }));

			UpdateViews();

			// Prepare the default View for graphics API initialization. This
			// won't prepare it for input, i.e. focus it; see MainForm for that.
			LoadComplete += (sender, e) => View = DefaultView;

			SizeChanged += (sender, e) => Views[View].Control.Size = ClientSize;
		}

		protected void UpdateViews()
		{
			foreach (KeyValuePair<int, (Control Control, string Name, Action<Control> SetUp)> view in Views)
			{
				if (!Controls.Contains(view.Value.Control))
				{
					Add(view.Value.Control, 0, 0);
				}

				var command = new Command { MenuText = view.Value.Name };
				command.Executed += (sender, e) => { View = view.Key; };

				ViewCommands[view.Key] = command;
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

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
	}
}
