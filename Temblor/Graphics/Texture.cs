using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Graphics
{
	public class Texture : Bitmap
	{
		public string Name;

		public Texture() : base(16, 16, PixelFormat.Format24bppRgb)
		{
		}
		public Texture(int width, int height) : base(width, height, PixelFormat.Format24bppRgb)
		{
		}
		public Texture(int width, int height, PixelFormat format) : base(width, height, format)
		{
		}
		public Texture(string fileName) : base(fileName)
		{
		}
		public Texture(Stream stream) : base(stream)
		{
		}

		/// <summary>
		/// Convert this Texture into a block of raw, uncompressed pixel data.
		/// </summary>
		/// <param name="format">The pixel format of the returned data.</param>
		/// <param name="flip">Whether to flip the image vertically, since RGB is often "upside down" in memory.</param>
		/// <returns>The uncompressed pixel data for this Texture as an array of bytes.</returns>
		public byte[] ToUncompressed(PixelFormat format = PixelFormat.Format24bppRgb, bool flip = false)
		{
			var components = 3;
			if (format == PixelFormat.Format32bppRgb || format == PixelFormat.Format32bppRgba)
			{
				components++;
			}

			var bytes = new byte[Width * Height * components];

			var pitch = Width * components;

			for (var y = 0; y < Height; y++)
			{
				var line = pitch;
				if (flip)
				{
					line *= y;
				}
				else
				{
					line *= Height - 1 - y;
				}

				for (var x = 0; x < Width; x++)
				{
					var color = GetPixel(x, y);

					var pixel = x * components;

					bytes[line + pixel + 0] = Convert.ToByte(color.Rb);
					bytes[line + pixel + 1] = Convert.ToByte(color.Gb);
					bytes[line + pixel + 2] = Convert.ToByte(color.Bb);
					if (format == PixelFormat.Format32bppRgba)
					{
						bytes[line + pixel + 3] = Convert.ToByte(color.Ab);
					}
					else if (format == PixelFormat.Format32bppRgb)
					{
						bytes[line + pixel + 3] = 0;
					}
				}
			}

			return bytes;
		}
	}
}
