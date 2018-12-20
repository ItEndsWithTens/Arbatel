using Eto;
using Eto.Forms;
using Eto.Gl;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Arbatel.Controllers;
using Arbatel.Formats;
using Arbatel.Graphics;

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
			foreach (var style in (ShadingStyle[])Enum.GetValues(typeof(ShadingStyle)))
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

	public class View : GLSurface
	{
		// -- System types
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
			}
		}

		// -- Eto
		public UITimer GraphicsClock = new UITimer();
		public UITimer InputClock = new UITimer();
		public Label Label = new Label();

		// -- OpenTK
		public Color4 ClearColor;
		public PolygonMode PolygonMode;

		// -- Arbatel

		/// <summary>
		/// The graphics backend to use when rendering in this View.
		/// </summary>
		public Backend Backend { get; set; }
		public Camera Camera = new Camera();
		public Controller Controller;

		public Map Map;

		public ShadingStyle ShadingStyle;
		public Dictionary<ShadingStyle, Shader> Shaders;

		// Explicitly choosing an eight-bit stencil buffer prevents visual artifacts
		// on the Mac platform; the GraphicsMode defaults are apparently insufficient.
		private static GraphicsMode _mode = new GraphicsMode(new ColorFormat(32), 8, 8, 8);

		// -- Constructors
		public View() : this(_mode, 3, 3, GraphicsContextFlags.Default)
		{
		}
		public View(GraphicsMode _mode, int _major, int _minor, GraphicsContextFlags _flags) :
			base(_mode, _major, _minor, _flags)
		{
			Fps = 60.0f;
			InputClock.Interval = 1.0 / (Fps * 2.0);

			ShadingStyle = ShadingStyle.Wireframe;

			Label.Text = "View";
			Label.BackgroundColor = Eto.Drawing.Colors.Black;
			Label.TextColor = Eto.Drawing.Colors.White;

			GraphicsClock.Elapsed += GraphicsClock_Elapsed;
			InputClock.Elapsed += InputClock_Elapsed;
			GLInitalized += View_GLInitialized;
		}

		// -- Methods
		public void Clear()
		{
			GL.Viewport(0, 0, Width, Height);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		public void Refresh()
		{
			Clear();

			if (Map != null)
			{
				Camera.AspectRatio = (float)Width / (float)Height;

				for (int i = 0; i < Map.MapObjects.Count; i++)
				{
					Map.MapObjects[i].Draw(Shaders, ShadingStyle, this, Camera);
				}
			}

			SwapBuffers();
		}

		// -- Overrides
		protected override void OnDraw(EventArgs e)
		{
			base.OnDraw(e);

			// OnDraw only gets called in certain circumstances, for example
			// when the application window is resized. During such an event,
			// there's no guarantee that the call to Refresh by this class's
			// clock will happen before the call to OnDraw, in which GLSurface
			// clears the viewport, which means the view will flicker with its
			// clear color. Refreshing when the OnDraw event is raised prevents
			// that, and keeps everything smooth.
			Refresh();
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);

			GraphicsClock.Interval = 1.0 / Fps;
			InputClock.Start();
		}
		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);

			// A perhaps-unnecessary performance thing; would love to allow full framerate
			// for all visible views, so editing objects is visually smooth everywhere at once.
			GraphicsClock.Interval = 1.0 / (Fps / 4.0);
			InputClock.Stop();
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			if (Enabled)
			{
				GraphicsClock.Start();
			}
			else
			{
				GraphicsClock.Stop();
				InputClock.Stop();

				Controller.MouseLook = false;
				Style = "showcursor";
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			Controller.KeyDown(this, e);
		}
		protected override void OnKeyUp(KeyEventArgs e)
		{
			Controller.KeyUp(this, e);
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			Focus();
		}

		// -- Event handlers
		private void GraphicsClock_Elapsed(object sender, EventArgs e)
		{
			Refresh();
		}

		private void InputClock_Elapsed(object sender, EventArgs e)
		{
			Controller.Update();
		}

		private void View_GLInitialized(object sender, EventArgs e)
		{
			GL.Enable(EnableCap.DepthTest);

			GL.Enable(EnableCap.CullFace);
			GL.FrontFace(FrontFaceDirection.Ccw);
			GL.CullFace(CullFaceMode.Back);

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			// GL.ClearColor has two overloads, and if this class' ClearColor field is
			// passed in, the compiler tries to use the one taking a System.Drawing.Color
			// parameter instead of the one taking an OpenTK.Graphics.Color4. Using the
			// float signature therefore avoids an unnecessary System.Drawing reference.
			GL.ClearColor(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);

			Shader.GetGlslVersion(out int major, out int minor);
			Shaders = new Dictionary<ShadingStyle, Shader>
			{
				{ ShadingStyle.Wireframe, new Shader() { Backend = Backend } },
				{ ShadingStyle.Flat, new FlatShader(major, minor) { Backend = Backend } },
				{ ShadingStyle.Textured, new SingleTextureShader(major, minor) { Backend = Backend } }
			};

			// FIXME: Causes InvalidEnum from GL.GetError, at least on my OpenGL 2.1, GLSL 1.2, Intel HD Graphics laptop.
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode);

			// TEST. Also remember to switch Camera to use left-handed, Z-up position at some point.
			Camera.Position = new Vector3(256.0f, 1024.0f, 1024.0f);
			Camera.Pitch = -30.0f;
		}
	}
}
