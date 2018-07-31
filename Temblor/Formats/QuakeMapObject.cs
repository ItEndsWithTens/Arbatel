﻿using OpenTK;
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
		/// <summary>
		/// The tolerance to use when comparing floats in geometry calculations.
		/// </summary>
		private static readonly float _tolerance = 0.01f;

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

			CalculateIntersections(ref sides, new Color4(1.0f, 1.0f, 1.0f, 1.0f));

			var renderable = new Renderable();

			BuildPolygons(ref sides, ref renderable);

			renderable.Position = new AABB(renderable.Vertices).Center;
			Renderables.Add(renderable);
		}

		/// <summary>
		/// Construct polygons for all provided sides, triangulate them, and add
		/// them to the specified Renderable.
		/// </summary>
		/// <param name="sides">The sides to build polygons for.</param>
		/// <param name="renderable">The renderable that will contain the resulting polygons.</param>
		private static void BuildPolygons(ref List<QuakeSide> sides, ref Renderable renderable)
		{
			foreach (var side in sides)
			{
				if (side.Vertices.Count < 3)
				{
					continue;
				}

				side.Vertices = MathUtilities.SortVertices(side.Vertices, side.Plane.Normal, Winding.Ccw);

				foreach (var sideVertex in side.Vertices)
				{
					var renderableContainsSideVertex = false;
					var index = 0;
					for (var i = 0; i < renderable.Vertices.Count; i++)
					{
						var renderableVertex = renderable.Vertices[i];
						if (MathHelper.ApproximatelyEquivalent(sideVertex.Position.X, renderableVertex.Position.X, _tolerance) &&
							MathHelper.ApproximatelyEquivalent(sideVertex.Position.Y, renderableVertex.Position.Y, _tolerance) &&
							MathHelper.ApproximatelyEquivalent(sideVertex.Position.Z, renderableVertex.Position.Z, _tolerance))
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

				var polygon = new Polygon();

				// By this point, the side's vertices are sorted CCW, and the
				// indices should reflect that. It should now just be a matter
				// of breaking them into groups of three.
				for (var i = 0; i < side.Vertices.Count - 2; i++)
				{
					var indexA = 0;
					var indexB = i + 1;
					var indexC = i + 2;

					polygon.Indices.Add(side.Indices[indexA]);
					polygon.Indices.Add(side.Indices[indexB]);
					polygon.Indices.Add(side.Indices[indexC]);

					renderable.Indices.Add(side.Indices[indexA]);
					renderable.Indices.Add(side.Indices[indexB]);
					renderable.Indices.Add(side.Indices[indexC]);
				}

				polygon.TextureName = side.TextureName;
				polygon.BasisS = side.TextureBasis[0];
				polygon.BasisT = side.TextureBasis[1];
				polygon.Offset = new Vector2(side.TextureOffset.X, side.TextureOffset.Y);
				polygon.Scale = new Vector2(side.TextureScale.X, side.TextureScale.Y);

				renderable.Polygons.Add(polygon);
			}
		}

		/// <summary>
		/// Calculate all valid intersection points of the provided sides, and
		/// add a vertex for each to all sides that share it.
		/// </summary>
		/// <param name="sides">The list of sides to find the intersections of.</param>
		/// <param name="color">The color to use for all created vertices.</param>
		private static void CalculateIntersections(ref List<QuakeSide> sides, Color4 color)
		{
			foreach (var combo in MathUtilities.Combinations(sides.Count, 3))
			{
				Vector3 intersection = Plane.Intersect(sides[combo[0]].Plane, sides[combo[1]].Plane, sides[combo[2]].Plane);
				if (!intersection.X.Equals(float.NaN) &&
					!intersection.Y.Equals(float.NaN) &&
					!intersection.Z.Equals(float.NaN) &&
					VertexIsLegal(intersection, sides))
				{
					for (var i = 0; i < 3; i++)
					{
						var side = sides[combo[i]];

						var vertexIsInSide = false;
						foreach (var sideVertex in side.Vertices)
						{
							if (MathHelper.ApproximatelyEquivalent(intersection.X, sideVertex.Position.X, _tolerance) &&
								MathHelper.ApproximatelyEquivalent(intersection.Y, sideVertex.Position.Y, _tolerance) &&
								MathHelper.ApproximatelyEquivalent(intersection.Z, sideVertex.Position.Z, _tolerance))
							{
								vertexIsInSide = true;
								break;
							}
						}

						if (!vertexIsInSide)
						{
							var vertex = new Vertex(intersection, color);
							side.Vertices.Add(vertex);
						}
					}
				}
			}
		}

		/// <summary>
		/// Determine whether the provided 3D point is valid, given the list of
		/// sides it was calculated from.
		/// </summary>
		/// <param name="vertex">The point to check.</param>
		/// <param name="sides">The sides to compare against.</param>
		/// <returns>True if the point is on or behind all sides.</returns>
		/// <remarks>
		/// A convex 3D object can be represented as a group of planes instead
		/// of explicitly stored vertices and edges. To turn such a group of
		/// planes back into a full mesh requires calculating the intersection
		/// point of every possible combination of 3 planes in order to retrieve
		/// the objects vertices. Not all of those are valid, though. Ziggurats,
		/// for example, have a phantom vertex where three of the walls meet
		/// above the flat top. To determine this, just figure out if the point
		/// is in front of any of the object's planes. That could be the result
		/// of a concave object, but since the assumption in formats like Quake
		/// maps is that the objects are convex, the only other explanation is
		/// that the intersection is a phantom, and shouldn't become a vertex.
		/// </remarks>
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

				inFront = diff > 0.0f && Math.Abs(diff) > _tolerance;

				if (inFront)
				{
					break;
				}
			}

			return !inFront;
		}
	}
}
