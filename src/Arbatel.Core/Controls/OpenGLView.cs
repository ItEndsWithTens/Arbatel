using Arbatel.Graphics;
using Eto;
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

		public Color4 ClearColor { get; set; }
		public PolygonMode PolygonMode { get; set; }

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

			Shaders.Clear();
			Shaders.Add(ShadingStyle.Wireframe, new Shader() { BackEnd = BackEnd });
			Shaders.Add(ShadingStyle.Flat, new FlatShader(glslMajor, glslMinor) { BackEnd = BackEnd });
			Shaders.Add(ShadingStyle.Textured, new SingleTextureShader(glslMajor, glslMinor) { BackEnd = BackEnd });

			// FIXME: Causes InvalidEnum from GL.GetError, at least on my OpenGL 2.1, GLSL 1.2, Intel HD Graphics laptop.
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode);

			// TEST. Also remember to switch Camera to use left-handed, Z-up position at some point.
			Camera.Position = new Vector3(256.0f, 1024.0f, 1024.0f);
			Camera.Pitch = -30.0f;

			OpenGLReady = true;
		}
	}
}
