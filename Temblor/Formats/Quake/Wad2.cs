using Eto.Drawing;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Temblor.Graphics;

namespace Temblor.Formats.Quake
{
	public class Wad2 : TextureCollection
	{
		public Palette Palette;

		public List<string> Translucents;

		public Wad2() : base()
		{
			Palette = new Palette();
			Translucents = new List<string>()
			{
				"trigger",
				"clip"
			};
		}
		public Wad2(string fileName, Palette palette) : this(new FileStream(fileName, FileMode.Open, FileAccess.Read), palette)
		{
		}
		public Wad2(Stream stream, Palette palette) : this()
		{
			Palette = palette;

			var bytes = new byte[stream.Length];

			using (var br = new BinaryReader(stream))
			{
				bytes = br.ReadBytes((int)stream.Length);
			}

			string type = "";
			for (var i = 0; i < 4; i++)
			{
				type += (char)bytes[i];
			}

			if (type != "WAD2")
			{
				string message = "Couldn't find WAD2 header";
				if (stream is FileStream)
				{
					message += " in " + (stream as FileStream).Name + "!";
				}
				else
				{
					message += "!";
				}

				throw new InvalidDataException(message);
			}

			var textureCount = BitConverter.ToInt32(bytes, 4);

			var dirOffset = BitConverter.ToInt32(bytes, 8);

			for (var i = 0; i < textureCount; i++)
			{
				var entrySize = 32;

				var entryOffset = dirOffset + (entrySize * i);

				Int32 dataOffset = BitConverter.ToInt32(bytes, entryOffset + 0);
				Int32 sizeInWad = BitConverter.ToInt32(bytes, entryOffset + 4);
				Int32 sizeInMemory = BitConverter.ToInt32(bytes, entryOffset + 8);
				char entryType = BitConverter.ToChar(bytes, entryOffset + 12);
				char compression = BitConverter.ToChar(bytes, entryOffset + 13);
				Int16 dummy = BitConverter.ToInt16(bytes, entryOffset + 14);
				var sb = new StringBuilder();
				for (var j = 0; j < 16; j++)
				{
					var c = (char)bytes[entryOffset + 16 + j];
					if (c == '\0')
					{
						break;
					}
					else
					{
						sb.Append(c);
					}
				}
				string name = sb.ToString();

				// 16 bytes for texture name
				// 4 byte width
				// 4 byte height
				// 4 byte offset (from start of entry) of full res texture
				// 4 byte offset of first mip level
				// 4 byte offset of second mip level
				// 4 byte offset of third mip level
				// width x height bytes for full res texture
				// (width / 2) x (height / 2) bytes for first mip level
				// (width / 4) x (height / 4) bytes for second mip level
				// (width / 8) x (height / 8) bytes for third mip level
				var width = BitConverter.ToInt32(bytes, dataOffset + 16);
				var height = BitConverter.ToInt32(bytes, dataOffset + 20);
				var fullResOffset = BitConverter.ToInt32(bytes, dataOffset + 24);

				var texture = new Texture(width, height, PixelFormat.Format32bppRgba) { Name = name };

				using (BitmapData data = texture.Lock())
				{
					for (var y = 0; y < height; y++)
					{
						for (var x = 0; x < width; x++)
						{
							Color color = Palette[bytes[dataOffset + fullResOffset + (width * y) + x]];

							if (Translucents.Contains(texture.Name))
							{
								color.A = 0.5f;
							}
							else if (texture.Name.StartsWith("{") && color == Palette[255])
							{
								color.A = 0.0f;
							}

							data.SetPixel(x, y, color);
						}
					}
				}

				Textures.Add(name, texture);
			}
		}
	}
}
