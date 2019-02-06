using Arbatel.Controls;
using Arbatel.Formats;
using Arbatel.Graphics;
using Arbatel.UI;
using Eto.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ArbatelTest.Rendering
{
	public static class Commands
	{
		public static RelayCommand<MainForm> CmdAverageRefreshTime = new RelayCommand<MainForm>(form =>
		{
			var viewport = form.Content as Viewport;

			if (viewport.Views[viewport.View].Control is View v)
			{
				v.GraphicsClock.Stop();
				form.ParentWindow.Title = "Testing refresh time...";

				int iterations = 60;

				var sw = Stopwatch.StartNew();
				v.Refresh();
				sw.Stop();

				long average = sw.ElapsedMilliseconds;

				sw.Reset();

				for (int i = 0; i < iterations - 1; i++)
				{
					sw.Start();
					v.Refresh();
					sw.Stop();

					average = (average + sw.ElapsedMilliseconds) / 2;

					sw.Reset();
				}

				v.GraphicsClock.Start();
				form.ParentWindow.Title = $"Average refresh time after {iterations} iterations: {average.ToString()}ms";
			}
		});

		public static RelayCommand<MainForm> CmdVisibleTriangles = new RelayCommand<MainForm>(form =>
		{
			var viewport = form.Content as Viewport;

			int triangles = 0;

			if (viewport.Views[viewport.View].Control is View v && v.Map != null)
			{
				IEnumerable<Renderable> renderables = v.Map.AllObjects.GetAllRenderables();

				List<(Polygon, Renderable)> polygons = v.Camera.GetVisiblePolygons(renderables);

				for (int i = 0; i < polygons.Count; i++)
				{
					triangles += polygons[i].Item1.Indices.Distinct().Count() - 2;
				}
			}

			form.Title = $"Visible triangles: {triangles}";
		});
	}
}
