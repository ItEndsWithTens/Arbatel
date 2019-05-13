using Arbatel.Graphics;
using Eto.Forms;
using System;

namespace Arbatel.Controls
{
	public class VeldridViewport : Viewport
	{
		public new VeldridBackEnd BackEnd
		{
			get { return (VeldridBackEnd)base.BackEnd; }
			set { base.BackEnd = value; }
		}

		public VeldridViewport() : base(new VeldridBackEnd())
		{
			var veldridView = new VeldridView3d
			{
				BackEnd = BackEnd,
				Enabled = false,
				Visible = false
			};

			veldridView.Updated += VeldridView_Updated;

			Views.Add(Views.Count, (veldridView, "3D Wireframe", BackEnd.SetUpWireframe));
			Views.Add(Views.Count, (veldridView, "3D Flat", BackEnd.SetUpFlat));
			Views.Add(Views.Count, (veldridView, "3D Textured", BackEnd.SetUpTextured));

			UpdateViews();
		}

		private void VeldridView_Updated(object sender, EventArgs e)
		{
			// Re-execute the current set up method to account for any changes
			// to settings like color scheme.
			(Control Control, _, Action<Control> SetUp) = Views[View];
			SetUp.Invoke(Control);
		}
	}
}
