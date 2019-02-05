using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Linq;

namespace Arbatel.Graphics
{
	/// <summary>
	/// A shader that applies a solid color to a surface.
	/// </summary>
	public class FlatShader : Shader
	{
		public FlatShader(int major, int minor) : base(
			major >= 3 ? "Flat330.vert" : "Flat120.vert",
			minor >= 3 ? "Flat330.frag" : "Flat120.frag")
		{
		}

		public override void Draw(IEnumerable<Renderable> renderables, Camera camera)
		{
			if (renderables.Count() == 0)
			{
				return;
			}

			base.Draw(renderables, camera);

			foreach (Renderable r in renderables)
			{
				SetUniform(LocationModelMatrix, r.ModelMatrix);

				GL.DrawElements(PrimitiveType.Triangles, r.Indices.Count, DrawElementsType.UnsignedInt, r.IndexOffset);
			}
		}
	}
}
