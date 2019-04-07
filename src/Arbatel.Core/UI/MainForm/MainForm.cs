using Arbatel.Controls;
using Arbatel.Formats;
using Arbatel.Graphics;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbatel.UI
{
	public partial class MainForm
	{
		/// <summary>
		/// The graphics backend used by this application.
		/// </summary>
		public BackEnd BackEnd { get; private set; }

		private Map _map;
		/// <summary>
		/// The Map currently loaded in this form.
		/// </summary>
		public Map Map
		{
			get { return _map; }
			set
			{
				_map = value;

				(Content as Viewport).Map = _map;
			}
		}

		public Settings Settings { get; } = new Settings();

		public MainForm()
		{
			InitializeComponent();

			InitializeCommands();

			// In the long term, the goal is to use Veldrid's implementation of
			// all its supported graphics APIs. For now, though, Arbatel's code
			// doesn't run as well (or as correctly) in Veldrid's OpenGL as it
			// does in straight OpenTK, so skip Veldrid in the case of OpenGL.
			//
			// VeldridSurface does have provisions for OpenGL support, mind you,
			// and an option for users to manually specify what API they want is
			// coming to the settings dialog box sooner or later.
			bool veldrid = VeldridSurface.PreferredBackend != Veldrid.GraphicsBackend.OpenGL;

			Viewport viewport;
			if (veldrid)
			{
				viewport = new VeldridViewport { ID = "viewport" };
			}
			else
			{
				viewport = new OpenGLViewport { ID = "viewport" };
			}

			BackEnd = viewport.BackEnd;

			foreach ((Control Control, string Name, Action<View> SetUp) view in viewport.Views.Values)
			{
				if (view.Control is View v)
				{
					Settings.Updatables.Add(v);
				}
			}

			ButtonMenuItem viewMenu = Menu.Items.GetSubmenu("View");
			foreach (KeyValuePair<int, Command> command in viewport.ViewCommands)
			{
				viewMenu.Items.Insert(command.Key, command.Value);
			}
			viewMenu.Items.Insert(viewport.ViewCommands.Count, new SeparatorMenuItem());

			Content = viewport;

			Shown += SetDefaultView;
		}

		private void CloseMap()
		{
			if (Map == null)
			{
				return;
			}

			IEnumerable<View> views =
				from view in (Content as Viewport).Views
				where view.Value.Control is View
				select view.Value.Control as View;

			BackEnd.DeleteMap(Map, views);

			Settings.Updatables.Remove(Map);

			Map = null;
		}

		private void GetAllThisNonsenseReady()
		{
			var viewport = FindChild("viewport") as Viewport;

			IEnumerable<View> views =
				from view in viewport.Views
				where view.Value.Control is View
				select view.Value.Control as View;

			BackEnd.InitMap(Map, views.Distinct().ToList());

			if (rdoInstanceHidden.Checked)
			{
				rdoInstanceHidden.Command.Execute(null);
			}
			else if (rdoInstanceTinted.Checked)
			{
				rdoInstanceTinted.Command.Execute(null);
			}
			else if (rdoInstanceNormal.Checked)
			{
				rdoInstanceNormal.Command.Execute(null);
			}

			var tree = viewport.Views[1].Control as TreeGridView;
			tree.Columns.Add(new GridColumn() { HeaderText = "Column 1", DataCell = new TextBoxCell(0) });
			tree.Columns.Add(new GridColumn() { HeaderText = "Column 2", DataCell = new TextBoxCell(1) });
			tree.Columns.Add(new GridColumn() { HeaderText = "Column 3", DataCell = new TextBoxCell(2) });
			tree.Columns.Add(new GridColumn() { HeaderText = "Column 4", DataCell = new TextBoxCell(3) });

			var items = new List<TreeGridItem>
			{
				new TreeGridItem(new object[] { "first", "second", "third" }),
				new TreeGridItem(new object[] { "morpb", "kwang", "wump" }),
				new TreeGridItem(new object[] { "dlooob", "oorf", "dimples" }),
				new TreeGridItem(new object[] { "wort", "hey", "karen" })
			};

			var collection = new TreeGridItemCollection(items);

			tree.DataStore = collection;
		}

		/// <summary>
		/// Set this form's Viewport to display its default View.
		/// </summary>
		/// <remarks>
		/// This needs to happen well after the Viewport class's LoadComplete
		/// and Shown events are raised, as well as after MainForm.LoadComplete,
		/// but should also only happen once. A self-removing handler up here in
		/// the MainForm class does the trick well enough.
		/// </remarks>
		private void SetDefaultView(object sender, EventArgs e)
		{
			((Viewport)Content).View = Viewport.DefaultView;

			Shown -= SetDefaultView;
		}
	}
}
