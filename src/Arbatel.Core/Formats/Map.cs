﻿using Arbatel.Controls;
using Arbatel.Graphics;
using Arbatel.UI;
using Arbatel.Utilities;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Arbatel.Formats
{
	public enum FixUpStyle
	{
		Prefix,
		Postfix,
		None
	}

	public class Map : IUpdateFromSettings, IProgress
	{
		public string Raw { get; set; }

		public Aabb Aabb { get; protected set; } = new Aabb();

		public string AbsolutePath { get; protected set; }

		/// <summary>
		/// A flat list of all objects in this map.
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

		public List<MapObject> MapObjects { get; protected set; } = new List<MapObject>();

		public bool InitializedInBackEnd { get; set; } = false;

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
			Raw = map.Raw;
			Aabb = new Aabb(map.Aabb);
			AbsolutePath = map.AbsolutePath;
			OpenDelimiter = map.OpenDelimiter;
			CloseDelimiter = map.CloseDelimiter;
			Definitions = new DefinitionDictionary(map.Definitions);
			MapObjects = new List<MapObject>(map.MapObjects);
			InitializedInBackEnd = false;
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

		public virtual Map Parse()
		{
			return this;
		}

		public virtual void Prune(ClassType type)
		{
			var pruned = new List<MapObject>();

			foreach (MapObject mo in MapObjects)
			{
				if (mo.Definition.ClassType == type)
				{
					continue;
				}
				else
				{
					mo.Prune(type);
					pruned.Add(mo);
				}
			}

			MapObjects = pruned;
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

		public virtual void FixUp()
		{
			string defaultFixUpText = "AutoInstance";
			int defaultFixUpNumber = -1;

			foreach (MapObject mo in MapObjects)
			{
				FixUpStyle fixUpStyle = FixUpStyle.None;
				string fixUpName = null;
				var replacements = new Dictionary<string, string>();

				if (mo.Definition.ClassName == "func_instance")
				{
					if (Int32.TryParse(mo.KeyVals["fixup_style"].Value, out int rawStyle))
					{
						fixUpStyle = (FixUpStyle)rawStyle;
					}

					if (mo.KeyVals.ContainsKey("targetname"))
					{
						fixUpName = mo.KeyVals["targetname"].Value;
					}

					// There are only 10 predefined "replaceXY" keys in the sample
					// FGD included with an Arbatel distribution, but as explained
					// there, users should be able to add an arbitrary number of
					// them. Checking only for "replace" lets people go crazy.
					IEnumerable<KeyValuePair<string, Option>> pairs =
						from kv in mo.KeyVals
						where kv.Key.StartsWith("replace", StringComparison.InvariantCultureIgnoreCase)
						select kv;

					foreach (KeyValuePair<string, Option> pair in pairs)
					{
						string[] split = pair.Value.Value.Split(' ');

						replacements.Add(split[0].ToLower(), split[1].ToLower());
					}
				}

				if (fixUpStyle == FixUpStyle.None)
				{
					continue;
				}

				mo.FixUp(fixUpStyle, fixUpName, replacements, defaultFixUpText, ref defaultFixUpNumber);
			}
		}

		/// <summary>
		/// Translate, rotate, and/or scale a map, relative to a MapObject.
		/// </summary>
		/// <param name="map">The map to transform.</param>
		/// <param name="basis">The MapObject serving as the basis of the transform.</param>
		public void Transform(MapObject basis)
		{
			// Remember the orientation of instance objects: pointing toward
			// +X in a Z-up, left-handed coordinate space. Input objects exist
			// in that coordinate system, but have their 'angles' and similar
			// keys stored as Pitch Yaw Roll (rotation about YZX, respectively)
			// and so need to be converted with particular care. Very confusing.
			//
			// Also beware the difference in rotation between the two types of
			// instance entity: when looking from the negative end of an axis
			// toward the positive end, func_instance treats positive angles as
			// rotating clockwise, as in the Source engine and its tools, while
			// misc_external_map treats positive angles as counter-clockwise.

			var rotation = new Vector3();
			if (basis.KeyVals.ContainsKey("_external_map_angles"))
			{
				rotation = -basis.KeyVals["_external_map_angles"].Value.ToVector3().Zxy;
			}
			else if (basis.KeyVals.ContainsKey("_external_map_angle"))
			{
				rotation.Z = -Single.Parse(basis.KeyVals["_external_map_angle"].Value);
			}
			else if (basis.KeyVals.ContainsKey("angles"))
			{
				rotation = basis.KeyVals["angles"].Value.ToVector3().Zxy;
			}

			var scale = new Vector3(1.0f);
			if (basis.KeyVals.ContainsKey("_external_map_scale"))
			{
				string s = basis.KeyVals["_external_map_scale"].Value;

				string[] split = s.Split(' ');

				if (split.Length == 3)
				{
					scale = s.ToVector3();
				}
				else if (split.Length == 1)
				{
					scale = new Vector3(Single.Parse(s));
				}
				else
				{
					throw new InvalidDataException($"Invalid _external_map_scale value: {s}");
				}
			}

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

		public event EventHandler<ProgressEventArgs> ProgressUpdated;

		public virtual void OnProgressUpdated(object sender, ProgressEventArgs e)
		{
			ProgressUpdated?.Invoke(this, e);
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
