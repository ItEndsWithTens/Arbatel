using Arbatel.Controllers;
using Arbatel.Formats;
using Arbatel.Graphics;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;

namespace Arbatel.Controls
{
	public enum ShadingStyle
	{
		Wireframe,
		Flat,
		Textured
	}

	public static class ShadingStyleExtensions
	{
		public static Dictionary<ShadingStyle, ShadingStyle> Capped(this Dictionary<ShadingStyle, ShadingStyle> dict, ShadingStyle max)
		{
			foreach (ShadingStyle style in (ShadingStyle[])Enum.GetValues(typeof(ShadingStyle)))
			{
				dict.Add(style, style > max ? max : style);
			}

			return dict;
		}

		public static Dictionary<ShadingStyle, ShadingStyle> Default(this Dictionary<ShadingStyle, ShadingStyle> dict)
		{
			var values = (ShadingStyle[])Enum.GetValues(typeof(ShadingStyle));

			return dict.Capped(values[values.Length - 1]);
		}
	}

	public class View : Drawable, IUpdateFromSettings
	{
		/// <summary>
		/// The graphics backend to use when rendering in this View.
		/// </summary>
		public BackEnd BackEnd { get; set; }
		public Camera Camera { get; } = new Camera();

		public Dictionary<string, Color> ColorScheme { get; private set; } = new Dictionary<string, Color>();

		public Controller Controller { get; set; } = new Controller();

		public UITimer GraphicsClock { get; } = new UITimer();

		private ShadingStyle _shadingStyle = ShadingStyle.Wireframe;
		public ShadingStyle ShadingStyle
		{
			get { return _shadingStyle; }
			set
			{
				_shadingStyle = value;

				Map?.UpdateColors(ShadingStyle);
			}
		}

		public Map Map { get; set; }

		public override bool Enabled
		{
			get { return base.Enabled; }
			set
			{
				base.Enabled = value;

				if (value == false)
				{
					Controller.Deactivate();

					if (Style == "hidecursor")
					{
						Style = "showcursor";
					}
				}
			}
		}

		private float _fps;
		/// <summary>
		/// This view's framerate. Sets GraphicsClock.Interval to 1.0 / value,
		/// and Controller.Clock.Interval to 1.0 / (value * 2.0).
		/// </summary>
		public float Fps
		{
			get { return _fps; }
			set
			{
				_fps = value;
				GraphicsClock.Interval = 1.0 / value;
				Controller.Clock.Interval = 1.0 / (value * 2.0);
			}
		}

		public event EventHandler Updated;

		public View()
		{
			CanFocus = true;

			GraphicsClock.Elapsed += GraphicsClock_Elapsed;
		}
		public View(Control surface) : this()
		{
			Content = surface;
		}

		public virtual void Refresh()
		{
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);

			GraphicsClock.Interval = 1.0 / Fps;
			Controller.Activate();
		}
		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);

			// TODO: Get rid of this? I think focus may be lost when even just
			// clicking menus, so it's not a reliable way to lower CPU impact
			// when users make another application current.
			//
			// A perhaps-unnecessary performance thing; would love to allow full framerate
			// for all visible views, so editing objects is visually smooth everywhere at once.
			GraphicsClock.Interval = 1.0 / (Fps / 4.0);
			Controller.Deactivate();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			Controller.KeyDown(this, e);
		}
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			Controller.KeyUp(this, e);
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			Focus();
		}

		private void GraphicsClock_Elapsed(object sender, EventArgs e)
		{
			Refresh();
		}

		public virtual void UpdateFromSettings(Settings settings)
		{
			if (Controller is FirstPersonController)
			{
				Controller.InvertMouseX = settings.Roaming.InvertMouseX;
				Controller.InvertMouseY = settings.Roaming.InvertMouseY;
			}

			// Convert an input of [0, 100] to an output of [0.0, 0.5].
			Controller.MouseSensitivity = settings.Local.MouseSensitivity * 0.005f;

			Controller.Speed = settings.Roaming.MovementSpeed;

			ColorScheme = settings.Roaming.ColorSchemes[settings.Roaming.CurrentColorScheme];

			OnUpdated(EventArgs.Empty);
		}

		protected virtual void OnUpdated(EventArgs e)
		{
			Updated?.Invoke(this, e);
		}
	}
}
