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
using Arbatel.Formats;
using Arbatel.Graphics;
using Arbatel.Utilities;
using Arbatel.Controllers;

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
				_view = value;
				ChangeView(value);
			}
		}

		public Dictionary<int, Control> Views { get; set; } = new Dictionary<int, Control>();
		public Dictionary<int, Command> ViewCommands { get; set; } = new Dictionary<int, Command>();

		private Map _map;
		public Map Map
		{
			get { return _map; }
			set
			{
				_map = value;

				foreach (var view in Views)
				{
					if (view.Value is View v)
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
				ID = "Text",
				BackgroundColor = Colors.Yellow,
				TextReplacements = TextReplacements.None,
				Enabled = false,
				Visible = false
			};
			// To avoid interrupting Tab cycling until users actually want to edit
			// something, defocus controls by default and focus this Viewport instead.
			text.Shown += (sender, e) => { Focus(); };
			text.MouseLeave += (sender, e) => { Focus(); };

			var tree = new TreeGridView()
			{
				ID = "Tree",
				BackgroundColor = Colors.Cyan,
				Enabled = false,
				Visible = false
			};
			tree.Shown += (sender, e) => { Focus(); };
			tree.MouseLeave += (sender, e) => { Focus(); };

			var wireframe = new OpenGLView3d()
			{
				ID = "3D Wireframe",
				BackEnd = BackEnd,
				ClearColor = new Color4(1.0f, 0.0f, 0.0f, 1.0f),
				ShadingStyle = ShadingStyle.Wireframe,
				Enabled = false,
				Visible = false
			};

			var flat = new OpenGLView3d()
			{
				ID = "3D Flat",
				BackEnd = BackEnd,
				ClearColor = new Color4(0.0f, 1.0f, 0.0f, 1.0f),
				ShadingStyle = ShadingStyle.Flat,
				Enabled = false,
				Visible = false
			};

			var textured = new OpenGLView3d()
			{
				ID = "3D Textured",
				BackEnd = BackEnd,
				ClearColor = new Color4(0.0f, 0.0f, 1.0f, 1.0f),
				ShadingStyle = ShadingStyle.Textured,
				Enabled = false,
				Visible = false
			};

			Views.Add(0, text);
			Views.Add(1, tree);
			Views.Add(2, wireframe);
			Views.Add(3, flat);
			Views.Add(4, textured);

			foreach (var view in Views)
			{
				Add(view.Value, 0, 0);

				var command = new Command { MenuText = view.Value.ID };
				command.Executed += (sender, e) => { View = view.Key; };

				ViewCommands.Add(view.Key, command);
			}

			KeyDown += Viewport_KeyDown;
			LoadComplete += Viewport_LoadComplete;
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			Views[View].Size = ClientSize;
		}

		private void ChangeView(int requested)
		{
			int wrapped = (requested + Views.Count) % Views.Count;

			foreach (var view in Views)
			{
				var control = view.Value;

				if (view.Key == wrapped)
				{
					control.Size = ClientSize;

					control.Enabled = true;
					control.Visible = true;
				}
				else
				{
					control.Enabled = false;
					control.Visible = false;
				}
			}
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
			}
		}

		private void Viewport_LoadComplete(object sender, EventArgs e)
		{
			View = 0;
		}
	}
}
