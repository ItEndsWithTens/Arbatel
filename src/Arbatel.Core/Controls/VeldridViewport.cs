using Arbatel.Graphics;

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

			Views.Add(Views.Count, (veldridView, "3D Wireframe", BackEnd.SetUpWireframe));
			Views.Add(Views.Count, (veldridView, "3D Flat", BackEnd.SetUpFlat));
			Views.Add(Views.Count, (veldridView, "3D Textured", BackEnd.SetUpTextured));

			UpdateViews();
		}
	}
}
