using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbatel.Formats
{
	public static class DefinitionCollectionExtensions
	{
		/// <summary>
		/// From a list of Definitions, get the most recently added instance of the requested Definition.
		/// </summary>
		/// <param name="collections"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static Definition GetNewestDefinition(this List<DefinitionDictionary> collections, string key)
		{
			Definition recent = null;

			// Assume the input list is provided the way it was built, one
			// DefinitionCollection after another, most recent last.
			var reversed = new List<DefinitionDictionary>(collections);
			reversed.Reverse();

			foreach (var collection in reversed)
			{
				if (collection.ContainsKey(key))
				{
					recent = collection[key];
					break;
				}
			}

			return recent;
		}

		/// <summary>
		/// Blend together all duplicates of the requested Definition found in
		/// the specified list of collections.
		/// </summary>
		/// <param name="collections"></param>
		/// <param name="key"></param>
		/// <returns>A single Definition with all unique key/value pairs from
		/// its various duplicate instances, and the most recently added copy
		/// of any duplicate key/value pairs.</returns>
		public static Definition BlendDefinition(this List<DefinitionDictionary> collections, string key)
		{
			var blended = new Definition();

			// TODO: If a definition has duplicates, but their class type differs (e.g. it exists
			// as both a solid and a point entity), do something reasonable. Throw an exception? I
			// don't know what should happen there; I think it'd be an unexpected thing, so an
			// exception makes sense, but then most users probably don't want to be editing FGDs
			// manually for things like this. Maybe offer the option to choose which to use?

			return blended;
		}

		/// <summary>
		/// Combine the provided DefinitionCollections, blending duplicate
		/// definitions by combining their key/values.
		/// </summary>
		/// <param name="collections">The DefinitionCollections to combine.</param>
		/// <returns>A new DefinitionCollection built from the input list, with
		/// every unique definition from each collection present and accounted
		/// for, and duplicate definitions blended together. Duplicate key/value
		/// pairs will be represented by the one from the collection that has
		/// the highest index in the input list.</returns>
		public static DefinitionDictionary Blend(this List<DefinitionDictionary> collections)
		{
			var blended = new DefinitionDictionary();

			return blended;
		}

		/// <summary>
		/// Combine the provided DefinitionCollections, replacing earlier copies
		/// of duplicate definitions with later ones.
		/// </summary>
		/// <param name="dictionaries">The dictionaries to combine.</param>
		/// <returns>A new DefinitionDictionary built from the input list, with
		/// every unique definition from each dictionary present and accounted
		/// for, but duplicate definitions represented by whichever copy is
		/// found in the dictionary with the highest index in the input list.</returns>
		public static DefinitionDictionary Stack(this List<DefinitionDictionary> dictionaries)
		{
			var stacked = new DefinitionDictionary();

			foreach (var dictionary in dictionaries)
			{
				foreach (var definition in dictionary.Values)
				{
					var updated = new Definition(definition)
					{
						DefinitionCollection = stacked
					};

					if (stacked.ContainsKey(definition.ClassName))
					{
						stacked[definition.ClassName] = updated;
					}
					else
					{
						stacked.Add(definition.ClassName, updated);
					}
				}
			}

			return stacked;
		}
	}

	public class DefinitionDictionary : Dictionary<string, Definition>
	{
		/// <summary>
		/// A dictionary that maps key names to transform types.
		/// </summary>
		/// <remarks>For cases where a given key needs to be treated differently
		/// from other keys with the same data type, or where a key is expected
		/// to appear in a map but not in a DefinitionDictionary, e.g. 'origin'
		/// or 'angles' in a Quake map, which don't appear in FGDs and therefore
		/// won't have any data type defined.</remarks>
		public Dictionary<string, TransformType> TransformTypeOverrides { get; set; }

		/// <summary>
		/// A dictionary that maps value data types to transform types.
		/// </summary>
		public Dictionary<string, TransformType> TransformTypes { get; set; }

		public DefinitionDictionary() : base()
		{
			TransformTypeOverrides = new Dictionary<string, TransformType>();
			TransformTypes = new Dictionary<string, TransformType>();
		}
		public DefinitionDictionary(DefinitionDictionary definitions) : base(definitions)
		{
			TransformTypeOverrides = new Dictionary<string, TransformType>(definitions.TransformTypeOverrides);
			TransformTypes = new Dictionary<string, TransformType>(definitions.TransformTypes);
		}
		public DefinitionDictionary(string filename) : this(new FileStream(filename, FileMode.Open, FileAccess.Read))
		{
		}
		public DefinitionDictionary(Stream stream) : this()
		{
		}
		public DefinitionDictionary(List<DefinitionDictionary> collections) : this()
		{
		}

		virtual public void Parse(StreamReader sr)
		{
		}

		/// <summary>
		/// Prepare an input definition file for subsequent parsing.
		/// </summary>
		/// <param name="sr"></param>
		/// <returns></returns>
		virtual public List<string> Preprocess(StreamReader sr)
		{
			var list = new List<string>();

			while (!sr.EndOfStream)
			{
				list.Add(sr.ReadLine());
			}

			return list;
		}
	}
}
