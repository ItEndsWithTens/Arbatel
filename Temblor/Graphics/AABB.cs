using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temblor.Graphics
{
	/// <summary>
	/// An axis-aligned bounding box surrounding a given set of points.
	/// </summary>
	public class AABB
	{
		public Vector3 Min;
		public Vector3 Max;
		public Vector3 Center;

		public AABB()
		{
			Min = new Vector3();
			Max = new Vector3();
			Center = new Vector3();
		}
		public AABB(List<Vertex> vertices) : base()
		{
			var points = new List<Vector3>();

			foreach (var vertex in vertices)
			{
				points.Add(vertex.Position);
			}

			Init(points);
		}
		public AABB(List<Vector3> points) : base()
		{
			Init(points);
		}
		public AABB(AABB aabb)
		{
			Min = new Vector3(aabb.Min);
			Max = new Vector3(aabb.Max);
			Center = new Vector3(aabb.Center);
		}

		private void Init(List<Vector3> vertices)
		{
			Min = vertices[0];
			Max = vertices[0];

			foreach (var vertex in vertices)
			{
				if (vertex.X < Min.X)
				{
					Min.X = vertex.X;
				}
				if (vertex.X > Max.X)
				{
					Max.X = vertex.X;
				}

				if (vertex.Y < Min.Y)
				{
					Min.Y = vertex.Y;
				}
				if (vertex.Y > Max.Y)
				{
					Max.Y = vertex.Y;
				}

				if (vertex.Z < Min.Z)
				{
					Min.Z = vertex.Z;
				}
				if (vertex.Z > Max.Z)
				{
					Max.Z = vertex.Z;
				}
			}

			Center = Min + ((Max - Min) / 2.0f);
		}

		public static AABB operator +(AABB lhs, AABB rhs)
		{
			var updated = new AABB(lhs);

			if (rhs.Max.X > updated.Max.X)
			{
				updated.Max.X = rhs.Max.X;
			}

			if (rhs.Max.Y > updated.Max.Y)
			{
				updated.Max.Y = rhs.Max.Y;
			}

			if (rhs.Max.Z > updated.Max.Z)
			{
				updated.Max.Z = rhs.Max.Z;
			}

			if (rhs.Min.X < updated.Min.X)
			{
				updated.Min.X = rhs.Min.X;
			}

			if (rhs.Min.Y < updated.Min.Y)
			{
				updated.Min.Y = rhs.Min.Y;
			}

			if (rhs.Min.Z < updated.Min.Z)
			{
				updated.Min.Z = rhs.Min.Z;
			}

			updated.Center = (updated.Max - updated.Min) / 2.0f;

			return updated;
		}

		public static AABB operator +(AABB lhs, Vector3 rhs)
		{
			return new AABB
			{
				Min = lhs.Min + rhs,
				Max = lhs.Max + rhs,
				Center = lhs.Center + rhs
			};
		}

		public static AABB operator -(AABB lhs, Vector3 rhs)
		{
			return new AABB
			{
				Min = lhs.Min - rhs,
				Max = lhs.Max - rhs,
				Center = lhs.Center - rhs
			};
		}
	}
}
