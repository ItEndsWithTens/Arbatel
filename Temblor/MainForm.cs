using Eto.Gl;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Temblor.Controls;
using Temblor.Formats;
using Temblor.Utilities;

namespace Temblor
{
	public partial class MainForm
	{
		public MainForm()
		{
			InitializeComponent();

			// Initialize OpenGL to avoid a delay when switching to a GL view.
			var gl = new GlUtilities();
			gl.InitGl();

			KeyDown += MainForm_KeyDown;

			//var filename = "D:/Development/Temblor/scratch/jam6_tens.map";
			var filename = "D:/Development/Temblor/scratch/medieval1.map";
			var s = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			var map = new QuakeMap(s);



			var viewport = new Viewport() { ID = "topLeft" };

			Content = viewport;

			var surfaces = new List<GLSurface>();
			foreach (var view in viewport.Views)
			{
				if (view.Value is GLSurface)
				{
					surfaces.Add(view.Value as GLSurface);
				}
			}

			//map.Renderables.Add(new Renderable() { Surfaces = surfaces });
			//map.Renderables.Add(new Renderable() { Position = new Vector3(-4.0f, 2.25f, 1.0f), Surfaces = surfaces });
			//map.Renderables.Add(new Renderable() { Position = new Vector3(4.0f, 2.25f, 1.0f), Surfaces = surfaces });
			//map.Renderables.Add(new Renderable() { Position = new Vector3(0.0f, 0.0f, -10.0f), Surfaces = surfaces });

			var r = new Random();

			var scale = 100.0f;

			for (var i = 0; i < 40960; i++)
			{
				var pos = new Vector3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
				pos *= scale;

				map.Renderables.Add(new Renderable() { Position = pos, Surfaces = surfaces });
			}

			foreach (var renderable in map.Renderables)
			{
				renderable.Init();
			}


			viewport.Map = map;

			var text = viewport.Views[0] as TextArea;
			//text.Text = map.Raw;

			//text.Text = map.Blocks[0].ToString();
			//text.CaretIndex = 0;

			// Instead of making the text view mode vertically shorter, just add some phantom
			// line breaks to push the text down, and make sure to keep the cursor below them.
			text.Text = "\n\n" + map.Blocks[0].ToString();
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

		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Keys.Tab)
			{
				var viewport = FindChild("topLeft") as Viewport;

				if (e.Modifiers == Keys.Shift)
				{
					viewport.View--;
				}
				else
				{
					viewport.View++;
				}
			}
			else if (e.Key == Keys.F)
			{
				var viewport = Content as Viewport;

				var view = viewport.Views[viewport.View] as View;

				view.Map.Renderables[0].Vertices.Add(new Graphics.Vertex(1.0f, 0.75f, 0.0f));
				view.Map.Renderables[0].Indices.Add(2);
				view.Map.Renderables[0].Indices.Add(3);
				view.Map.Renderables[0].Indices.Add(0);
				view.Map.Renderables[0].Init();
				view.Invalidate();
			}
		}
	}
}
