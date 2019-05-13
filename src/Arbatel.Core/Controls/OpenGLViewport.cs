using Arbatel.Graphics;
using Eto.Forms;
using System;

namespace Arbatel.Controls
{
	public class OpenGLViewport : Viewport
	{
		public OpenGLViewport() : base(new OpenGLBackEnd())
		{
			var oglView = new OpenGLView3d()
			{
				BackEnd = (OpenGLBackEnd)BackEnd,
				Enabled = false,
				Visible = false
			};

			oglView.Updated += OglView_Updated;

			Views.Add(Views.Count, (oglView, "3D Wireframe", OpenGLBackEnd.SetUpWireframe));
			Views.Add(Views.Count, (oglView, "3D Flat", OpenGLBackEnd.SetUpFlat));
			Views.Add(Views.Count, (oglView, "3D Textured", OpenGLBackEnd.SetUpTextured));

			UpdateViews();
		}

		private void OglView_Updated(object sender, System.EventArgs e)
		{
			// Re-execute the current set up method to account for any changes
			// to settings like color scheme.
			(Control Control, _, Action<Control> SetUp) = Views[View];
			SetUp.Invoke(Control);
		}
	}
}
