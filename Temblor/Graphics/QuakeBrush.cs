using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Formats;
using Temblor.Formats.Quake;
using Temblor.Utilities;

namespace Temblor.Graphics
{
	public class QuakeBrush : Renderable
	{
		/// <summary>
		/// The tolerance to use when comparing floats in geometry calculations.
		/// </summary>
		private static readonly float _tolerance = 0.01f;

		public QuakeBrush()
		{
		}
		public QuakeBrush(Solid solid)
		{
			var random = new Random();
			var color = new Color4((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1.0f);

			CalculateIntersections(solid, color);

			BuildPolygons(solid, this);

			AABB = new Aabb(Vertices);

			_position = AABB.Center;
		}
		public QuakeBrush(Solid solid, TextureCollection textures)
		{
			TextureCollection = textures;

			var random = new Random();
			var color = new Color4((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1.0f);

			CalculateIntersections(solid, color);

			BuildPolygons(solid, this);

			AABB = new Aabb(Vertices);

			_position = AABB.Center;
		}

		/// <summary>
		/// Construct polygons for the provided solid, triangulate them, and add
		/// them to the specified Renderable.
		/// </summary>
		/// <param name="solid">The solid to build polygons for.</param>
		/// <param name="renderable">The renderable that will contain the resulting polygons.</param>
		private static void BuildPolygons(Solid solid, Renderable renderable)
		{
			foreach (var side in solid.Sides)
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

				if (renderable.TextureCollection != null && renderable.TextureCollection.ContainsKey(side.TextureName.ToLower()))
				{
					polygon.Texture = renderable.TextureCollection[side.TextureName.ToLower()];
				}
				else
				{
					polygon.Texture = new Texture() { Name = side.TextureName.ToLower() };
				}
				polygon.BasisS = side.TextureBasis[0];
				polygon.BasisT = side.TextureBasis[1];
				polygon.Offset = new Vector2(side.TextureOffset.X, side.TextureOffset.Y);
				polygon.Rotation = side.TextureRotation;
				polygon.Scale = new Vector2(side.TextureScale.X, side.TextureScale.Y);

				polygon.Normal = side.Plane.Normal;

				renderable.Polygons.Add(polygon);
			}
		}

		/// <summary>
		/// Calculate all valid intersection points of the provided solid, and
		/// add a vertex for each point to all that solid's sides that share it.
		/// </summary>
		/// <param name="solid">The solid to find the intersections of.</param>
		/// <param name="color">The color to use for all created vertices.</param>
		private static void CalculateIntersections(Solid solid, Color4 color)
		{
			var sides = solid.Sides;

			foreach (var combo in MathUtilities.Combinations(sides.Count, 3))
			{
				Vector3 intersection = Plane.Intersect(sides[combo[0]].Plane, sides[combo[1]].Plane, sides[combo[2]].Plane);
				if (!intersection.X.Equals(float.NaN) &&
					!intersection.Y.Equals(float.NaN) &&
					!intersection.Z.Equals(float.NaN) &&
					VertexIsLegal(intersection, solid))
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
		/// Determine whether the provided 3D point is valid, given the solid it
		/// was calculated from.
		/// </summary>
		/// <param name="vertex">The point to check.</param>
		/// <param name="solid">The solid to compare against.</param>
		/// <returns>True if the point is on or behind all the solid's sides.</returns>
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
		private static bool VertexIsLegal(Vertex vertex, Solid solid)
		{
			return VertexIsLegal(vertex.Position, solid);
		}
		private static bool VertexIsLegal(Vector3 vertex, Solid solid)
		{
			var inFront = false;

			foreach (var side in solid.Sides)
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
