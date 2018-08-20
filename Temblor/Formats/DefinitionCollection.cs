using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Formats
{
	public static class DefinitionCollectionExtensions
	{
		/// <summary>
		/// From a list of Definitions, get the most recently added instance of the requested Definition.
		/// </summary>
		/// <param name="collections"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static Definition GetNewestDefinition(this List<DefinitionCollection> collections, string key)
		{
			Definition recent = null;

			// Assume the input list is provided the way it was built, one
			// DefinitionCollection after another, most recent last.
			var reversed = new List<DefinitionCollection>(collections);
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
		public static Definition BlendDefinition(this List<DefinitionCollection> collections, string key)
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
		public static DefinitionCollection Blend(this List<DefinitionCollection> collections)
		{
			var blended = new DefinitionCollection();

			return blended;
		}

		/// <summary>
		/// Combine the provided DefinitionCollections, replacing earlier copies
		/// of duplicate definitions with later ones.
		/// </summary>
		/// <param name="collections">The DefinitionCollections to combine.</param>
		/// <returns>A new DefinitionCollection built from the input list, with
		/// every unique definition from each collection present and accounted
		/// for, but duplicate definitions represented by whichever copy is
		/// found in the collection with the highest index in the input list.</returns>
		public static DefinitionCollection Stack(this List<DefinitionCollection> collections)
		{
			var stacked = new DefinitionCollection();

			foreach (var collection in collections)
			{
				foreach (var definition in collection.Values)
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

	public class DefinitionCollection : Dictionary<string, Definition>
	{
		public DefinitionCollection() : base()
		{
		}
		public DefinitionCollection(string filename) : this(new FileStream(filename, FileMode.Open, FileAccess.Read))
		{
		}
		public DefinitionCollection(Stream stream) : this()
		{
			using (var sr = new StreamReader(stream))
			{
				Parse(sr);
			}
		}
		public DefinitionCollection(List<DefinitionCollection> collections) : this()
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
