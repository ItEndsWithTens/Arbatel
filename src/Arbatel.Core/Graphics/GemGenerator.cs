using Arbatel.Controls;
using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;

namespace Arbatel.Graphics
{
	public class GemGenerator : RenderableGenerator
	{
		public float Width { get; set; }
		public float Depth { get; set; }
		public float Height { get; set; }

		public GemGenerator() : this(8.0f, 8.0f, 16.0f, Color4.Yellow)
		{
		}
		public GemGenerator(Color4 color) : this(8.0f, 8.0f, 16.0f, color)
		{
		}
		public GemGenerator(float width, float depth, float height, Color4 color) : base(color)
		{
			Width = width;
			Depth = depth;
			Height = height;
		}

		public override Renderable Generate()
		{
			float halfWidth = Width / 2.0f;
			float halfDepth = Depth / 2.0f;
			float halfHeight = Height / 2.0f;

			var modelVerts = new List<Vertex>()
			{
				// Top SE
				new Vertex(halfWidth, 0.0f, 0.0f, Color),
				new Vertex(0.0f, 0.0f, halfHeight, Color),
				new Vertex(0.0f, -halfDepth, 0.0f, Color),

				// Top NE
				new Vertex(0.0f, halfDepth, 0.0f, Color),
				new Vertex(0.0f, 0.0f, halfHeight, Color),
				new Vertex(halfWidth, 0.0f, 0.0f, Color),

				// Top NW
				new Vertex(-halfWidth, 0.0f, 0.0f, Color),
				new Vertex(0.0f, 0.0f, halfHeight, Color),
				new Vertex(0.0f, halfDepth, 0.0f, Color),

				// Top SW
				new Vertex(0.0f, -halfDepth, 0.0f, Color),
				new Vertex(0.0f, 0.0f, halfHeight, Color),
				new Vertex(-halfWidth, 0.0f, 0.0f, Color),

				// Bottom NW
				new Vertex(0.0f, halfDepth, 0.0f, Color),
				new Vertex(0.0f, 0.0f, -halfHeight, Color),
				new Vertex(-halfWidth, 0.0f, 0.0f, Color),

				// Bottom NE
				new Vertex(halfWidth, 0.0f, 0.0f, Color),
				new Vertex(0.0f, 0.0f, -halfHeight, Color),
				new Vertex(0.0f, halfDepth, 0.0f, Color),

				// Bottom SE
				new Vertex(0.0f, -halfDepth, 0.0f, Color),
				new Vertex(0.0f, 0.0f, -halfHeight, Color),
				new Vertex(halfWidth, 0.0f, 0.0f, Color),

				// Bottom SW
				new Vertex(-halfWidth, 0.0f, 0.0f, Color),
				new Vertex(0.0f, 0.0f, -halfHeight, Color),
				new Vertex(0.0f, -halfDepth, 0.0f, Color)
			};

			var gem = new Renderable(modelVerts)
			{
				CoordinateSpace = CoordinateSpace.Model,
				ShadingStyleDict = new Dictionary<ShadingStyle, ShadingStyle>().Capped(ShadingStyle.Flat)
			};

			for (int i = 0; i < 24; i += 3)
			{
				var polygon = new Polygon();

				polygon.Indices.Add(i + 0);
				polygon.Indices.Add(i + 1);
				polygon.Indices.Add(i + 2);

				Vector3 a = modelVerts[polygon.Indices[1]] - modelVerts[polygon.Indices[0]];
				Vector3 b = modelVerts[polygon.Indices[2]] - modelVerts[polygon.Indices[0]];
				polygon.Normal = Vector3.Cross(a, b);
				polygon.Normal.Normalize();

				gem.Polygons.Add(polygon);
				gem.Indices.AddRange(polygon.Indices);
			}

			gem.Transformability = Transformability;

			return gem;
		}
	}
}
