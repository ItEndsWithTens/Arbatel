using Arbatel.Controls;
using Arbatel.Graphics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;

namespace Arbatel.Formats
{
	public class Map : IUpdateFromSettings
	{
		public Aabb Aabb { get; protected set; } = new Aabb();

		public string AbsolutePath { get; protected set; }

		/// <summary>
		/// The flat stack of all objects in this map.
		/// </summary>
		public List<MapObject> AllObjects
		{
			get
			{
				var all = new List<MapObject>();

				foreach (MapObject mo in MapObjects)
				{
					all.AddRange(mo.AllObjects);
				}

				return all;
			}
		}

		public string OpenDelimiter { get; set; } = "{";
		public string CloseDelimiter { get; set; } = "}";

		public DefinitionDictionary Definitions { get; }

		public List<MapObject> MapObjects { get; } = new List<MapObject>();

		/// <summary>
		/// The texture dictionaries used to produce this map's working texture set.
		/// </summary>
		public List<TextureDictionary> TextureDictionaries { get; } = new List<TextureDictionary>();

		private TextureDictionary _textures = new TextureDictionary();
		/// <summary>
		/// The set of textures currently available to this map.
		/// </summary>
		/// <remarks>Only one TextureDictionary is in use at a time, possibly
		/// the result of combining a series of dictionaries.</remarks>
		public TextureDictionary Textures
		{
			get { return _textures; }
			set
			{
				_textures = value;

				UpdateTextures(value);
			}
		}

		public event EventHandler Updated;

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
			_textures = new TextureDictionary(map.Textures);
		}
		public Map(Stream stream, DefinitionDictionary definitions)
		{
			if (stream is FileStream f)
			{
				AbsolutePath = f.Name;
			}

			Definitions = definitions;
		}
		public Map(Stream stream, DefinitionDictionary definitions, TextureDictionary textures)
		{
			if (stream is FileStream f)
			{
				AbsolutePath = f.Name;
			}

			Definitions = definitions;

			_textures = textures;
		}

		public virtual Map Collapse()
		{
			return this;
		}

		public virtual void UpdateColors(ShadingStyle style)
		{
			IEnumerable<Renderable> renderables = MapObjects.GetAllRenderables();
			foreach (Renderable r in renderables)
			{
				if (r.Selected)
				{
					r.SetColor(r.Colors[style].selected);
				}
				else
				{
					if (r.Tint != null)
					{
						r.SetColor(r.Tint.Value);
					}
					else
					{
						r.SetColor(r.Colors[style].deselected);
					}
				}
			}
		}

		public virtual void UpdateTextures(TextureDictionary textures)
		{
			foreach (MapObject mo in MapObjects)
			{
				mo.UpdateTextures(textures);
			}

			SortTranslucents();
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
				Single.TryParse(angles[0], out rotation.Y); // Pitch
				Single.TryParse(angles[1], out rotation.Z); // Yaw
				Single.TryParse(angles[2], out rotation.X); // Roll
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
			foreach (MapObject mapObject in MapObjects)
			{
				mapObject.Transform(translation, rotation, scale);
			}
		}

		private void SortTranslucents()
		{
			var opaques = new List<MapObject>();
			var translucents = new List<MapObject>();

			foreach (MapObject mo in MapObjects)
			{
				bool translucent = mo.UpdateTranslucency(Textures.Translucents);

				if (translucent)
				{
					translucents.Add(mo);
				}
				else
				{
					opaques.Add(mo);
				}
			}

			MapObjects.Clear();
			MapObjects.AddRange(opaques);
			MapObjects.AddRange(translucents);
		}

		public virtual void UpdateFromSettings(Settings settings)
		{
			OnUpdated();
		}

		protected virtual void OnUpdated()
		{
			OnUpdated(new EventArgs());
		}
		protected virtual void OnUpdated(EventArgs e)
		{
			Updated?.Invoke(this, e);
		}
	}
}
