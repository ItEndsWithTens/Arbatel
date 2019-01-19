using Arbatel.Controls;
using Arbatel.UI;
using Eto.Forms;
using System.Diagnostics;

namespace ArbatelTest.Rendering
{
	public static class Commands
	{
		public static RelayCommand<MainForm> CmdAverageRefreshTime = new RelayCommand<MainForm>((form) =>
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
	}
}
