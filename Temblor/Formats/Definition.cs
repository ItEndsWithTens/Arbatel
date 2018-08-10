using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temblor.Graphics;

namespace Temblor.Formats
{
	public enum ClassType
	{
		/// <summary>
		/// An abstract class, meant to serve only as the base of inherited types.
		/// </summary>
		Base,

		/// <summary>
		/// An entity with geometry.
		/// </summary>
		Solid,

		/// <summary>
		/// An entity without geometry.
		/// </summary>
		Point
	}

	public struct Flag
	{
		public string Description;
		public string Default;
	}

	public class Option
	{
		public string Type;
		public string Description;
		public string Default;
		public Dictionary<string, string> Choices;
		public string Remarks;

		public string Value;

		public Option()
		{
			Choices = new Dictionary<string, string>();
		}
	}

	public class Definition
	{
		/// <summary>
		/// Names of the base classes this entity inherits from.
		/// </summary>
		public List<string> BaseNames;

		public string ClassName;

		public ClassType ClassType;

		public Color4 Color;

		public string Description;

		public Dictionary<string, Flag> Flags;

		// TODO: Implement models! For now just use bounding boxes.
		//public Model Model;

		/// <summary>
		/// The predefined list of key/value pairs this entity might contain.
		/// </summary>
		/// <remarks>
		/// Entities often won't have everything contained in this template, and
		/// it's possible to add keys to an entity even if they aren't defined
		/// in this set, so this only a template of expected possibilities.
		/// </remarks>
		public Dictionary<string, List<Option>> KeyValsTemplate;

		public Vector3 Offset;

		public AABB Size;

		public Definition()
		{
			BaseNames = new List<string>();

			Color = new Color4();

			Flags = new Dictionary<string, Flag>();

			KeyValsTemplate = new Dictionary<string, List<Option>>();

			Offset = new Vector3();
		}
	}
}
