using Eto.Gl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Utilities
{
	public class GlUtilities : GLSurface
	{
		/// <summary>
		/// Globally initialize OpenGL for this application.
		/// </summary>
		/// <remarks>
		/// This allows OpenGL to be prepped before a GLSurface is shown to the
		/// user. Forgetting to call this method is harmless, and only results
		/// in a cosmetic issue in the form of a delay between showing a GL
		/// control and seeing its content.
		/// </remarks>
		public void InitGl()
		{
			// There seems to be no direct way to get OpenGL up and running
			// without showing a GLSurface on screen, which means an ugly,
			// visible delay before anything is drawn. MakeCurrent gets the
			// party started, at least as a side effect.
			//
			// Doing this in the View class's constructor doesn't work quite
			// right, perhaps for thread related reasons.
			MakeCurrent();
		}
	}
}
