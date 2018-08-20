using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Controls;
using Temblor.Formats;

namespace Temblor.Graphics
{
	public class GemGenerator : RenderableGenerator
	{
		public float Width;
		public float Depth;
		public float Height;

		public GemGenerator() : this(8.0f, 8.0f, 16.0f, Color4.Yellow)
		{
		}
		public GemGenerator(Color4 _color) : this(8.0f, 8.0f, 16.0f, _color)
		{
		}
		public GemGenerator(float _width, float _depth, float _height, Color4 _color) : base(_color)
		{
			Width = _width;
			Depth = _depth;
			Height = _height;
		}

		public override Renderable Generate()
		{
			float halfWidth = Width / 2.0f;
			float halfDepth = Depth / 2.0f;
			float halfHeight = Height / 2.0f;

			var modelVerts = new List<Vertex>()
			{
				new Vertex(halfWidth, 0.0f, 0.0f, Color),
				new Vertex(0.0f, halfDepth, 0.0f, Color),
				new Vertex(-halfWidth, 0.0f, 0.0f, Color),
				new Vertex(0.0f, -halfDepth, 0.0f, Color),
				new Vertex(0.0f, 0.0f, halfHeight, Color),
				new Vertex(0.0f, 0.0f, -halfHeight, Color)
			};

			var gem = new Renderable(modelVerts)
			{
				ShadingStyleDict = new Dictionary<ShadingStyle, ShadingStyle>().Capped(ShadingStyle.Flat)
			};

			// Top half
			for (var i = 0; i < 4; i++)
			{
				var polygon = new Polygon();

				polygon.Indices.Add(i);
				polygon.Indices.Add((i + 1) % 4);
				polygon.Indices.Add(4);

				Vector3 a = modelVerts[polygon.Indices[1]] - modelVerts[polygon.Indices[0]];
				Vector3 b = modelVerts[polygon.Indices[2]] - modelVerts[polygon.Indices[0]];
				polygon.Normal = Vector3.Cross(a, b);
				polygon.Normal.Normalize();

				gem.Polygons.Add(polygon);
				gem.Indices.AddRange(polygon.Indices);
			}

			// Bottom half
			for (var i = 0; i < 4; i++)
			{
				var polygon = new Polygon();

				polygon.Indices.Add(i);
				polygon.Indices.Add(5);
				polygon.Indices.Add((i + 1) % 4);

				Vector3 a = modelVerts[polygon.Indices[1]] - modelVerts[polygon.Indices[0]];
				Vector3 b = modelVerts[polygon.Indices[2]] - modelVerts[polygon.Indices[0]];
				polygon.Normal = Vector3.Cross(a, b);
				polygon.Normal.Normalize();

				gem.Polygons.Add(polygon);
				gem.Indices.AddRange(polygon.Indices);
			}

			return gem;
		}
	}
}
