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
using Temblor.Controls;
using Temblor.Formats;
using Temblor.Graphics;
using Temblor.UI.Settings;
using Temblor.Utilities;
using Temblor.Formats.Quake;
using nucs.JsonSettings;
using System.Collections.ObjectModel;
using Temblor.UI.Preferences;
using System.Linq;

namespace Temblor.UI
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

			// A quick, simple way to load definitions and textures on startup.
			DlgPreferences_Closed(null, null);

			Content = new Viewport(BackEnd) { ID = "viewport" };
		}

		private void DlgPreferences_Closed(object sender, EventArgs e)
		{
			LocalSettings localSettings = DlgPreferences.LocalSettings;
			RoamingSettings roamingSettings = DlgPreferences.RoamingSettings;

			DefinitionDictionaries.Clear();

			foreach (var path in localSettings.DefinitionDictionaryPaths)
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

			TextureDictionaries.Clear();

			Stream stream = null;
			if (localSettings.UsingCustomPalette)
			{
				stream = new FileStream(localSettings.LastCustomPalette.LocalPath, FileMode.Open, FileAccess.Read);
			}
			else
			{
				var assembly = Assembly.GetAssembly(typeof(MainForm));
				var attribute = (AssemblyProductAttribute)assembly.GetCustomAttribute(typeof(AssemblyProductAttribute));

				var name = $"{attribute.Product}.res.palette-{roamingSettings.LastBuiltInPalette.ToLower()}.lmp";
				stream = assembly.GetManifestResourceStream(name);
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




			foreach (var view in viewport.Views)
			{
				if (view.Value is View)
				{
					(view.Value as View).Controller.InvertMouseY = true;
				}
			}

			var text = viewport.Views[0] as TextArea;

			// Instead of making the text view mode vertically shorter, just add some phantom
			// line breaks to push the text down, and make sure to keep the cursor below them.
			text.Text = "\n\n" + Map.ToString();
			text.CaretIndex = 2;
			text.CaretIndexChanged += (sender, e) =>
			{
				Title = text.CaretIndex.ToString();
				text.CaretIndex = text.CaretIndex < 2 ? 2 : text.CaretIndex;

			};

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
	}
}
