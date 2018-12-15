using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbatel.Graphics
{
	public class Texture : Bitmap
	{
		public string Name;

		public Texture() : base(16, 16, PixelFormat.Format24bppRgb)
		{
		}
		public Texture(int width, int height) : this(width, height, PixelFormat.Format24bppRgb)
		{
		}
		public Texture(int width, int height, PixelFormat format) : base(width, height, format)
		{
		}
		public Texture(string filename) : base(filename)
		{
		}
		public Texture(Stream stream) : base(stream)
		{
		}
		public Texture(Texture texture) : base(texture)
		{
			Name = texture.Name;
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

			// Direct access of this Bitmap's data by pointer is safe since Eto
			// currently only supports 8bpc pixel formats. The component and
			// pixel offsets are therefore predictable.
			unsafe
			{
				using (BitmapData raw = Lock())
				{
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
							var pixel = x * components;

							var ofsR = line + pixel + 0;
							var ofsG = line + pixel + 1;
							var ofsB = line + pixel + 2;
							var ofsA = line + pixel + 3;

							bytes[ofsR] = ((byte*)raw.Data.ToPointer())[ofsR];
							bytes[ofsG] = ((byte*)raw.Data.ToPointer())[ofsG];
							bytes[ofsB] = ((byte*)raw.Data.ToPointer())[ofsB];

							if (format == PixelFormat.Format32bppRgba)
							{
								bytes[ofsA] = ((byte*)raw.Data.ToPointer())[ofsA];
							}
							else if (format == PixelFormat.Format32bppRgb)
							{
								bytes[ofsA] = 0;
							}
						}
					}

					return bytes;
				}
			}
		}
	}
}
