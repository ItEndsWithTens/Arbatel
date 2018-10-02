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
	public class BoxGenerator : RenderableGenerator
	{
		public Vector3 Min;
		public Vector3 Max;

		public BoxGenerator() : this(16.0f)
		{
		}
		public BoxGenerator(float _size) : this(_size, _size, _size)
		{
		}
		public BoxGenerator(float _width, float _depth, float _height) : base()
		{
			float Width = _width;
			float Depth = _depth;
			float Height = _height;

			float halfWidth = Width / 2.0f;
			float halfDepth = Depth / 2.0f;
			float halfHeight = Height / 2.0f;

			Min = new Vector3(-halfWidth, -halfDepth, -halfHeight);
			Max = new Vector3(halfWidth, halfDepth, halfHeight);
		}
		public BoxGenerator(Vector3 _min, Vector3 _max)
		{
			Min = _min;
			Max = _max;

			Color = Color4.White;
		}
		public BoxGenerator(Vector3 _min, Vector3 _max, Color4 _color) : this(_min, _max)
		{
			Color = _color;
		}

		public override Renderable Generate()
		{
			var modelVerts = new List<Vertex>()
			{
				new Vertex(Min.X, Min.Y, Min.Z, Color),
				new Vertex(Min.X, Max.Y, Min.Z, Color),
				new Vertex(Max.X, Max.Y, Min.Z, Color),
				new Vertex(Max.X, Min.Y, Min.Z, Color),

				new Vertex(Min.X, Min.Y, Max.Z, Color),
				new Vertex(Min.X, Max.Y, Max.Z, Color),
				new Vertex(Max.X, Max.Y, Max.Z, Color),
				new Vertex(Max.X, Min.Y, Max.Z, Color)
			};

			var box = new Renderable(modelVerts)
			{
				CoordinateSpace = CoordinateSpace.Model,
				ShadingStyleDict = new Dictionary<ShadingStyle, ShadingStyle>().Capped(ShadingStyle.Flat)
			};

			// Sides
			for (var i = 0; i < 4; i++)
			{
				var polygon = new Polygon();

				polygon.Indices.Add(i);
				polygon.Indices.Add((i + 3) % 4);
				polygon.Indices.Add(4 + ((i + 3) % 4));

				polygon.Indices.Add(i);
				polygon.Indices.Add(4 + ((i + 3) % 4));
				polygon.Indices.Add(i + 4);

				Vector3 a = modelVerts[polygon.Indices[1]] - modelVerts[polygon.Indices[0]];
				Vector3 b = modelVerts[polygon.Indices[2]] - modelVerts[polygon.Indices[0]];
				polygon.Normal = Vector3.Cross(a, b);
				polygon.Normal.Normalize();

				box.Polygons.Add(polygon);
				box.Indices.AddRange(polygon.Indices);
			}

			// Bottom
			var polyBottom = new Polygon();

			{
				polyBottom.Indices.Add(0);
				polyBottom.Indices.Add(1);
				polyBottom.Indices.Add(2);

				polyBottom.Indices.Add(0);
				polyBottom.Indices.Add(2);
				polyBottom.Indices.Add(3);

				Vector3 a = modelVerts[polyBottom.Indices[1]] - modelVerts[polyBottom.Indices[0]];
				Vector3 b = modelVerts[polyBottom.Indices[2]] - modelVerts[polyBottom.Indices[0]];
				polyBottom.Normal = Vector3.Cross(a, b);
				polyBottom.Normal.Normalize();
			}

			box.Polygons.Add(polyBottom);
			box.Indices.AddRange(polyBottom.Indices);

			// Top
			var polyTop = new Polygon();

			{
				polyTop.Indices.Add(4);
				polyTop.Indices.Add(6);
				polyTop.Indices.Add(5);

				polyTop.Indices.Add(4);
				polyTop.Indices.Add(7);
				polyTop.Indices.Add(6);

				Vector3 a = modelVerts[polyTop.Indices[1]] - modelVerts[polyTop.Indices[0]];
				Vector3 b = modelVerts[polyTop.Indices[2]] - modelVerts[polyTop.Indices[0]];
				polyTop.Normal = Vector3.Cross(a, b);
				polyTop.Normal.Normalize();

				box.Polygons.Add(polyTop);
				box.Indices.AddRange(polyTop.Indices);
			}

			box.Transformability = Transformability.Translate;

			return box;
		}
	}
}
