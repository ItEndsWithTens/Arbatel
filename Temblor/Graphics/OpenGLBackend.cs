using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;

namespace Temblor.Graphics
{
	public class OpenGL4BackEnd : Backend
	{
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
