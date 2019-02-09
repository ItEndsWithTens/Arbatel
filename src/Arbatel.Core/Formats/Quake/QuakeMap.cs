using Arbatel.UI;
using Arbatel.Utilities;
using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Arbatel.Formats.Quake
{
	public enum CollapseType
	{
		Position,
		Angle,
		Target
	}

	public class QuakeMap : Map
	{
		public QuakeMap() : base()
		{
		}
		public QuakeMap(QuakeMap map) : base(map)
		{
		}
		public QuakeMap(Stream stream, DefinitionDictionary definitions) : base(stream, definitions)
		{
			var sb = new StringBuilder();
			using (var sr = new StreamReader(stream))
			{
				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();

					int commentStart = line.IndexOf("//", StringComparison.OrdinalIgnoreCase);
					switch (commentStart)
					{
						case -1:
							sb.Append(line);
							break;
						case 0:
							continue;
						default:
							sb.Append(line.Remove(commentStart));
							break;
					}
				}
			}

			Parse(sb.ToString());
		}

		/// <summary>
		/// Update the Saveability of every MapObject in this map, setting
		/// instance entities to be Saveability.Children.
		/// </summary>
		/// <param name="map"></param>
		/// <returns>A copy of the input Map with its MapObjects' Saveability
		/// updated, in preparation for writing to disk as a string.</returns>
		public override Map Collapse()
		{
			var collapsed = new QuakeMap(this);
			collapsed.MapObjects.Clear();

			int worldspawnIndex = MapObjects.FindIndex(o => o.Definition.ClassName == "worldspawn");

			MapObject worldspawn = MapObjects[worldspawnIndex];

			collapsed.MapObjects.Add(worldspawn);

			for (int i = 0; i < MapObjects.Count; i++)
			{
				if (i != worldspawnIndex)
				{
					List<MapObject> collapsedObject = MapObjects[i].Collapse();

					foreach (MapObject co in collapsedObject)
					{
						if (co.Definition.ClassName == "worldspawn")
						{
							worldspawn.Renderables.AddRange(co.Renderables);
						}
						else
						{
							collapsed.MapObjects.Add(co);
						}
					}
				}
			}

			return collapsed;
		}

		public override string ToString()
		{
			return ToString(QuakeSideFormat.Valve220);
		}
		public string ToString(QuakeSideFormat format)
		{
			var sb = new StringBuilder();

			// First build the final worldspawn.
			int worldspawnIndex = MapObjects.FindIndex(o => o.Definition.ClassName == "worldspawn");
			MapObject worldspawn = MapObjects[worldspawnIndex];
			foreach (MapObject mo in AllObjects)
			{
				// Only copy solids to worldspawn if that's all this entity
				// is set to save, otherwise let it get handled later.
				if (mo.Saveability == Saveability.Solids)
				{
					worldspawn.Renderables.AddRange(mo.Renderables);
				}
			}

			sb.AppendLine(new QuakeBlock(worldspawn).ToString(format));

			for (int i = 0; i < MapObjects.Count; i++)
			{
				if (i == worldspawnIndex)
				{
					continue;
				}

				MapObject mo = MapObjects[i];

				// Avoid saving a null character to the output.
				if (mo.Saveability == Saveability.None)
				{
					continue;
				}

				sb.AppendLine(new QuakeBlock(mo).ToString(format));
			}

			return sb.ToString();
		}

		private void Parse(string raw)
		{
			string oldCwd = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(Path.GetDirectoryName(AbsolutePath ?? oldCwd));

			// FIXME: Only strip leading tabs! Or just tabs not within a key or value?
			string stripped = raw
				.Replace("\r", String.Empty)
				.Replace("\n", String.Empty)
				.Replace("\t", String.Empty);

			// Modern Quake sourceports allow for transparency in textures,
			// indicated by an open curly brace in the texture name. Any open
			// curly brace that's actually a delimiter will be followed by an
			// open parenthesis or one or more whitespace characters. Texture
			// names will instead have ordinary text characters after the brace.
			List<string> split = stripped.SplitAndKeepDelimiters(
				$"{OpenDelimiter} ",
				$"{OpenDelimiter}(",
				$"{OpenDelimiter}\"",
				CloseDelimiter).ToList();

			split.RemoveAll(s => s.Trim().Length == 0 || s.Trim() == "\"");

			// The regex patterns above include capture groups to retain some
			// delimiters in the output, but they end up in the same item in the
			// list resulting from Split. This ugly loop fixes that.
			int item = 0;
			while (item < split.Count)
			{
				if (split[item] == "{(")
				{
					split[item] = "{";
					split[item + 1] = $"({split[item + 1]}";
					item++;
				}
				else if (split[item] == "{\"")
				{
					split[item] = "{";
					split[item + 1] = $"\"{split[item + 1]}";
					item++;
				}
				else
				{
					item++;
				}
			}

			int i = 0;
			while (i < split.Count)
			{
				int firstBraceIndex = split.IndexOf("{", i);

				var block = new QuakeBlock(split, firstBraceIndex, Definitions);
				MapObjects.Add(new QuakeMapObject(block, Definitions, Textures));

				i = firstBraceIndex + block.RawLength;
			}

			// First build the final worldspawn.
			int worldspawnIndex = MapObjects.FindIndex(o => o.Definition.ClassName == "worldspawn");
			MapObject worldspawn = MapObjects[worldspawnIndex];
			foreach (MapObject mo in AllObjects)
			{
				Aabb += mo.Aabb;
			}

			Directory.SetCurrentDirectory(oldCwd);
		}

		public override void UpdateFromSettings(Settings settings)
		{
			// TODO: Update entity definitions too. Will probably require
			// hanging on to a copy of the input map's raw string for reparsing,
			// or at least the path to the original file.

			var textures = new Dictionary<string, TextureDictionary>();

			Stream stream = null;
			if (settings.Local.UsingCustomPalette)
			{
				stream = File.OpenRead(settings.Local.LastCustomPalette.LocalPath);
			}
			else
			{
				string name = $"palette-{settings.Roaming.LastBuiltInPalette.ToLower()}.lmp";

				stream = Assembly.GetAssembly(typeof(MainForm)).GetResourceStream(name);
			}

			using (stream)
			{
				Palette palette = new Palette().LoadQuakePalette(stream);

				foreach (string path in settings.Local.TextureDictionaryPaths)
				{
					textures.Add(path, Loader.LoadTextureDictionary(path, palette));
				}
			}

			Textures = textures.Values.ToList().Stack();
		}
	}
}
