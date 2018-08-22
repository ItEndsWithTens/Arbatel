﻿using OpenTK;
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

	/// <summary>
	/// One possible source of Renderables for a given Definition.
	/// </summary>
	public enum RenderableSource
	{
		/// <summary>
		/// A simple flat shaded box serving as a placeholder for an entity.
		/// </summary>
		Size,

		/// <summary>
		/// One or more 3D shapes that make up an entity's visual aspect.
		/// </summary>
		Solids,

		/// <summary>
		/// A 2D image that represents an entity.
		/// </summary>
		Sprite,

		/// <summary>
		/// A 3D model that represents an entity.
		/// </summary>
		Model,

		/// <summary>
		/// A Renderable that's built from the value of one of an entity's keys.
		/// </summary>
		Key
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

		/// <summary>
		/// The DefinitionCollection this Definition came from.
		/// </summary>
		public DefinitionCollection DefinitionCollection;

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

		/// <summary>
		/// The possible places this Definition's Renderables can be taken from.
		/// </summary>
		public Dictionary<RenderableSource, string> RenderableSources;

		public Aabb Size;

		public Definition()
		{
			BaseNames = new List<string>();
			Color = new Color4();
			Flags = new Dictionary<string, Flag>();
			KeyValsTemplate = new Dictionary<string, List<Option>>();
			Offset = new Vector3();
			RenderableSources = new Dictionary<RenderableSource, string>();
		}
		public Definition(Definition d)
		{
			BaseNames = new List<string>(d.BaseNames);
			ClassName = d.ClassName;
			ClassType = d.ClassType;
			Color = new Color4(d.Color.R, d.Color.G, d.Color.B, d.Color.A);
			DefinitionCollection = d.DefinitionCollection;
			Description = d.Description;
			Flags = new Dictionary<string, Flag>(d.Flags);
			KeyValsTemplate = new Dictionary<string, List<Option>>(d.KeyValsTemplate);
			Offset = new Vector3(d.Offset);
			RenderableSources = new Dictionary<RenderableSource, string>(d.RenderableSources);
			Size = d.Size != null ? new Aabb(d.Size) : null;
		}
	}
}
