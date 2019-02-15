using Arbatel.Controls;
using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;

namespace Arbatel.Graphics
{
	public class BoxGenerator : RenderableGenerator
	{
		public Vector3 Min { get; set; }
		public Vector3 Max { get; set; }

		public BoxGenerator() : this(Color4.Yellow)
		{
		}
		public BoxGenerator(Color4 color) : this(16.0f, color)
		{
		}
		public BoxGenerator(float size, Color4 color) : this(size, size, size, color)
		{
		}
		public BoxGenerator(float width, float depth, float height, Color4 color) : base()
		{
			float Width = width;
			float Depth = depth;
			float Height = height;

			float halfWidth = Width / 2.0f;
			float halfDepth = Depth / 2.0f;
			float halfHeight = Height / 2.0f;

			Min = new Vector3(-halfWidth, -halfDepth, -halfHeight);
			Max = new Vector3(halfWidth, halfDepth, halfHeight);

			Color = color;
		}
		public BoxGenerator(Vector3 min, Vector3 max)
		{
			Min = min;
			Max = max;

			Color = Color4.White;
		}
		public BoxGenerator(Vector3 min, Vector3 max, Color4 color) : this(min, max)
		{
			Color = color;
		}

		public override Renderable Generate()
		{
			var modelVerts = new List<Vertex>()
			{
				// Bottom
				new Vertex(Min.X, Min.Y, Min.Z),
				new Vertex(Min.X, Max.Y, Min.Z),
				new Vertex(Max.X, Max.Y, Min.Z),
				new Vertex(Max.X, Min.Y, Min.Z),

				// Top
				new Vertex(Max.X, Min.Y, Max.Z),
				new Vertex(Max.X, Max.Y, Max.Z),
				new Vertex(Min.X, Max.Y, Max.Z),
				new Vertex(Min.X, Min.Y, Max.Z),

				// Left
				new Vertex(Min.X, Min.Y, Min.Z),
				new Vertex(Min.X, Min.Y, Max.Z),
				new Vertex(Min.X, Max.Y, Max.Z),
				new Vertex(Min.X, Max.Y, Min.Z),

				// Right
				new Vertex(Max.X, Max.Y, Min.Z),
				new Vertex(Max.X, Max.Y, Max.Z),
				new Vertex(Max.X, Min.Y, Max.Z),
				new Vertex(Max.X, Min.Y, Min.Z),

				// Front
				new Vertex(Min.X, Max.Y, Min.Z),
				new Vertex(Min.X, Max.Y, Max.Z),
				new Vertex(Max.X, Max.Y, Max.Z),
				new Vertex(Max.X, Max.Y, Min.Z),

				// Back
				new Vertex(Max.X, Min.Y, Min.Z),
				new Vertex(Max.X, Min.Y, Max.Z),
				new Vertex(Min.X, Min.Y, Max.Z),
				new Vertex(Min.X, Min.Y, Min.Z)
			};

			var box = new Renderable(modelVerts)
			{
				CoordinateSpace = CoordinateSpace.Model,
				ShadingStyleDict = new Dictionary<ShadingStyle, ShadingStyle>().Capped(ShadingStyle.Flat)
			};

			box.Colors[ShadingStyle.Wireframe] = (Color, box.Colors[ShadingStyle.Wireframe].selected);
			box.Colors[ShadingStyle.Flat] = (Color, box.Colors[ShadingStyle.Flat].selected);
			box.Colors[ShadingStyle.Textured] = (Color, box.Colors[ShadingStyle.Textured].selected);

			for (int i = 0; i < 24; i += 4)
			{
				var polygon = new Polygon();

				polygon.Indices.Add(i + 0);
				polygon.Indices.Add(i + 1);
				polygon.Indices.Add(i + 2);
				polygon.Indices.Add(i + 3);

				Vector3 a = modelVerts[polygon.Indices[1]] - modelVerts[polygon.Indices[0]];
				Vector3 b = modelVerts[polygon.Indices[2]] - modelVerts[polygon.Indices[0]];
				polygon.Normal = Vector3.Cross(a, b);
				polygon.Normal.Normalize();

				box.Polygons.Add(polygon);
				box.Indices.AddRange(polygon.Indices);
			}

			box.Transformability = Transformability.Translate;

			return box;
		}
	}
}
