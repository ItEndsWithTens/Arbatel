using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Linq;

namespace Arbatel.Graphics
{
	/// <summary>
	/// A shader that draws polygons grouped by texture.
	/// </summary>
	public class SingleTextureShader : Shader
	{
		public int LocationBasisS { get; set; } = 0;
		public int LocationBasisT { get; set; } = 0;
		public int LocationOffset { get; set; } = 0;
		public int LocationScale { get; set; } = 0;
		public int LocationTextureWidth { get; set; } = 0;
		public int LocationTextureHeight { get; set; } = 0;

		public SingleTextureShader(int major, int minor) : base(
			major >= 3 && minor >= 3 ? "SingleTexture330.vert" : "SingleTexture120.vert",
			major >= 3 && minor >= 3 ? "SingleTexture330.frag" : "SingleTexture120.frag")
		{ 
			LocationBasisS = GL.GetUniformLocation(Program, "basisS");
			LocationBasisT = GL.GetUniformLocation(Program, "basisT");
			LocationOffset = GL.GetUniformLocation(Program, "offset");
			LocationScale = GL.GetUniformLocation(Program, "scale");
			LocationTextureWidth = GL.GetUniformLocation(Program, "textureWidth");
			LocationTextureHeight = GL.GetUniformLocation(Program, "textureHeight");
		}

		private List<(Polygon, Renderable)> VisiblePolygons { get; } = new List<(Polygon, Renderable)>();

		public override void Draw(IEnumerable<Renderable> renderables, Camera camera)
		{
			if (renderables.Count() == 0)
			{
				return;
			}

			base.Draw(renderables, camera);

			VisiblePolygons.Clear();
			foreach (Renderable r in renderables)
			{
				foreach (Polygon p in camera.VisiblePolygons(r))
				{
					VisiblePolygons.Add((p, r));
				}
			}

			GL.ActiveTexture(TextureUnit.Texture0);

			IEnumerable<IGrouping<Texture, (Polygon, Renderable)>> byTexture =
				VisiblePolygons
				.GroupBy(pair => pair.Item1.Texture)
				.OrderBy(t => t.Key.Translucent);

			foreach (IGrouping<Texture, (Polygon, Renderable)> t in byTexture)
			{
				SetUniform(LocationTextureWidth, (float)t.Key.Width);
				SetUniform(LocationTextureHeight, (float)t.Key.Height);

				GL.BindTexture(TextureTarget.Texture2D, BackEnd.Textures[t.Key.Name.ToLower()]);

				IEnumerable<IGrouping<Renderable, (Polygon, Renderable)>> byRenderable =
					t
					.GroupBy(pair => pair.Item2);

				foreach (IGrouping<Renderable, (Polygon, Renderable)> r in byRenderable)
				{
					SetUniform(LocationModelMatrix, r.Key.ModelMatrix);

					foreach ((Polygon p, _) in r)
					{
						SetUniform(LocationBasisS, p.BasisS);
						SetUniform(LocationBasisT, p.BasisT);
						SetUniform(LocationOffset, p.Offset);
						SetUniform(LocationScale, p.Scale);

						GL.DrawElements(PrimitiveType.Triangles, p.Indices.Count, DrawElementsType.UnsignedInt, p.IndexOffset);
					}
				}
			}
		}
	}
}
