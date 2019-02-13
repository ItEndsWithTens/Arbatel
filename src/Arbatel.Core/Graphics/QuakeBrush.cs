﻿using Arbatel.Formats;
using Arbatel.Utilities;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbatel.Graphics
{
	public class QuakeBrush : Renderable
	{
		/// <summary>
		/// The tolerance to use when comparing floats in geometry calculations.
		/// </summary>
		private const float Tolerance = 0.01f;

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

		private static void BuildPolygons(Solid solid, Renderable renderable)
		{
			foreach (Side side in solid.Sides)
			{
				if (side.Vertices.Count < 3)
				{
					continue;
				}

				var p = new Polygon
				{
					Texture = new Texture() { Name = side.TextureName.ToLower() },
					BasisS = side.TextureBasis[0],
					BasisT = side.TextureBasis[1],
					Offset = new Vector2(side.TextureOffset.X, side.TextureOffset.Y),
					Rotation = side.TextureRotation,
					Scale = new Vector2(side.TextureScale.X, side.TextureScale.Y),

					Normal = side.Plane.Normal
				};

				side.Vertices = MathUtilities.SortVertices(side.Vertices, side.Plane.Normal, Winding.Ccw);

				for (int i = 0; i < side.Vertices.Count; i++)
				{
					Vertex v = side.Vertices[i];
					v.Normal = p.Normal;

					renderable.Vertices.Add(v);
					side.Indices.Add(renderable.Vertices.Count - 1);

					p.Indices.Add(side.Indices[i]);
					renderable.Indices.Add(side.Indices[i]);
				}

				renderable.Polygons.Add(p);
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
			List<Side> sides = solid.Sides;

			foreach (List<int> combo in MathUtilities.Combinations(sides.Count, 3))
			{
				Vector3 intersection = Plane.Intersect(combo.Select(n => sides[n].Plane));

				if (IntersectionIsLegal(intersection, solid))
				{
					for (int i = 0; i < 3; i++)
					{
						Side side = sides[combo[i]];

						bool sideContainsVertex = false;
						foreach (Vertex sideVertex in side.Vertices)
						{
							if (MathUtilities.ApproximatelyEquivalent(intersection, sideVertex.Position, Tolerance))
							{
								sideContainsVertex = true;
								break;
							}
						}

						if (!sideContainsVertex)
						{
							side.Vertices.Add(new Vertex(intersection, color));
						}
					}
				}
			}
		}

		public override void UpdateTextureCoordinates()
		{
			foreach (Polygon p in Polygons)
			{
				foreach (int index in p.Indices)
				{
					Vertex v = Vertices[index];

					v.TexCoords = new Vector2
					{
						X = (Vector3.Dot(v.Position, p.BasisS) + (p.Offset.X * p.Scale.X)) / (p.Texture.Width * p.Scale.X),
						Y = (Vector3.Dot(v.Position, p.BasisT) + (p.Offset.Y * p.Scale.Y)) / (p.Texture.Height * p.Scale.Y)
					};

					Vertices[index] = v;
				}
			}
			
		}

		/// <summary>
		/// Determine whether the provided 3D point is valid, given the solid it
		/// was calculated from.
		/// </summary>
		/// <param name="intersection">The point to check.</param>
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
		private static bool IntersectionIsLegal(Vector3 intersection, Solid solid)
		{
			if (intersection.IsNaN())
			{
				return false;
			}

			bool inFront = false;

			foreach (Side side in solid.Sides)
			{
				float dot = Vector3.Dot(side.Plane.Normal, intersection);

				float diff = dot - side.Plane.DistanceFromOrigin;

				inFront = diff > 0.0f && Math.Abs(diff) > Tolerance;

				if (inFront)
				{
					break;
				}
			}

			return !inFront;
		}
	}
}
