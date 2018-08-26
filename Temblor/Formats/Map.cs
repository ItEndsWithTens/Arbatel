using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;
using Temblor.Formats.Quake;
using Temblor.Graphics;

namespace Temblor.Formats
{
	public class Map
	{
		public Aabb Aabb { get; protected set; }

		public string AbsolutePath { get; protected set; }

		/// <summary>
		/// The flat stack of all objects in this map.
		/// </summary>
		public List<MapObject> AllObjects
		{
			get
			{
				var all = new List<MapObject>();

				foreach (var mo in MapObjects)
				{
					all.AddRange(mo.AllObjects);
				}

				return all;
			}
		}

		public string OpenDelimiter { get; set; } = "{";
		public string CloseDelimiter { get; set; } = "}";

		public DefinitionDictionary Definitions { get; set; }

		public List<MapObject> MapObjects { get; set; } = new List<MapObject>();

		public TextureDictionary TextureCollection { get; set; }

		public Map()
		{
		}
		public Map(Map map)
		{
			Aabb = new Aabb(map.Aabb);
			AbsolutePath = map.AbsolutePath;
			OpenDelimiter = map.OpenDelimiter;
			CloseDelimiter = map.CloseDelimiter;
			Definitions = new DefinitionDictionary(map.Definitions);
			MapObjects = new List<MapObject>(map.MapObjects);
			TextureCollection = new TextureDictionary(map.TextureCollection);
		}
		public Map(Stream stream, DefinitionDictionary definitions)
		{
			Aabb = new Aabb();

			if (stream is FileStream f)
			{
				AbsolutePath = f.Name;
			}

			Definitions = definitions;

			TextureCollection = new TextureDictionary();

			using (StreamReader sr = new StreamReader(stream))
			{
				Parse(sr.ReadToEnd());
			}
		}
		public Map(Stream stream, DefinitionDictionary definitions, TextureDictionary textures)
		{
			Aabb = new Aabb();

			if (stream is FileStream f)
			{
				AbsolutePath = f.Name;
			}

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
			//var translation = new Vector3();
			//if (basis.KeyVals.ContainsKey("origin"))
			//{
			//	string[] origin = basis.KeyVals["origin"].Value.Split(' ');

			//	float.TryParse(origin[0], out translation.X);
			//	float.TryParse(origin[1], out translation.Y);
			//	float.TryParse(origin[2], out translation.Z);
			//}

			var rotation = new Vector3();
			if (basis.KeyVals.ContainsKey("angles"))
			{
				string[] angles = basis.KeyVals["angles"].Value.Split(' ');

				// Remember the orientation of instance objects, pointing toward
				// +X in a Z-up, left-handed coordinate space.
				float.TryParse(angles[0], out rotation.Y); // Pitch
				float.TryParse(angles[1], out rotation.Z); // Yaw
				float.TryParse(angles[2], out rotation.X); // Roll
			}

			// TODO: Implement scale.
			var scale = new Vector3(1.0f, 1.0f, 1.0f);

			Transform(basis.Position, rotation, scale);
		}

		/// <summary>
		/// Translate, rotate, and/or scale a map.
		/// </summary>
		/// <param name="map">The map to transform.</param>
		/// <param name="origin">The point these transforms are relative to.</param>
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
