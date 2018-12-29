using Eto.Gl;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Arbatel.Controls;
using Arbatel.Formats;
using Arbatel.Graphics;
using Arbatel.UI.Settings;
using Arbatel.Utilities;
using Arbatel.Formats.Quake;
using nucs.JsonSettings;
using System.Collections.ObjectModel;
using Arbatel.UI.Preferences;
using System.Linq;

namespace Arbatel.UI
{
	public partial class MainForm
	{
		/// <summary>
		/// The graphics backend used by this application.
		/// </summary>
		public Backend BackEnd { get; private set; } = new OpenGL4BackEnd();

		public DefinitionDictionary CombinedDefinitions { get; set; } = new DefinitionDictionary();

		public TextureDictionary CombinedTextures { get; set; } = new TextureDictionary();

		public Dictionary<string, DefinitionDictionary> DefinitionDictionaries { get; set; } = new Dictionary<string, DefinitionDictionary>();

		public Dictionary<string, TextureDictionary> TextureDictionaries { get; set; } = new Dictionary<string, TextureDictionary>();

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

		public MainForm()
		{
			InitializeComponent();

			InitializeCommands();

			DlgPreferences.Closed += DlgPreferences_Closed;

			UpdateDefinitions(DlgPreferences.LocalSettings);

			UpdateTextures(DlgPreferences.LocalSettings, DlgPreferences.RoamingSettings);

			var viewport = new Viewport(BackEnd) { ID = "viewport" };

			ButtonMenuItem viewMenu = Menu.Items.GetSubmenu("View");
			foreach (var command in viewport.ViewCommands)
			{
				viewMenu.Items.Insert(command.Key, command.Value);
			}
			viewMenu.Items.Insert(viewport.ViewCommands.Count, new SeparatorMenuItem());

			Content = viewport;

			UpdateControls(DlgPreferences.RoamingSettings);
		}

		private void DlgPreferences_Closed(object sender, EventArgs e)
		{
			LocalSettings local = DlgPreferences.LocalSettings;
			RoamingSettings roaming = DlgPreferences.RoamingSettings;

			UpdateControls(roaming);

			UpdateDefinitions(local);

			UpdateTextures(local, roaming);
		}

		private void GetAllThisNonsenseReady()
		{
			var viewport = FindChild("viewport") as Viewport;

			var view3ds = new List<View>()
			{
				viewport.Views[2] as View,
				viewport.Views[3] as View,
				viewport.Views[4] as View
			};

			foreach (var mo in Map.MapObjects)
			{
				mo.Init(view3ds);
			}

			var tree = viewport.Views[1] as TreeGridView;
			tree.Columns.Add(new GridColumn() { HeaderText = "Column 1", DataCell = new TextBoxCell(0) });
			tree.Columns.Add(new GridColumn() { HeaderText = "Column 2", DataCell = new TextBoxCell(1) });
			tree.Columns.Add(new GridColumn() { HeaderText = "Column 3", DataCell = new TextBoxCell(2) });
			tree.Columns.Add(new GridColumn() { HeaderText = "Column 4", DataCell = new TextBoxCell(3) });

			var items = new List<TreeGridItem>();
			items.Add(new TreeGridItem(new object[] { "first", "second", "third" }));
			items.Add(new TreeGridItem(new object[] { "morpb", "kwang", "wump" }));
			items.Add(new TreeGridItem(new object[] { "dlooob", "oorf", "dimples" }));
			items.Add(new TreeGridItem(new object[] { "wort", "hey", "karen" }));

			var collection = new TreeGridItemCollection(items);

			tree.DataStore = collection;
		}

		private void UpdateControls(RoamingSettings roaming)
		{
			foreach (var view in (Content as Viewport).Views.Values)
			{
				if (view is View3d v)
				{
					v.Controller.InvertMouseX = roaming.InvertMouseX;
					v.Controller.InvertMouseY = roaming.InvertMouseY;
				}
			}
		}

		private void UpdateDefinitions(LocalSettings local)
		{
			DefinitionDictionaries.Clear();

			foreach (var path in local.DefinitionDictionaryPaths)
			{
				DefinitionDictionaries.Add(path, Loader.LoadDefinitionDictionary(path));
			}

			if (DlgPreferences.FindChild<RadioButton>(DlgPreferences.BtnFgdCombineStackName).Checked)
			{
				CombinedDefinitions = DefinitionDictionaries.Values.ToList().Stack();
			}
			else
			{
				// TODO: Implement blend.
			}
		}

		private void UpdateTextures(LocalSettings local, RoamingSettings roaming)
		{
			TextureDictionaries.Clear();

			Stream stream = null;
			if (local.UsingCustomPalette)
			{
				stream = new FileStream(local.LastCustomPalette.LocalPath, FileMode.Open, FileAccess.Read);
			}
			else
			{
				var name = $"palette-{roaming.LastBuiltInPalette.ToLower()}.lmp";

				stream = Assembly.GetAssembly(typeof(MainForm)).GetResourceStream(name);
			}

			using (stream)
			{
				var palette = new Palette().LoadQuakePalette(stream);

				foreach (var path in DlgPreferences.LocalSettings.TextureDictionaryPaths)
				{
					TextureDictionaries.Add(path, Loader.LoadTextureDictionary(path, palette));
				}
			}

			CombinedTextures = TextureDictionaries.Values.ToList().Stack();

			if (Map != null)
			{
				Map.Textures = CombinedTextures;
			}
		}
	}
}
