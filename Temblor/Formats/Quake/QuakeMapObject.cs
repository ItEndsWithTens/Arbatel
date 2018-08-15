using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
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
			this(_block as QuakeBlock, _definitions as QuakeFgd)
		{
		}
		public QuakeMapObject(QuakeBlock _block, QuakeFgd _definitions) : base(_block, _definitions)
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
		}

		protected override void ExtractRenderables(Block block)
		{
			var b = block as QuakeBlock;

			if (b.Solids.Count == 0)
			{
				if (Definition != null && Definition.ClassType == ClassType.Point)
				{
					if (Definition.Size != null && Definition.Size.Min != new Vector3(0.0f, 0.0f, 0.0f) && Definition.Size.Max != new Vector3(0.0f, 0.0f, 0.0f))
					{
						var box = new BoxGenerator(Definition.Size.Min, Definition.Size.Max, Definition.Color).Generate();

						string[] coords = b.KeyVals["origin"][0].Split(' ');

						float.TryParse(coords[0], out float x);
						float.TryParse(coords[1], out float y);
						float.TryParse(coords[2], out float z);

						box.Position = new Vector3(x, y, z);
						box.ModelMatrix = Matrix4.CreateTranslation(box.Position.X, box.Position.Z, -box.Position.Y);

						Renderables.Add(box);
					}
				}
			}
			else
			{
				foreach (var solid in b.Solids)
				{
					Renderables.Add(new QuakeBrush(solid));
				}
			}

			if (Renderables.Count == 0 && Children.Count == 0)
			{
				Renderable gem = new GemGenerator().Generate();

				string[] coords = KeyVals["origin"][0].Split(' ');

				float.TryParse(coords[0], out float x);
				float.TryParse(coords[1], out float y);
				float.TryParse(coords[2], out float z);

				gem.Position = new Vector3(x, y, z);
				gem.ModelMatrix = Matrix4.CreateTranslation(gem.Position.X, gem.Position.Z, -gem.Position.Y);

				Renderables.Add(gem);
			}
		}
	}
}
