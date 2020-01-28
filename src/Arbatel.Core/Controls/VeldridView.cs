using Arbatel.Graphics;
using Eto.Veldrid;
using System;
using Veldrid;

namespace Arbatel.Controls
{
	public class VeldridView : View
	{
		public new VeldridBackEnd BackEnd
		{
			get { return (VeldridBackEnd)base.BackEnd; }
			set { base.BackEnd = value; }
		}

		private bool _veldridReady = false;
		public bool VeldridReady
		{
			get { return _veldridReady; }
			private set
			{
				_veldridReady = value;

				SetUpVeldrid();
			}
		}

		private bool _viewReady = false;
		public bool ViewReady
		{
			get { return _viewReady; }
			private set
			{
				_viewReady = value;

				SetUpVeldrid();
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

		public RgbaFloat ClearColor { get; set; } = RgbaFloat.Pink;

		public VeldridView()
		{
			Shown += VeldridView_Shown;

			var gdOptions = new GraphicsDeviceOptions(
				false,
				PixelFormat.R32_Float,
				false,
				ResourceBindingModel.Improved);

			var tkOptions = new OpenTKOptions(
				new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(32), 8));

			var surface = new VeldridSurface(VeldridSurface.PreferredBackend, gdOptions, tkOptions);
			surface.VeldridInitialized += (sender, e) => VeldridReady = true;
			surface.Draw += (sender, e) => Refresh();

			Content = surface;
		}

		public override void Refresh()
		{
			var s = (VeldridSurface)Content;

			BackEnd.CommandList = BackEnd.Factory.CreateCommandList();

			BackEnd.CommandList.Begin();

			BackEnd.CommandList.SetFramebuffer(s.Swapchain.Framebuffer);
			BackEnd.CommandList.ClearColorTarget(0, ClearColor);
			BackEnd.CommandList.ClearDepthStencil(1.0f);

			if (Map != null && Map.InitializedInBackEnd)
			{
				Camera.AspectRatio = (float)Width / (float)Height;

				BackEnd.DrawMap(Map, this, Camera);
			}

			BackEnd.CommandList.End();

			s.GraphicsDevice.SubmitCommands(BackEnd.CommandList);
			s.GraphicsDevice.SwapBuffers(s.Swapchain);

			BackEnd.CommandList.Dispose();
		}

		private void SetUpVeldrid()
		{
			if (!(ViewReady && VeldridReady))
			{
				return;
			}

			BackEnd.SetUp((VeldridSurface)Content);

			BackEnd.SetUpTextured(this);

			GraphicsClock.Start();
		}

		private void VeldridView_Shown(object sender, EventArgs e)
		{
			ViewReady = true;

			// Veldrid setup should only happen once, but won't work properly in
			// all backends (namely OpenGL) if it's done before this control is
			// shown. The LoadComplete event is therefore ruled out, with the
			// Shown event taking its place. Then, in order to prevent setup
			// from occurring every time users Tab cycle to this view, this
			// event handler removes itself.
			((VeldridView)sender).Shown -= VeldridView_Shown;
		}
	}
}
