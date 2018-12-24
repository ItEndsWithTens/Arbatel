using Arbatel.Controllers;
using Arbatel.Formats;
using Arbatel.Graphics;
using Eto;
using Eto.Forms;
using Eto.Gl;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
		// OpenGL 3.0 is the lowest version that has all the features this
		// project needs built in. Vertex array objects are actually the only
		// feature this project currently uses that's not available in 2.X, but
		// with the appropriate extension, everything works fine. Unfortunately,
		// requesting a context of less than 3.2 in macOS will produce a 2.1
		// context without said extension.
		public static int GLMajor { get; } = EtoEnvironment.Platform.IsMac ? 3 : 2;
		public static int GLMinor { get; } = EtoEnvironment.Platform.IsMac ? 2 : 0;

		/// <summary>
		/// Extensions required to run under OpenGL versions below 3.0.
		/// </summary>
		public static ReadOnlyCollection<string> RequiredGLExtensions { get; } =
			new List<string>
			{
				"ARB_vertex_array_object"
			}.AsReadOnly();

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

		private bool _openGLReady = false;
		public bool OpenGLReady
		{
			get { return _openGLReady; }
			protected set
			{
				_openGLReady = value;

				// If Enabled was set true before OpenGL was initialized, the
				// GraphicsClock didn't start, so give it a nudge if it's time.
				if (value && Enabled)
				{
					Enabled = true;
				}
			}
		}

		// -- Eto
		UITimer GraphicsClock = new UITimer();
		public UITimer InputClock = new UITimer();

		// This was previously accomplished by overriding OnEnabledChanged, but
		// Eto.Gl is currently built against Eto 2.4.0, whose MacView class, the
		// base for GLSurface and in turn View, doesn't support that event.
		public override bool Enabled
		{
			get { return base.Enabled; }
			set
			{
				base.Enabled = value;

				if (OpenGLReady && value == true)
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
		}

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
		public View() : this(_mode)
		{
		}
		public View(GraphicsMode _mode) : base(_mode, GLMajor, GLMinor, GraphicsContextFlags.ForwardCompatible)
		{
			Fps = 60.0f;
			InputClock.Interval = 1.0 / (Fps * 2.0);

			ShadingStyle = ShadingStyle.Wireframe;

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
			string version = GL.GetString(StringName.Version);

			string[] split = version.Split('.', ' ');

			bool gotMajor = int.TryParse(split[0], out int glMajor);
			bool gotMinor = int.TryParse(split[1], out int glMinor);

			if (gotMajor && gotMinor)
			{
				if (glMajor < 3)
				{
					string extensions = GL.GetString(StringName.Extensions);

					var missing = new List<string>();

					foreach (string extension in RequiredGLExtensions)
					{
						if (!extensions.Contains(extension))
						{
							missing.Add(extension);
						}
					}

					if (missing.Count > 0)
					{
						string message = $"{Core.Name} needs at least OpenGL 3.0, or these missing extensions:\n\n";
						message += String.Join("\n", missing.ToArray());

						throw new GraphicsException(message);
					}
				}
			}
			else
			{
				throw new GraphicsException("Couldn't parse OpenGL version string!");
			}

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

			Shader.GetGlslVersion(out int glslMajor, out int glslMinor);
			Shaders = new Dictionary<ShadingStyle, Shader>
			{
				{ ShadingStyle.Wireframe, new Shader() { Backend = Backend } },
				{ ShadingStyle.Flat, new FlatShader(glslMajor, glslMinor) { Backend = Backend } },
				{ ShadingStyle.Textured, new SingleTextureShader(glslMajor, glslMinor) { Backend = Backend } }
			};

			// FIXME: Causes InvalidEnum from GL.GetError, at least on my OpenGL 2.1, GLSL 1.2, Intel HD Graphics laptop.
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode);

			// TEST. Also remember to switch Camera to use left-handed, Z-up position at some point.
			Camera.Position = new Vector3(256.0f, 1024.0f, 1024.0f);
			Camera.Pitch = -30.0f;

			OpenGLReady = true;
		}
	}
}
