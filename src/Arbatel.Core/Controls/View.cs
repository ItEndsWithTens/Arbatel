using Arbatel.Controllers;
using Arbatel.Formats;
using Arbatel.Graphics;
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

	public class View : Panel, IUpdateFromSettings
	{
		/// <summary>
		/// The graphics backend to use when rendering in this View.
		/// </summary>
		public BackEnd BackEnd { get; set; }
		public Camera Camera { get; } = new Camera();
		public Controller Controller { get; set; }

		public UITimer GraphicsClock { get; } = new UITimer();
		public UITimer InputClock { get; } = new UITimer();

		public Dictionary<ShadingStyle, Shader> Shaders { get; } = new Dictionary<ShadingStyle, Shader>();
		public ShadingStyle ShadingStyle { get; set; } = ShadingStyle.Wireframe;

		public Map Map { get; set; }

		public override bool Enabled
		{
			get { return base.Enabled; }
			set
			{
				base.Enabled = value;

				if (value == false)
				{
					InputClock.Stop();

					Controller.MouseLook = false;
					Style = "showcursor";
				}
			}
		}

		private float _fps;
		public float Fps
		{
			get
			{
				return _fps;
			}

			set
			{
				_fps = value;
				GraphicsClock.Interval = 1.0 / value;
				InputClock.Interval = 1.0 / (value * 2.0);
			}
		}

		public new Control Content
		{
			get { return base.Content; }
			set
			{
				base.Content = value;

				Content.GotFocus += Content_GotFocus;
				Content.LostFocus += Content_LostFocus;

				Content.KeyDown += Content_KeyDown;
				Content.KeyUp += Content_KeyUp;

				Content.MouseEnter += Content_MouseEnter;
			}
		}

		public View()
		{
			Fps = 60.0f;

			GraphicsClock.Elapsed += GraphicsClock_Elapsed;
			InputClock.Elapsed += InputClock_Elapsed;
		}
		public View(Control surface) : this()
		{
			Content = surface;
		}

		public virtual void Refresh()
		{
		}

		private void Content_GotFocus(object sender, EventArgs e)
		{
			GraphicsClock.Interval = 1.0 / Fps;
			InputClock.Start();
		}
		private void Content_LostFocus(object sender, EventArgs e)
		{
			// A perhaps-unnecessary performance thing; would love to allow full framerate
			// for all visible views, so editing objects is visually smooth everywhere at once.
			GraphicsClock.Interval = 1.0 / (Fps / 4.0);
			InputClock.Stop();
		}

		private void Content_KeyDown(object sender, KeyEventArgs e)
		{
			Controller.KeyDown(this, e);
		}
		private void Content_KeyUp(object sender, KeyEventArgs e)
		{
			Controller.KeyUp(this, e);
		}

		private void Content_MouseEnter(object sender, MouseEventArgs e)
		{
			Content.Focus();
		}

		private void GraphicsClock_Elapsed(object sender, EventArgs e)
		{
			Refresh();
		}

		private void InputClock_Elapsed(object sender, EventArgs e)
		{
			Controller.Update();
		}

		public virtual void UpdateFromSettings(Settings settings)
		{
			if (Controller is FirstPersonController)
			{
				Controller.InvertMouseX = settings.Roaming.InvertMouseX;
				Controller.InvertMouseY = settings.Roaming.InvertMouseY;
			}
		}
	}
}
