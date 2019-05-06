using Arbatel.Formats.Quake;
using Arbatel.Graphics;
using Arbatel.Utilities;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Arbatel.Formats
{
	public static class QuakeMapObjectExtensions
	{
		public static List<MapObject> Collapse(this MapObject mo)
		{
			var collapsed = new QuakeMapObject(mo);
			collapsed.Children.Clear();

			foreach (MapObject child in mo.Children)
			{
				collapsed.Children.AddRange(new QuakeMapObject(child).Collapse());
			}

			if (mo.Definition.ClassName == "worldspawn")
			{
				collapsed.Saveability = Saveability.Solids;
			}
			else if (mo.Definition.ClassName == "func_instance")
			{
				collapsed.Saveability = Saveability.Children;
			}
			else if (mo.Definition.ClassName == "misc_external_map")
			{
				if (mo.KeyVals.ContainsKey("_external_map_classname"))
				{
					// To mimic the behavior of ericw's QBSP, any point entities
					// within a misc_external_map should be ignored, and all
					// brushes should be added to a new instance of the entity
					// class specified in _external_map_classname.
					//
					// The former is accomplished down in ExtractRenderables,
					// when first loading the misc_external_map's referenced map
					// file. The latter is done here, by changing the current
					// map object's entity definition, clearing out the baggage
					// it's carrying, then bringing along only the renderables
					// of its children so as to skip its placeholder cube.

					string name = mo.KeyVals["_external_map_classname"].Value;
					Definition entity = mo.Definition.DefinitionCollection[name];

					collapsed.Definition = entity;
					collapsed.Saveability = Saveability.All;

					collapsed.KeyVals.Clear();
					collapsed.Children.Clear();
					collapsed.Renderables.Clear();

					Option classname = entity.KeyValsTemplate["classname"];
					classname.Value = name;
					collapsed.KeyVals.Add("classname", classname);

					collapsed.Renderables.AddRange(mo.Children.GetAllRenderables());
				}
				else
				{
					collapsed.Saveability = Saveability.Children;
				}
			}

			bool onlyChildren = collapsed.Saveability == Saveability.Children;
			return onlyChildren ? collapsed.Children : new List<MapObject> { collapsed };
		}
	}

	/// <summary>
	/// Any key/value-bearing entity in a Quake map.
	/// </summary>
	public class QuakeMapObject : MapObject
	{
		public QuakeMapObject() : base()
		{
		}
		public QuakeMapObject(MapObject mo) : base(mo)
		{
		}
		public QuakeMapObject(QuakeMapObject qmo) : base(qmo)
		{
		}
		public QuakeMapObject(Block block, DefinitionDictionary definitions) :
			this(block, definitions, new TextureDictionary())
		{
		}
		public QuakeMapObject(Block block, DefinitionDictionary definitions, TextureDictionary textures) :
			base(block, definitions, textures)
		{
			QuakeBlock quakeBlock;
			if (block is QuakeBlock)
			{
				quakeBlock = block as QuakeBlock;
			}
			else
			{
				throw new ArgumentException("Provided Block isn't actually a QuakeBlock!");
			}

			KeyVals = new Dictionary<string, Option>(quakeBlock.KeyVals);

			if (definitions.ContainsKey(KeyVals["classname"].Value))
			{
				Definition = definitions[KeyVals["classname"].Value];
			}
			else
			{
				Definition = new Definition();
			}

			Saveability = Definition.Saveability;

			TextureCollection = textures;

			foreach (Block child in quakeBlock.Children)
			{
				if (child.KeyVals.Count > 0)
				{
					Children.Add(new QuakeMapObject(child, definitions, textures));
				}
				else
				{
					ExtractRenderables(child);
				}
			}

			ExtractRenderables(quakeBlock);

			UpdateBounds();

			Position = Aabb.Center;

			if (KeyVals.ContainsKey("origin"))
			{
				Position = KeyVals["origin"].Value.ToVector3();
			}
			else if (Definition.ClassName == "worldspawn")
			{
				Position = new Vector3(0, 0, 0);
			}
		}

		protected override void ExtractRenderables(Block block)
		{
			var b = block as QuakeBlock;

			// Contains brushes. Checking the Solids count allows for both known
			// and unknown solid entities, which can be treated the same way.
			if (b.Solids.Count > 0)
			{
				foreach (Solid solid in b.Solids)
				{
					Renderables.Add(new QuakeBrush(solid));
				}
			}
			// Known point entity.
			else if (Definition?.ClassType == ClassType.Point)
			{
				if (KeyVals.ContainsKey("origin"))
				{
					Position = KeyVals["origin"].Value.ToVector3();
				}

				if (Definition.RenderableSources.ContainsKey(RenderableSource.Key))
				{
					string key = Definition.RenderableSources[RenderableSource.Key];

					string path = KeyVals[key].Value;

					if (path.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
					{
						string oldCwd = Directory.GetCurrentDirectory();

						string instancePath;
						if (Path.IsPathRooted(path))
						{
							instancePath = path;
						}
						else
						{
							instancePath = Path.Combine(oldCwd, path);
						}

						QuakeMap map;
						using (FileStream stream = File.OpenRead(instancePath))
						{
							map = new QuakeMap(stream, Definition.DefinitionCollection);
						}
						map.Parse();

						if (Definition.ClassName == "misc_external_map")
						{
							map.Prune(ClassType.Point);
						}

						// For now just tweak the instance map's renderable
						// geometry; name fixup and variable replacement will
						// be easier to accomplish as a separate pass, once all
						// instance maps have been loaded and parsed.
						map.Transform(this);
						UserData = map;

						foreach (MapObject mo in map.AllObjects)
						{
							var modified = new QuakeMapObject(mo);
							if (mo.KeyVals["classname"].Value == "worldspawn")
							{
								modified.Saveability = Saveability.Solids;
							}

							Children.Add(modified);
						}

						// Create a simple box to mark this instance's origin.
						Renderable box = new BoxGenerator(Color4.Orange).Generate();
						box.Position = Position;
						box.Transformability = Definition.RenderableTransformability;

						Renderables.Add(box);
					}
				}
				else if (Definition.RenderableSources.ContainsKey(RenderableSource.Model))
				{
					LoadModel(block as QuakeBlock);
				}
				else if (Definition.RenderableSources.ContainsKey(RenderableSource.Size))
				{
					Aabb s = Definition.Size;

					Renderable box = new BoxGenerator(s.Min, s.Max, Definition.Color).Generate();

					box.Position = Position;

					Renderables.Add(box);
				}
				// Known point entity with no predefined size.
				else
				{
					Renderable gem = new GemGenerator(Color4.Lime).Generate();

					gem.Position = Position;
					gem.Transformability = Definition.RenderableTransformability;

					Renderables.Add(gem);
				}
			}
			// Unknown entity.
			else if (Definition == null)
			{
				Renderable gem = new GemGenerator().Generate();

				gem.Position = Position;
				gem.Transformability = Definition.RenderableTransformability;

				Renderables.Add(gem);
			}
		}

		public void LoadModel(QuakeBlock block)
		{
			Renderable gem = new GemGenerator(Color4.Red).Generate();

			gem.Position = block.KeyVals["origin"].Value.ToVector3();
			gem.Transformability = Definition.RenderableTransformability;

			Renderables.Add(gem);
		}
	}
}
