using Arbatel.Controls;
using Arbatel.Formats;
using Eto.Gl;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace Arbatel.Graphics
{
	public class OpenGL4BackEnd : BackEnd
	{
		public override void DrawRenderable(Renderable r, Dictionary<ShadingStyle, Shader> shaders, ShadingStyle style, object surface, Camera camera)
		{
			ShadingStyle actualStyle = r.ShadingStyleDict[style];

			shaders[actualStyle].Draw(r, surface as GLSurface, camera);
		}

		public override void InitRenderable(Renderable renderable, Shader shader, object surface)
		{
			var glSurface = (GLSurface)surface;

			glSurface.MakeCurrent();

			Buffers b;

			if (renderable.Buffers.ContainsKey(glSurface))
			{
				b = renderable.Buffers[glSurface];
			}
			else
			{
				b = new Buffers();

				renderable.Buffers.Add(glSurface, b);
			}

			GL.BindVertexArray(b.Vao);
			GL.BindBuffer(BufferTarget.ArrayBuffer, b.Vbo);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, b.Ebo);

			// Configure position element.
			int positionLocation = GL.GetAttribLocation(shader.Program, "position");
			GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, Vertex.MemorySize, 0);
			GL.EnableVertexAttribArray(positionLocation);

			// Normal
			int normalLocation = GL.GetAttribLocation(shader.Program, "normal");
			GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, Vertex.MemorySize, sizeof(float) * 3);
			GL.EnableVertexAttribArray(normalLocation);

			// Color
			int colorLocation = GL.GetAttribLocation(shader.Program, "color");
			GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, Vertex.MemorySize, sizeof(float) * 6);
			GL.EnableVertexAttribArray(colorLocation);

			GL.BufferData(
				BufferTarget.ArrayBuffer,
				Vertex.MemorySize * renderable.Vertices.Count,
				renderable.Vertices.ToArray(),
				BufferUsageHint.StaticDraw);

			GL.BufferData(
				BufferTarget.ElementArrayBuffer,
				4 * renderable.Indices.Count,
				renderable.Indices.ToArray(),
				BufferUsageHint.StaticDraw);

			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		public override void InitTextures(TextureDictionary dictionary)
		{
			foreach (var t in dictionary.Values)
			{
				GL.GenTextures(1, out int id);
				Textures.Add(t.Name, id);

				GL.BindTexture(TextureTarget.Texture2D, id);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, t.Width, t.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, t.ToUncompressed(Eto.Drawing.PixelFormat.Format32bppRgba, flip: true));
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}
		}
	}
}
