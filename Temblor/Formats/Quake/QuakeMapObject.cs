using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Controls;
using Temblor.Formats.Quake;
using Temblor.Graphics;
using Temblor.Utilities;

namespace Temblor.Formats
{
	/// <summary>
	/// Any key/value-bearing entity in a Quake map.
	/// </summary>
	public class QuakeMapObject : MapObject
	{
		public QuakeMapObject() : base()
		{
		}
		public QuakeMapObject(QuakeMapObject qmo) : base(qmo)
		{
		}
		public QuakeMapObject(Block _block, DefinitionCollection _definitions) :
			this(_block as QuakeBlock, _definitions)
		{
		}
		public QuakeMapObject(Block _block, DefinitionCollection _definitions, TextureCollection _textures) :
			this(_block as QuakeBlock, _definitions, _textures)
		{
		}
		public QuakeMapObject(QuakeBlock _block, DefinitionCollection _definitions) :
			base(_block, _definitions)
		{
			KeyVals = new Dictionary<string, List<string>>(_block.KeyVals);

			Definition = _definitions[KeyVals["classname"][0]];

			foreach (var child in _block.Children)
			{
				if (child.KeyVals.Count > 0)
				{
					Children.Add(new QuakeMapObject(child, _definitions));
				}
				else
				{
					ExtractRenderables(child);
				}
			}

			ExtractRenderables(_block);

			UpdateBounds();
		}
		public QuakeMapObject(QuakeBlock _block, DefinitionCollection _definitions, TextureCollection _textures) :
			base(_block, _definitions, _textures)
		{
			KeyVals = new Dictionary<string, List<string>>(_block.KeyVals);

			Definition = _definitions[KeyVals["classname"][0]];

			TextureCollection = _textures;

			foreach (var child in _block.Children)
			{
				if (child.KeyVals.Count > 0)
				{
					Children.Add(new QuakeMapObject(child, _definitions, _textures));
				}
				else
				{
					ExtractRenderables(child);
				}
			}

			ExtractRenderables(_block);

			UpdateBounds();
		}

		protected override void ExtractRenderables(Block block)
		{
			var b = block as QuakeBlock;

			// Contains brushes. Checking the Solids count allows for both known
			// and unknown solid entities, which can be treated the same way.
			if (b.Solids.Count > 0)
			{
				foreach (var solid in b.Solids)
				{
					Renderables.Add(new QuakeBrush(solid, TextureCollection));
				}
			}
			// Known point entity.
			else if (Definition?.ClassType == ClassType.Point)
			{
				float x = Position.X;
				float y = Position.Y;
				float z = Position.Z;

				if (KeyVals.ContainsKey("origin"))
				{
					string[] coords = KeyVals["origin"][0].Split(' ');

					float.TryParse(coords[0], out x);
					float.TryParse(coords[1], out y);
					float.TryParse(coords[2], out z);

					Position = new Vector3(x, y, z);
				}

				if (Definition.RenderableSources.ContainsKey(RenderableSource.Key))
				{
					string key = Definition.RenderableSources[RenderableSource.Key];

					string path = KeyVals[key][0];

					if (path.EndsWith(".map"))
					{
						var oldCwd = Directory.GetCurrentDirectory();
						var instancePath = oldCwd + Path.DirectorySeparatorChar + path;

						Directory.SetCurrentDirectory(Path.GetDirectoryName(instancePath));

						var map = new QuakeMap(instancePath, Definition.DefinitionCollection, TextureCollection);
						map.Transform(this);
						UserData = map;

						foreach (var mo in map.MapObjects)
						{
							// Since instances are point entities, none of their
							// Renderables will be written out when saving the
							// map to disk, so this is safe. Actually collapsing
							// the instance is accomplished by way of UserData.
							Renderables.AddRange(mo.GetAllRenderables());
						}

						// Create a simple box to mark this instance's origin.
						var generator = new BoxGenerator()
						{
							Color = Color4.Orange
						};

						var box = generator.Generate();
						box.Position = new Vector3(x, y, z);

						Renderables.Add(box);

						UpdateBounds();

						Directory.SetCurrentDirectory(oldCwd);
					}
				}
				else if (Definition.RenderableSources.ContainsKey(RenderableSource.Model))
				{
					LoadModel(block as QuakeBlock);
				}
				else if (Definition.RenderableSources.ContainsKey(RenderableSource.Size))
				{
					Aabb s = Definition.Size;

					var box = new BoxGenerator(s.Min, s.Max, Definition.Color).Generate();

					box.CoordinateSpace = CoordinateSpace.World;
					box.Position = Position;

					Renderables.Add(box);
				}
				// Known point entity with no predefined size.
				else
				{
					Renderable gem = new GemGenerator(Color4.Lime).Generate();

					gem.Position = new Vector3(x, y, z);

					Renderables.Add(gem);
				}
			}
			// Unknown entity.
			else if (Definition == null)
			{
				Renderable gem = new GemGenerator().Generate();

				gem.Position = Position;

				Renderables.Add(gem);
			}
		}

		public void LoadModel(QuakeBlock block)
		{
			Renderable gem = new GemGenerator(Color4.Red).Generate();

			string[] coords = block.KeyVals["origin"][0].Split(' ');

			float.TryParse(coords[0], out float x);
			float.TryParse(coords[1], out float y);
			float.TryParse(coords[2], out float z);

			gem.Position = new Vector3(x, y, z);

			Renderables.Add(gem);
		}
	}
}
