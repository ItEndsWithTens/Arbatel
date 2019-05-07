using Eto.Drawing;

namespace Arbatel.Graphics
{
	public static class TextureGenerator
	{
		public static string MissingTextureName { get; set; } = "ARBATEL_MISSING_TEXTURE";

		public static Texture GenerateGrid()
		{
			return GenerateGrid(Colors.Magenta);
		}
		public static Texture GenerateGrid(Color colorB)
		{
			return GenerateGrid(Colors.Black, colorB);
		}
		public static Texture GenerateGrid(Color colorA, Color colorB)
		{
			return GenerateGrid(16, colorA, colorB);
		}
		public static Texture GenerateGrid(int size, Color colorA, Color colorB)
		{
			var texture = new Texture(size * 2, size * 2, PixelFormat.Format32bppRgba)
			{
				Name = MissingTextureName
			};

			using (BitmapData data = texture.Bitmap.Lock())
			{
				bool useA = true;

				for (int y = 0; y < texture.Bitmap.Height; y++)
				{
					if (y % size == 0)
					{
						useA = !useA;
					}

					for (int x = 0; x < texture.Bitmap.Width; x++)
					{
						if (x % size == 0)
						{
							useA = !useA;
						}

						if (useA)
						{
							data.SetPixel(x, y, colorA);
						}
						else
						{
							data.SetPixel(x, y, colorB);
						}
					}
				}
			}

			return texture;
		}

		public static Texture GenerateSolid(Color color)
		{
			var texture = new Texture(16, 16, PixelFormat.Format32bppRgba)
			{
				Name = MissingTextureName
			};

			using (BitmapData data = texture.Bitmap.Lock())
			{
				for (int y = 0; y < texture.Bitmap.Height; y++)
				{
					for (int x = 0; x < texture.Bitmap.Width; x++)
					{
						data.SetPixel(x, y, color);
					}
				}
			}

			return texture;
		}
	}
}
