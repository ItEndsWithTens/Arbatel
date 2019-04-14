using Arbatel.Graphics;
using Eto.Gl;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Arbatel.Controls
{
	public class OpenGLView : View
	{
		public new OpenGLBackEnd BackEnd
		{
			get { return (OpenGLBackEnd)base.BackEnd; }
			set { base.BackEnd = value; }
		}

		private bool _controlReady = false;
		public bool ControlReady
		{
			get { return _controlReady; }
			private set
			{
				_controlReady = value;

				SetUpGL();
			}
		}

		private bool _openGLContextReady = false;
		public bool OpenGLContextReady
		{
			get { return _openGLContextReady; }
			private set
			{
				_openGLContextReady = value;

				SetUpGL();
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

		// This was previously accomplished by overriding OnEnabledChanged, but
		// Eto.Gl is currently built against Eto 2.4.0, whose MacView class, the
		// base for GLSurface, doesn't support that event.
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
				}
			}
		}

		public new ShadingStyle ShadingStyle
		{
			get { return base.ShadingStyle; }
			set
			{
				base.ShadingStyle = value;

				switch (value)
				{
					case ShadingStyle.Textured:
						BackEnd.DrawMap = BackEnd.DrawMapTextured;
						break;
					case ShadingStyle.Flat:
						BackEnd.DrawMap = BackEnd.DrawMapFlat;
						break;
					case ShadingStyle.Wireframe:
						BackEnd.DrawMap = BackEnd.DrawMapWireframe;
						break;
					default:
						throw new ArgumentException("Invalid ShadingStyle!");
				}
			}
		}

		// Explicitly choosing an eight-bit stencil buffer prevents visual artifacts
		// on the Mac platform; the GraphicsMode defaults are apparently insufficient.
		private static GraphicsMode mode = new GraphicsMode(new ColorFormat(32), 8, 8, 8);

		public OpenGLView() : this(mode)
		{
		}
		public OpenGLView(GraphicsMode mode) : base()
		{
			var surface = new GLSurface(
				mode,
				OpenGLBackEnd.DesiredMajorVersion,
				OpenGLBackEnd.DesiredMinorVersion,
				GraphicsContextFlags.ForwardCompatible);

			surface.GLInitalized += (sender, e) => OpenGLContextReady = true;

			// The Draw event of this control's GLSurface child will clear the
			// OpenGL viewport with its clear color. That event might be raised
			// in situations like a window resize, and may very well supersede
			// the call to Refresh by the GraphicsClock.Elapsed handler of this
			// class's base. Calling Refresh manually here prevents flickering.
			surface.Draw += (sender, e) => Refresh();

			Content = surface;

			LoadComplete += (sender, e) => ControlReady = true;
		}

		public override void Refresh()
		{
			GL.Viewport(0, 0, Width, Height);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			if (Map != null && Map.InitializedInBackEnd)
			{
				Camera.AspectRatio = (float)Width / (float)Height;

				BackEnd.DrawMap(Map, this, Camera);
			}

			((GLSurface)Content).SwapBuffers();
		}

		private void SetUpGL()
		{
			if (!(ControlReady && OpenGLContextReady))
			{
				return;
			}

			((GLSurface)Content).MakeCurrent();

			BackEnd.SetUp();

			OpenGLReady = true;

			OpenGLBackEnd.SetUpTextured(this);
		}
	}
}
