using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Temblor.Controls;
using Temblor.Formats;

namespace Temblor
{
	public partial class MainForm
	{
		public MainForm()
		{
			InitializeComponent();

			var viewport = new Viewport() { ID = "topLeft" };

			Content = viewport;

			KeyDown += MainForm_KeyDown;

			var filename = "D:/Development/Temblor/scratch/jam6_tens.map";
			var s = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			var map = new QuakeMap(s);

			

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
		}
	}
}
