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

			// Contains brushes.
			if (b.Solids.Count > 0)
			{
				foreach (var solid in b.Solids)
				{
					Renderables.Add(new QuakeBrush(solid, TextureCollection));
				}
			}
			// Known point entity with predefined size.
			else if (Definition != null && Definition.ClassType == ClassType.Point && Definition.Size != null)
			{
				string[] coords = KeyVals["origin"][0].Split(' ');

				float.TryParse(coords[0], out float x);
				float.TryParse(coords[1], out float y);
				float.TryParse(coords[2], out float z);

				Position = new Vector3(x, y, z);

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
						foreach (var mo in map.MapObjects)
						{
							Renderables.AddRange(mo.GetAllRenderables());
						}

						foreach (var r in Renderables)
						{
							string[] angles = KeyVals["angles"][0].Split(' ');

							float.TryParse(angles[0], out float pitch);
							float.TryParse(angles[1], out float yaw);
							float.TryParse(angles[2], out float roll);

							r.Rotate(pitch, yaw, roll);

							r.Position += Position;
						}

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
			}

			// Unknown point entity, known point entity with no predefined size,
			// or entity whose renderables failed to load for some reason.
			if (Renderables.Count == 0 && Children.Count == 0)
			{
				Renderable gem = new GemGenerator().Generate();

				string[] coords = b.KeyVals["origin"][0].Split(' ');

				float.TryParse(coords[0], out float x);
				float.TryParse(coords[1], out float y);
				float.TryParse(coords[2], out float z);

				gem.Position = new Vector3(x, y, z);

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
