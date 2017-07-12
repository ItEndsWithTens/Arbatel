using Eto;
using Eto.Gl;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Temblor.Controls
{
	public class Viewport : GLSurface
	{
		// Explicitly choosing an eight-bit stencil buffer prevents visual artifacts
		// on the Mac platform; the GraphicsMode defaults are apparently insufficient.
		private static GraphicsMode _graphicsMode = new GraphicsMode(new ColorFormat(32), 8, 8, 8);

		public Color4 ClearColor = new Color4(1.0f, 1.0f, 0.0f, 1.0f);

		public Viewport() : this(_graphicsMode, 3, 3, GraphicsContextFlags.Default)
		{
		}

		public Viewport(GraphicsMode mode, int major, int minor, GraphicsContextFlags flags) :
			base(mode, major, minor, flags)
		{
			Draw += Viewport_Draw;
			GLInitalized += Viewport_GLInitalized;
		}

		private void Viewport_GLInitalized(object sender, EventArgs e)
		{
			GL.Enable(EnableCap.DepthTest);

			GL.Viewport(0, 0, Width, Height);

			// GL.ClearColor has two overloads, and if this class' ClearColor field is
			// passed in, the compiler tries to use the one taking a System.Drawing.Color
			// parameter instead of the one taking an OpenTK.Graphics.Color4. Using the
			// float signature therefore avoids an unnecessary System.Drawing reference.
			GL.ClearColor(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);
		}

		public void Clear()
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			SwapBuffers();
		}

		private void Viewport_Draw(object sender, EventArgs e)
		{
			Clear();
		}
	}
}
