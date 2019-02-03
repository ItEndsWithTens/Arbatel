using Eto.Drawing;
using System;
using System.IO;

namespace Arbatel.Graphics
{
	public class Texture : Bitmap
	{
		public string Name { get; set; }

		public bool Translucent { get; set; } = false;

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
			int components = 3;
			if (format == PixelFormat.Format32bppRgb || format == PixelFormat.Format32bppRgba)
			{
				components++;
			}

			byte[] bytes = new byte[Width * Height * components];

			int pitch = Width * components;

			// An unsafe block is necessary for speed; Eto's Bitmap GetPixel is
			// too slow for grabbing every pixel of an image, but the BitmapData
			// version of GetPixel flies. Direct access to the data buffer by
			// pointer is also possible, but exposes each platform's underlying
			// pixel formats, e.g. RGB, BGR, RGBA, ARGB, and would then demand
			// per-platform branching to reorder the components properly.
			unsafe
			{
				using (BitmapData raw = Lock())
				{
					for (int y = 0; y < Height; y++)
					{
						int line = pitch;
						if (flip)
						{
							line *= y;
						}
						else
						{
							line *= Height - 1 - y;
						}

						for (int x = 0; x < Width; x++)
						{
							Color pixel = raw.GetPixel(x, y);

							int offset = x * components;

							bytes[line + offset + 0] = Convert.ToByte(pixel.Rb);
							bytes[line + offset + 1] = Convert.ToByte(pixel.Gb);
							bytes[line + offset + 2] = Convert.ToByte(pixel.Bb);

							if (format == PixelFormat.Format32bppRgba)
							{
								bytes[line + offset + 3] = Convert.ToByte(pixel.Ab);
							}
							else if (format == PixelFormat.Format32bppRgb)
							{
								bytes[line + offset + 3] = 0;
							}
						}
					}

					return bytes;
				}
			}
		}
	}
}
