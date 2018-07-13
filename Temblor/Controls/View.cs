using Eto;
using Eto.Forms;
using Eto.Gl;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Temblor.Controls
{
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
				Clock.Interval = 1.0 / value;
			}
		}

		public string[] VertexShaderSource330 =
		{
			"#version 330 core",
			"layout (location = 0) in vec3 position;",
			"layout (location = 1) in vec3 normal;",
			"layout (location = 2) in vec4 color;",
			"layout (location = 3) in vec2 texCoords;",
			"",
			"out vec4 vertexColor;",
			"",
			"void main()",
			"{",
			"   gl_Position = vec4(position, 1.0f);",
			"	vertexColor = color;",
			"}"
		};
		public string[] FragmentShaderSource330 =
		{
			"#version 330 core",
			"",
			"in vec4 vertexColor;",
			"",
			"out vec4 color;",
			"",
			"void main()",
			"{",
			"   color = vertexColor;",
			"}"
		};

		private DateTime _initTime;
		private float _previousTime = 0.0f;
		private float _deltaTime = 0.0f;

		// -- Eto
		public UITimer Clock = new UITimer();
		public Label Label = new Label();

		// -- OpenTK
		public Color4 ClearColor = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
		
		// -- Temblor
		public Camera Camera = new Camera();
		public Shader Shader;

		// Explicitly choosing an eight-bit stencil buffer prevents visual artifacts
		// on the Mac platform; the GraphicsMode defaults are apparently insufficient.
		private static GraphicsMode _graphicsMode = new GraphicsMode(new ColorFormat(32), 8, 8, 8);

		// -- Constructors
		public View() : this(_graphicsMode, 3, 3, GraphicsContextFlags.Default)
		{
		}
		public View(GraphicsMode mode, int major, int minor, GraphicsContextFlags flags) :
			base(mode, major, minor, flags)
		{
			GLInitalized += View_GLInitialized;

			Label.Text = "View";
			Label.BackgroundColor = Eto.Drawing.Colors.Black;
			Label.TextColor = Eto.Drawing.Colors.White;

			Fps = 60.0f;

			Clock.Elapsed += Clock_Elapsed;
		}

		// -- Methods
		public void Clear()
		{
			GL.Viewport(0, 0, Width, Height);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		private void Refresh()
		{
			Clear();

			var map = (Parent as Viewport).Map;

			Shader.Use();

			foreach (var renderable in map.Renderables)
			{
				renderable.Draw(Shader);
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

			Clock.Interval = 1.0 / Fps;
		}
		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);

			// A perhaps-unnecessary performance thing; would love to allow full framerate
			// for all visible views, so editing objects is visually smooth everywhere at once.
			Clock.Interval = 1.0 / (Fps / 4.0);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Clock.Start();
		}
		protected override void OnUnLoad(EventArgs e)
		{
			base.OnUnLoad(e);

			Clock.Stop();
		}

		// -- Event handlers
		private void Clock_Elapsed(object sender, EventArgs e)
		{
			var time = Convert.ToSingle(DateTime.Now.Subtract(_initTime).TotalSeconds);

			_deltaTime = time - _previousTime;
			_previousTime = time;

			// Quick test, this does seem to work. Just checking that the clock is functional.
			if (ParentWindow != null)
			{
				//ParentWindow.Title = _previousTime.ToString();
				ParentWindow.Title = "Parent size: " + Parent.Size.ToString() + " View size: " + Size.ToString();
			}

			Refresh();
		}

		private void View_GLInitialized(object sender, EventArgs e)
		{
			GL.Enable(EnableCap.DepthTest);

			// GL.ClearColor has two overloads, and if this class' ClearColor field is
			// passed in, the compiler tries to use the one taking a System.Drawing.Color
			// parameter instead of the one taking an OpenTK.Graphics.Color4. Using the
			// float signature therefore avoids an unnecessary System.Drawing reference.
			GL.ClearColor(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);

			_initTime = DateTime.Now;

			Shader.GetGlslVersion(out int major, out int minor);

			if (major >= 3 && minor >= 3)
			{
				Shader = new Shader(VertexShaderSource330, FragmentShaderSource330);
			}
			else
			{
				// TODO: Bring 1.30 shaders over from CSharpGlTest project.
			}

			var map = (Parent as Viewport).Map;
			map.Renderables.Add(new Renderable());
		}
	}
}
