using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbatel.Graphics
{
	/// <summary>
	/// A shader that draws polygons grouped by texture.
	/// </summary>
	public class SingleTextureShader : Shader
	{
		public SingleTextureShader(int major, int minor) : base(
			major >= 3 && minor >= 3 ? "SingleTexture330.vert" : "SingleTexture120.vert",
			major >= 3 && minor >= 3 ? "SingleTexture330.frag" : "SingleTexture120.frag")
		{
		}

		private List<int> _indexCounts = new List<int>();
		private List<IntPtr> _indexOffsets = new List<IntPtr>();

		public override void DrawModel(IEnumerable<Renderable> renderables, Camera camera)
		{
			base.DrawModel(renderables, camera);

			GL.ActiveTexture(TextureUnit.Texture0);

			IEnumerable<IGrouping<Texture, (Polygon, Renderable)>> byTexture =
				camera.GetVisiblePolygons(renderables)
				.GroupBy(pair => pair.Item1.Texture)
				.OrderBy(t => t.Key.Translucent);

			foreach (IGrouping<Texture, (Polygon, Renderable)> t in byTexture)
			{
				GL.BindTexture(TextureTarget.Texture2D, BackEnd.Textures[t.Key.Name.ToLower()]);

				IEnumerable<IGrouping<Renderable, (Polygon, Renderable)>> byRenderable =
					t
					.GroupBy(pair => pair.Item2);

				foreach (IGrouping<Renderable, (Polygon, Renderable)> pair in byRenderable)
				{
					SetUniform(LocationModelMatrix, pair.Key.ModelMatrix);

					_indexCounts.Clear();
					_indexOffsets.Clear();
					foreach ((Polygon p, _) in t)
					{
						_indexCounts.Add(p.Indices.Count);
						_indexOffsets.Add(p.IndexOffset);
					}

					GL.MultiDrawElements(
						PrimitiveType.Triangles,
						_indexCounts.ToArray(),
						DrawElementsType.UnsignedInt,
						_indexOffsets.ToArray(),
						_indexCounts.Count);
				}
			}
		}

		public override void DrawWorld(IEnumerable<Renderable> renderables, Camera camera)
		{
			base.DrawWorld(renderables, camera);

			IEnumerable<IGrouping<Texture, (Polygon, Renderable)>> byTexture =
				camera.GetVisiblePolygons(renderables)
				.GroupBy(pair => pair.Item1.Texture)
				.OrderBy(t => t.Key.Translucent);

			foreach (IGrouping<Texture, (Polygon, Renderable)> t in byTexture)
			{
				GL.BindTexture(TextureTarget.Texture2D, BackEnd.Textures[t.Key.Name.ToLower()]);

				_indexCounts.Clear();
				_indexOffsets.Clear();
				foreach ((Polygon p, _) in t)
				{
					_indexCounts.Add(p.Indices.Count);
					_indexOffsets.Add(p.IndexOffset);
				}

				GL.MultiDrawElements(
					PrimitiveType.Triangles,
					_indexCounts.ToArray(),
					DrawElementsType.UnsignedInt,
					_indexOffsets.ToArray(),
					_indexCounts.Count);
			}
		}
	}
}
