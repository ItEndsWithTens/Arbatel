using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace Arbatel.Graphics
{
	/// <summary>
	/// A shader that draws polygons as wire frames, using their underlying
	/// vertex colors.
	/// </summary>
	public class WireframeShader : Shader
	{
		public WireframeShader(int major, int minor) : base(
			major >= 3 && minor >= 3 ? "Flat330.vert" : "Flat120.vert",
			major >= 3 && minor >= 3 ? "Flat330.frag" : "Flat120.frag")
		{
		}

		private List<int> _indexCounts = new List<int>();
		private List<IntPtr> _indexOffsets = new List<IntPtr>();

		public override void DrawModel(IEnumerable<Renderable> renderables, Camera camera)
		{
			base.DrawModel(renderables, camera);

			foreach (Renderable r in renderables)
			{
				SetUniform(LocationModelMatrix, r.ModelMatrix);

				_indexCounts.Clear();
				_indexOffsets.Clear();
				foreach (Polygon p in r.Polygons)
				{
					_indexCounts.Add(p.LineLoopIndices.Count);
					_indexOffsets.Add(p.LineLoopIndexOffset);
				}

				GL.MultiDrawElements(
					PrimitiveType.LineLoop,
					_indexCounts.ToArray(),
					DrawElementsType.UnsignedInt,
					_indexOffsets.ToArray(),
					_indexOffsets.Count);
			}
		}

		public override void DrawWorld(IEnumerable<Renderable> renderables, Camera camera)
		{
			base.DrawWorld(renderables, camera);

			_indexCounts.Clear();
			_indexOffsets.Clear();
			foreach (Renderable r in renderables)
			{
				foreach (Polygon p in r.Polygons)
				{
					_indexCounts.Add(p.LineLoopIndices.Count);
					_indexOffsets.Add(p.LineLoopIndexOffset);
				}
			}

			GL.MultiDrawElements(
				PrimitiveType.LineLoop,
				_indexCounts.ToArray(),
				DrawElementsType.UnsignedInt,
				_indexOffsets.ToArray(),
				_indexOffsets.Count);
		}
	}
}
