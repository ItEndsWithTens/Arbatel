using Arbatel.Controls;
using Arbatel.Formats;
using Arbatel.Formats.Quake;
using Arbatel.Graphics;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Arbatel.UI
{
	public class ProgressEventArgs : EventArgs
	{
		public string Message { get; private set; }
		public int? Value { get; private set; }
		public bool Clear { get; private set; }

		public ProgressEventArgs(int value)
		{
			Value = value;

			if (value >= 100)
			{
				Clear = true;
			}
		}
		public ProgressEventArgs(string message)
		{
			Message = message;

			Clear = true;
		}
		public ProgressEventArgs(int value, string message)
		{
			Value = value;
			Message = message;

			if (value >= 100)
			{
				Clear = true;
			}
		}
	}

	public interface IProgress
	{
		event EventHandler<ProgressEventArgs> ProgressUpdated;

		void OnProgressUpdated(object sender, ProgressEventArgs e);
	}

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

				Viewport.Map = _map;
			}
		}

		public Settings Settings { get; } = new Settings();

		public AutoReloader MapReloader { get; private set; }

		private ProgressBar ProgressBar { get; } = new ProgressBar();

		private UITimer ProgressClearClock = new UITimer { Interval = 3 };

		private Label StatusDisplay { get; } = new Label();

		private Viewport Viewport { get; }

		public MainForm()
		{
			InitializeComponent();

			InitializeCommands();

			if (Core.UseVeldrid)
			{
				Viewport = new VeldridViewport { ID = "viewport" };
			}
			else
			{
				Viewport = new OpenGLViewport { ID = "viewport" };
			}

			BackEnd = Viewport.BackEnd;
			BackEnd.ProgressUpdated += ProgressReported;

			foreach ((Control Control, string Name, Action<View> SetUp) view in Viewport.Views.Values)
			{
				if (view.Control is View v)
				{
					Settings.Updatables.Add(v);
				}
			}

			ButtonMenuItem viewMenu = Menu.Items.GetSubmenu("View");
			foreach (KeyValuePair<int, Command> command in Viewport.ViewCommands)
			{
				viewMenu.Items.Insert(command.Key, command.Value);
			}
			viewMenu.Items.Insert(Viewport.ViewCommands.Count, new SeparatorMenuItem());

			var bottomBar = new TableLayout(2, 1);
			bottomBar.Add(StatusDisplay, 0, 0, true, true);
			bottomBar.Add(ProgressBar, 1, 0, true, true);

			var table = new TableLayout(1, 2);
			table.Add(Viewport, 0, 0, true, true);
			table.Add(bottomBar, 0, 1, true, false);

			Content = table;

			ProgressClearClock.Elapsed += (sender, e) =>
			{
				ProgressClearClock.Stop();

				StatusDisplay.Text = "";
				ProgressBar.Value = 0;
			};

			Shown += SetDefaultView;

			MapReloader = new AutoReloader((f) => ReloadMap(f));
		}

		private void OpenMap(string fileName)
		{
			if (String.IsNullOrEmpty(fileName))
			{
				return;
			}

			string ext = Path.GetExtension(fileName);

			if (ext.ToLower() != ".map")
			{
				throw new InvalidDataException("Unrecognized map format!");
			}

			var definitions = new Dictionary<string, DefinitionDictionary>();

			foreach (string path in Settings.Local.DefinitionDictionaryPaths)
			{
				definitions.Add(path, Loader.LoadDefinitionDictionary(path));
			}

			using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				Map = new QuakeMap(stream, definitions.Values.ToList().Stack());
			}

			Map.ProgressUpdated += ProgressReported;

			Task.Run(() =>
			{
				Map.Parse();

				Settings.Updatables.Add(Map);
				Settings.Save();

				BackEnd.InitTextures(Map.Textures);

				GetAllThisNonsenseReady();

				Application.Instance.Invoke(() =>
				{
					MapReloader.File = fileName;
					MapReloader.Enabled = cbxAutoReload.Checked;
				});
			});
		}
		private void CloseMap()
		{
			if (Map == null)
			{
				return;
			}

			MapReloader.Enabled = false;

			IEnumerable<View> views =
				from view in Viewport.Views
				where view.Value.Control is View
				select view.Value.Control as View;

			Application.Instance.Invoke(() => StatusDisplay.Text = "Dropping map from backend...");

			Map.InitializedInBackEnd = false;
			BackEnd.DeleteMap(Map, views);

			Settings.Updatables.Remove(Map);

			Map = null;

			Application.Instance.Invoke(() =>
			{
				StatusDisplay.Text = "";
				ProgressBar.Value = 0;
			});
		}
		private void ReloadMap(string fileName)
		{
			if (Map == null)
			{
				return;
			}

			CloseMap();
			OpenMap(fileName);
		}

		private void GetAllThisNonsenseReady()
		{
			IEnumerable<View> views =
				from view in Viewport.Views
				where view.Value.Control is View
				select view.Value.Control as View;

			BackEnd.InitMap(Map, views.Distinct().ToList());

			Application.Instance.Invoke(() =>
			{
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
			});

			// TODO: Reenable this once I actually understand data binding in
			// Eto! Currently it's just wasting memory every time users close
			// a map and open a new one. Eventually I want a hierarchical view
			// of the level contents, but we'll get there when we get there.
			//var tree = viewport.Views[1].Control as TreeGridView;
			//tree.Columns.Add(new GridColumn() { HeaderText = "Column 1", DataCell = new TextBoxCell(0) });
			//tree.Columns.Add(new GridColumn() { HeaderText = "Column 2", DataCell = new TextBoxCell(1) });
			//tree.Columns.Add(new GridColumn() { HeaderText = "Column 3", DataCell = new TextBoxCell(2) });
			//tree.Columns.Add(new GridColumn() { HeaderText = "Column 4", DataCell = new TextBoxCell(3) });

			//var items = new List<TreeGridItem>
			//{
			//	new TreeGridItem(new object[] { "first", "second", "third" }),
			//	new TreeGridItem(new object[] { "morpb", "kwang", "wump" }),
			//	new TreeGridItem(new object[] { "dlooob", "oorf", "dimples" }),
			//	new TreeGridItem(new object[] { "wort", "hey", "karen" })
			//};

			//var collection = new TreeGridItemCollection(items);

			//tree.DataStore = collection;
		}

		private void ProgressReported(object sender, ProgressEventArgs e)
		{
			Application.Instance.Invoke(() =>
			{
				ProgressClearClock.Stop();

				if (e.Value != null)
				{
					ProgressBar.Value = (int)e.Value;
				}

				if (!String.IsNullOrEmpty(e.Message))
				{
					StatusDisplay.Text = e.Message;
				}

				if (e.Clear)
				{
					ProgressClearClock.Start();
				}
			});
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
			Viewport.View = Viewport.DefaultView;

			Shown -= SetDefaultView;
		}
	}
}
