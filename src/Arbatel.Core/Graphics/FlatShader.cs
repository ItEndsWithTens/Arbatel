using OpenTK.Graphics.OpenGL4;
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

		public override void Draw(IEnumerable<Renderable> renderables, Camera camera)
		{
			base.Draw(renderables, camera);

			foreach (Renderable r in renderables)
			{
				SetUniform(LocationModelMatrix, r.ModelMatrix);

				GL.DrawElements(PrimitiveType.Triangles, r.Indices.Count, DrawElementsType.UnsignedInt, r.IndexOffset);
			}
		}
	}
}
