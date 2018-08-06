using Eto.Gl;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using Eto.Forms;
using Eto.Drawing;
using Temblor.Controls;
using Temblor.Formats;
using Temblor.Graphics;
using Temblor.Utilities;

namespace Temblor
{
	public partial class MainForm
	{
		// HACK: Quick, dirty test to see how much I'm dumping on the graphics card.
		public static int triangleCount = 0;

		// Texture testing!
		public static Dictionary<string, int> testTextureDict;

		public static Wad2 Wad;

		public MainForm()
		{
			InitializeComponent();

			var viewport = new Viewport() { ID = "topLeft" };
			Content = viewport;

			var view3ds = new List<View>()
			{
				viewport.Views[2] as View,
				viewport.Views[3] as View,
				viewport.Views[4] as View
			};

			// Initialize OpenGL to avoid a delay when switching to a GL view.
			foreach (var view in view3ds)
			{
				view.MakeCurrent();
			}

			var palette = new Palette();
			using (var stream = new FileStream("D:/Development/Temblor/res/paletteQ.lmp", FileMode.Open, FileAccess.Read))
			//using (var stream = new FileStream("C:/Users/Harry/Development/Temblor/res/paletteQ.lmp", FileMode.Open, FileAccess.Read))
			using (var br = new BinaryReader(stream))
			{
				for (var i = 0; i < stream.Length / 3; i++)
				{
					var color = new Color() { Ab = 255 };
					color.Rb = br.ReadByte();
					color.Gb = br.ReadByte();
					color.Bb = br.ReadByte();

					palette.Add(color);
				}
			}

			Wad = new Wad2("D:/Projects/Games/Maps/Quake/common/wads/quake.wad", palette);
			//Wad = new Wad2("D:/Games/Quake/ad/maps/ad_sepulcher.wad", palette);
			//Wad = new Wad2("D:/Games/Quake/ad/maps/xmasjam_tens.wad", palette);
			//Wad = new Wad2("D:/Games/Quake/ad/maps/xmasjam_icequeen.wad", palette);

			//Wad = new Wad2("C:/Games/Quake/ID1/quake.wad", palette);

			// Should throw InvalidDataException referencing the filename.
			//var wad = new Wad2("D:/Projects/Games/Maps/Quake/common/wads/prototype.txt");

			KeyDown += MainForm_KeyDown;

			//var filename = "D:/Development/Temblor/scratch/jam6_tens.map";
			//var filename = "D:/Development/Temblor/scratch/medieval1.map";
			//var filename = "D:/Development/Temblor/scratch/basicobjectstest.map";
			//var filename = "D:/Development/Temblor/scratch/justacube.map";
			//var filename = "D:/Development/Temblor/scratch/justapyramid.map";
			//var filename = "D:/Development/Temblor/scratch/justaziggurat.map";
			//var filename = "D:/Development/Temblor/scratch/justacylinder.map";
			//var filename = "D:/Development/Temblor/scratch/rocktris.map";
			//var filename = "D:/Development/Temblor/scratch/brokensepulcherthing.map";
			//var filename = "D:/Development/Temblor/scratch/brokensepulcherthing-minimal.map";
			//var filename = "D:/Development/Temblor/scratch/texturedthing.map";
			//var filename = "D:/Development/Temblor/scratch/texturedthings.map";
			//var filename = "D:/Development/Temblor/scratch/texturedangledthing.map";
			//var filename = "D:/Development/Temblor/scratch/rockface.map";
			//var filename = "D:/Development/Temblor/scratch/rockface2.map";
			//var filename = "D:/Development/Temblor/scratch/manytextures.map";
			//var filename = "D:/Development/Temblor/scratch/sepulcher_chain.map";
			//var filename = "D:/Development/Temblor/scratch/sepulcher-metal1_3.map";
			//var filename = "D:/Development/Temblor/scratch/ad_sepulcher_test-tenslogo_EditedByHand.map";
			//var filename = "D:/Development/Temblor/scratch/translucency.map";
			//var filename = "D:/Games/Quake/ad/src/xmasjam_tens.map";
			//var filename = "D:/Games/Quake/ad/src/xmasjam_bal.map";
			//var filename = "D:/Games/Quake/ad/src/xmasjam_icequeen.map";
			//var filename = "D:/Games/Quake/ad/maps/ad_sepulcher.map";
			//var filename = "D:/Games/Quake/ad/maps/ad_magna.map";
			//var filename = "D:/Games/Quake/quake_map_source/start.map";
			var filename = "D:/Games/Quake/quake_map_source/e1m1.map";
			//var filename = "D:/Games/Quake/quake_map_source/e3m7.map";
			//var filename = "D:/Games/Quake/quake_map_source/e4m8.map";
			//var filename = "D:/Games/Quake/quake_map_source/e4m3.map";
			//var filename = "D:/Games/Quake/quake_map_source/e4m7.map";
			//var filename = "D:/Games/Quake/jam6/source/jam666_daz.map";

			//var filename = "C:/Users/Harry/Development/Temblor/scratch/basicobjectstest.map";
			//var filename = "C:/Users/Harry/Development/Temblor/scratch/texturedthing.map";
			//var filename = "C:/Users/Harry/Development/Temblor/scratch/manytextures.map";
			//var filename = "C:/Games/Quake/quake_map_source/start.map";
			var s = new FileStream(filename, FileMode.Open, FileAccess.Read);
			var map = new QuakeMap(s);












			testTextureDict = new Dictionary<string, int>();
			foreach (var t in Wad.Textures.Values)
			{
				GL.GenTextures(1, out int id);
				testTextureDict.Add(t.Name, id);

				GL.BindTexture(TextureTarget.Texture2D, id);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, t.Width, t.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, t.ToUncompressed(Eto.Drawing.PixelFormat.Format32bppRgba, flip: true));
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}



			foreach (var view in view3ds)
			{
				foreach (var mapObject in map.MapObjects)
				{
					mapObject.Init(view);
				}
			}

			viewport.Map = map;

			foreach (var view in viewport.Views)
			{
				if (view.Value is View)
				{
					(view.Value as View).Controller.InvertMouseY = true;
				}
			}

			var text = viewport.Views[0] as TextArea;
			//text.Text = map.Raw;

			//text.Text = map.Blocks[0].ToString();
			//text.CaretIndex = 0;

			// Instead of making the text view mode vertically shorter, just add some phantom
			// line breaks to push the text down, and make sure to keep the cursor below them.
			text.Text = "\n\n" + map.MapObjects[0].Block.ToString();
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

				//foreach (var renderable in view.Map.Renderables)
				//{
				//	//renderable.Vertices.Add(new Graphics.Vertex(1.0f, 0.75f, 0.0f));
				//	//renderable.Indices.Add(2);
				//	//renderable.Indices.Add(3);
				//	//renderable.Indices.Add(0);
				//	//renderable.Init(surfaces);
				//}

				view.Invalidate();
			}
		}
	}
}
