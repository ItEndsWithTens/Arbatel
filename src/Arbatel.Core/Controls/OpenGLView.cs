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
	public class OpenGLView : View
	{
		public static Action<Control> SetUpWireframe { get; } = new Action<Control>(control =>
		{
			if (control is OpenGLView o && o.OpenGLReady)
			{
				o.ShadingStyle = ShadingStyle.Wireframe;

				GL.Disable(EnableCap.CullFace);

				GL.Disable(EnableCap.Blend);

				GL.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			}
		});

		public static Action<Control> SetUpFlat { get; } = new Action<Control>(control =>
		{
			if (control is OpenGLView o && o.OpenGLReady)
			{
				o.ShadingStyle = ShadingStyle.Flat;

				GL.Enable(EnableCap.CullFace);
				GL.CullFace(CullFaceMode.Back);

				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

				GL.ClearColor(0.0f, 1.0f, 0.0f, 1.0f);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
		});

		public static Action<Control> SetUpTextured { get; } = new Action<Control>(control =>
		{
			if (control is OpenGLView o && o.OpenGLReady)
			{
				o.ShadingStyle = ShadingStyle.Textured;

				GL.Enable(EnableCap.CullFace);
				GL.CullFace(CullFaceMode.Back);

				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

				GL.ClearColor(0.0f, 0.0f, 1.0f, 1.0f);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
		});

		// OpenGL 3.0 is the lowest version that has all the features this
		// project needs built in. With the appropriate extensions 2.X is also
		// usable; unfortunately, requesting a context of less than 3.2 in macOS
		// will produce a 2.1 context without said extensions.
		public static int GLMajor { get; } = EtoEnvironment.Platform.IsMac ? 3 : 2;
		public static int GLMinor { get; } = EtoEnvironment.Platform.IsMac ? 2 : 0;

		/// <summary>
		/// Extensions required to run under OpenGL versions below 3.0.
		/// </summary>
		public static ReadOnlyCollection<string> RequiredGLExtensions { get; } =
			new List<string>
			{
				"ARB_vertex_array_object",
				"ARB_framebuffer_object",
				"ARB_uniform_buffer_object"
			}.AsReadOnly();

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
						BackEnd.DrawMap = (BackEnd as OpenGL4BackEnd).DrawMapTextured;
						break;
					default:
						BackEnd.DrawMap = (BackEnd as OpenGL4BackEnd).DrawMapFlat;
						break;
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
			var surface = new GLSurface(mode, GLMajor, GLMinor, GraphicsContextFlags.ForwardCompatible);

			surface.GLInitalized += GLSurface_GLInitialized;
			surface.Draw += GLSurface_Draw;

			Content = surface;
		}

		public void Clear()
		{
			GL.Viewport(0, 0, Width, Height);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		public override void Refresh()
		{
			Clear();

			if (Map != null)
			{
				Camera.AspectRatio = (float)Width / (float)Height;

				BackEnd.DrawMap(Map, Shaders, ShadingStyle, this, Camera);
			}

			(Content as GLSurface).SwapBuffers();
		}

		private void GLSurface_Draw(object sender, EventArgs e)
		{
			// The Draw event of this control's GLSurface child will clear the
			// OpenGL viewport with its clear color. That event might be raised
			// in situations like a window resize, and may very well supersede
			// the call to Refresh by the GraphicsClock.Elapsed handler of this
			// class's base. Calling Refresh manually here prevents flickering.
			Refresh();
		}

		private void GLSurface_GLInitialized(object sender, EventArgs e)
		{
			string version = GL.GetString(StringName.Version);

			string[] split = version.Split('.', ' ');

			bool gotMajor = Int32.TryParse(split[0], out int glMajor);
			bool gotMinor = Int32.TryParse(split[1], out int glMinor);

			if (!(gotMajor && gotMinor))
			{
				throw new GraphicsException("Couldn't parse OpenGL version string!");
			}

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

			GL.Enable(EnableCap.DepthTest);

			GL.FrontFace(FrontFaceDirection.Ccw);

			(int glslMajor, int glslMinor) = Shader.GetGlslVersion();

			Shaders.Clear();
			Shaders.Add(ShadingStyle.Wireframe, new FlatShader(glslMajor, glslMinor) { BackEnd = BackEnd });
			Shaders.Add(ShadingStyle.Flat, new FlatShader(glslMajor, glslMinor) { BackEnd = BackEnd });
			Shaders.Add(ShadingStyle.Textured, new SingleTextureShader(glslMajor, glslMinor) { BackEnd = BackEnd });

			OpenGLReady = true;

			SetUpTextured(this);
		}
	}
}
