using OpenTK;
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
		public BoxGenerator(float _width, float _depth, float _height)
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
		}

		public override Renderable Generate()
		{
			var modelVerts = new List<Vector3>()
			{
				new Vector3(Min.X, Min.Y, Min.Z),
				new Vector3(Min.X, Max.Y, Min.Z),
				new Vector3(Max.X, Max.Y, Min.Z),
				new Vector3(Max.X, Min.Y, Min.Z),

				new Vector3(Min.X, Min.Y, Max.Z),
				new Vector3(Min.X, Max.Y, Max.Z),
				new Vector3(Max.X, Max.Y, Max.Z),
				new Vector3(Max.X, Min.Y, Max.Z)
			};

			var cube = new Renderable(modelVerts)
			{
				ShadingStyleDict = new Dictionary<ShadingStyle, ShadingStyle>()
				{
					{ ShadingStyle.Wireframe, ShadingStyle.Wireframe },
					{ ShadingStyle.Flat, ShadingStyle.Flat },
					{ ShadingStyle.Textured, ShadingStyle.Flat }
				}
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

				cube.Polygons.Add(polygon);
				cube.Indices.AddRange(polygon.Indices);
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

			cube.Polygons.Add(polyBottom);
			cube.Indices.AddRange(polyBottom.Indices);

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

				cube.Polygons.Add(polyTop);
				cube.Indices.AddRange(polyTop.Indices);
			}

			return cube;
		}
	}
}
