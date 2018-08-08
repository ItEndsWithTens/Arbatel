﻿using OpenTK.Graphics;
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

	public class EntityDefinition
	{
		public AABB AABB;

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
		/// The list of key/value pairs this entity has, if any.
		/// </summary>
		/// <remarks>
		/// Quake maps, as one example, permit duplicate keys with different
		/// values, so it's necessary to store values as a List.
		/// </remarks>
		public Dictionary<string, List<Option>> KeyVals;

		public EntityDefinition()
		{
			BaseNames = new List<string>();

			Color = new Color4();

			Flags = new Dictionary<string, Flag>();

			KeyVals = new Dictionary<string, List<Option>>();
		}
	}
}
