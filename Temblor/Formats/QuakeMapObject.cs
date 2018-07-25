using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Graphics;
using Temblor.Utilities;

namespace Temblor.Formats
{
	public class QuakeMapObject : MapObject
	{
		public QuakeMapObject(Block _block) : this(_block as QuakeBlock)
		{
		}
		public QuakeMapObject(QuakeBlock _block) : base(_block)
		{
			foreach (var child in Block.Children)
			{
				Children.Add(new QuakeMapObject(child));
			}

			ExtractRenderables(Block);
		}

		protected override void ExtractRenderables(Block block)
		{
			var b = block as QuakeBlock;

			if (b.Sides.Count == 0)
			{
				return;
			}

			var sides = new List<QuakeSide>();
			foreach (var side in b.Sides)
			{
				sides.Add(new QuakeSide(side));
			}

			foreach (var combo in MathUtilities.Combinations(sides.Count, 3))
			{
				Vector3 intersection = Plane.Intersect(sides[combo[0]].Plane, sides[combo[1]].Plane, sides[combo[2]].Plane);
				if (!intersection.X.Equals(float.NaN) &&
					!intersection.Y.Equals(float.NaN) &&
					!intersection.Z.Equals(float.NaN))
				{
					if (VertexIsLegal(intersection, sides))
					{
						var r = new Random();
						var color = new Color4((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), 1.0f);

						var vertex = new Vertex(intersection, color);

						sides[combo[0]].Vertices.Add(vertex);
						sides[combo[1]].Vertices.Add(vertex);
						sides[combo[2]].Vertices.Add(vertex);
					}
				}
			}

			var renderable = new Renderable();

			foreach (var side in sides)
			{
				side.Vertices = MathUtilities.SortVertices(side.Vertices, side.Plane.Normal, Winding.CCW);

				foreach (var sideVertex in side.Vertices)
				{
					var renderableContainsSideVertex = false;
					var index = 0;
					for (var i = 0; i < renderable.Vertices.Count; i++)
					{
						var renderableVertex = renderable.Vertices[i];
						if (MathHelper.ApproximatelyEquivalent(sideVertex.Position.X, renderableVertex.Position.X, 0.001f) &&
							MathHelper.ApproximatelyEquivalent(sideVertex.Position.Y, renderableVertex.Position.Y, 0.001f) &&
							MathHelper.ApproximatelyEquivalent(sideVertex.Position.Z, renderableVertex.Position.Z, 0.001f))
						{
							renderableContainsSideVertex = true;
							index = i;
							break;
						}
					}

					if (renderableContainsSideVertex)
					{
						side.Indices.Add(index);
					}
					else
					{
						renderable.Vertices.Add(sideVertex);
						side.Indices.Add(renderable.Vertices.Count - 1);
					}
				}

				// By this point, the side's vertices are sorted CCW, and the
				// indices should reflect that. It should now just be a matter
				// of breaking them into groups of three.
				for (var i = 0; i < side.Vertices.Count - 2; i++)
				{
					var indexA = 0;
					var indexB = i + 1;
					var indexC = i + 2;

					renderable.Indices.Add(side.Indices[indexA]);
					renderable.Indices.Add(side.Indices[indexB]);
					renderable.Indices.Add(side.Indices[indexC]);
				}
			}

			Renderables.Add(renderable);
		}

		private static bool VertexIsLegal(Vertex vertex, List<QuakeSide> sides)
		{
			return VertexIsLegal(vertex.Position, sides);
		}
		private static bool VertexIsLegal(Vector3 vertex, List<QuakeSide> sides)
		{
			var inFront = false;

			foreach (var side in sides)
			{
				float dot = Vector3.Dot(side.Plane.Normal, vertex);

				float diff = dot - side.Plane.DistanceFromOrigin;

				inFront = diff > 0.0f && Math.Abs(diff) > 0.001f;

				if (inFront)
				{
					break;
				}
			}

			return !inFront;
		}
	}
}
