using Eto;
using Eto.Gl;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Controls
{
	public class Viewport : GLSurface
	{
		// Explicitly choosing an eight-bit stencil buffer prevents visual artifacts
		// on the Mac platform; the GraphicsMode defaults are apparently insufficient.
		private static GraphicsMode _graphicsMode = new GraphicsMode(new ColorFormat(32), 8, 8, 8);

		public Viewport() : this(_graphicsMode, 3, 3, GraphicsContextFlags.Default)
		{
		}

		public Viewport(GraphicsMode mode, int major, int minor, GraphicsContextFlags flags) :
			base(mode, major, minor, flags)
		{

		}

		public bool ViewportIsInitialized = false;

		public void Update()
		{
			if (!IsInitialized)
			{
				return;
			}

			if (!ViewportIsInitialized)
			{
				Init();
			}
		}

		protected override void OnDraw(EventArgs e)
		{
			
		}

		protected override void OnShown(EventArgs e)
		{
			
		}

		private void Init()
		{
			Draw += (object sender, EventArgs e) => { Update(); };
		}
	}
}
