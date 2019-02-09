using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace Arbatel.Graphics
{
	/// <summary>
	/// A shader that draws polygons using their underlying vertex colors.
	/// </summary>
	public class FlatShader : Shader
	{
		public FlatShader(int major, int minor) : base(
			major >= 3 && minor >= 3 ? "Flat330.vert" : "Flat120.vert",
			major >= 3 && minor >= 3 ? "Flat330.frag" : "Flat120.frag")
		{
		}

		public override void DrawModel(IEnumerable<Renderable> renderables, Camera camera)
		{
			base.DrawModel(renderables, camera);

			foreach (Renderable r in renderables)
			{
				SetUniform(LocationModelMatrix, r.ModelMatrix);

				GL.DrawElements(PrimitiveType.Triangles, r.Indices.Count, DrawElementsType.UnsignedInt, r.IndexOffset);
			}
		}

		private List<int> _indexCounts = new List<int>();
		private List<IntPtr> _indexOffsets = new List<IntPtr>();
		public override void DrawWorld(IEnumerable<Renderable> renderables, Camera camera)
		{
			base.DrawWorld(renderables, camera);

			_indexCounts.Clear();
			_indexOffsets.Clear();
			foreach (Renderable r in renderables)
			{
				_indexCounts.Add(r.Indices.Count);
				_indexOffsets.Add(r.IndexOffset);
			}

			GL.MultiDrawElements(
				PrimitiveType.Triangles,
				_indexCounts.ToArray(),
				DrawElementsType.UnsignedInt,
				_indexOffsets.ToArray(),
				_indexOffsets.Count);
		}
	}
}
