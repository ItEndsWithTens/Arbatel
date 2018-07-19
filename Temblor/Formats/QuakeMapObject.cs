using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Graphics;

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

			var verts = new List<Vertex>();

			var combos = Utilities.MathUtilities.Combinations(sides.Count, 3);
			foreach (var combo in combos)
			{
				var intersection = Plane.Intersect(sides[combo[0]].Plane, sides[combo[1]].Plane, sides[combo[2]].Plane);
				if (!intersection.X.Equals(float.NaN) &&
					!intersection.Y.Equals(float.NaN) &&
					!intersection.Z.Equals(float.NaN))
				{
					verts.Add(new Vertex(intersection));
				}
			}

			foreach (var vert in verts)
			{
				if (VertexIsLegal(vert, sides))
				{
					Renderables.Add(new Renderable() { Position = vert.Position });
				}
			}
		}

		private bool VertexIsLegal(Vertex vertex, List<QuakeSide> sides)
		{
			var inFront = false;

			foreach (var side in sides)
			{
				var dot = Vector3.Dot(side.Plane.Normal, vertex.Position);

				var diff = dot + side.Plane.DistanceFromOrigin;

				inFront = diff < 0 && Math.Abs(diff) > 0.0001f;

				if (inFront)
				{
					break;
				}
			}

			return !inFront;
		}
	}
}
