using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;
using Temblor.Formats.Quake;

namespace Temblor.Formats
{
	public class Map
	{
		public string OpenDelimiter = "{";
		public string CloseDelimiter = "}";

		public DefinitionCollection Definitions;

		public List<MapObject> MapObjects;

		public TextureCollection TextureCollection;

		public Map()
		{
			MapObjects = new List<MapObject>();
		}
		public Map(string filename, DefinitionCollection definitions) :
			this(new FileStream(filename, FileMode.Open, FileAccess.Read), definitions)
		{
		}
		public Map(Stream stream, DefinitionCollection definitions) : this()
		{
			Definitions = definitions;

			using (StreamReader sr = new StreamReader(stream))
			{
				Parse(sr.ReadToEnd());
			}
		}
		public Map(string filename, DefinitionCollection definitions, TextureCollection textures) :
			this(new FileStream(filename, FileMode.Open, FileAccess.Read), definitions, textures)
		{
		}
		public Map(Stream stream, DefinitionCollection definitions, TextureCollection textures) : this()
		{
			Definitions = definitions;

			TextureCollection = textures;

			using (StreamReader sr = new StreamReader(stream))
			{
				Parse(sr.ReadToEnd());
			}
		}

		virtual public void Parse(string raw)
		{
		}

		/// <summary>
		/// Translate, rotate, and/or scale a map, relative to a MapObject.
		/// </summary>
		/// <param name="map">The map to transform.</param>
		/// <param name="basis">The MapObject serving as the basis of the transform.</param>
		public void Transform(MapObject basis)
		{
			var translation = new Vector3();
			if (basis.KeyVals.ContainsKey("origin"))
			{
				string[] origin = basis.KeyVals["origin"][0].Split(' ');

				float.TryParse(origin[0], out translation.X);
				float.TryParse(origin[1], out translation.Y);
				float.TryParse(origin[2], out translation.Z);
			}

			var rotation = new Vector3();
			if (basis.KeyVals.ContainsKey("angles"))
			{
				string[] angles = basis.KeyVals["angles"][0].Split(' ');

				// Remember the orientation of instance objects, pointing toward
				// +X in a Z-up, left-handed coordinate space.
				float.TryParse(angles[0], out rotation.Y); // Pitch
				float.TryParse(angles[1], out rotation.Z); // Yaw
				float.TryParse(angles[2], out rotation.X); // Roll
			}

			// TODO: Implement scale.
			var scale = new Vector3(1.0f, 1.0f, 1.0f);

			Transform(translation, rotation, scale);
		}

		/// <summary>
		/// Translate, rotate, and/or scale a map.
		/// </summary>
		/// <param name="map">The map to transform.</param>
		/// <param name="translation">The relative translation to apply.</param>
		/// <param name="rotation">The roll/pitch/yaw to apply.</param>
		/// <param name="scale">The scale to apply.</param>
		public void Transform(Vector3 translation, Vector3 rotation, Vector3 scale)
		{
			foreach (var mapObject in MapObjects)
			{
				mapObject.Transform(translation, rotation, scale);
			}
		}
	}
}
