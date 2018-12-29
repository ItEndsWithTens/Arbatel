using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Arbatel.Graphics;

namespace Arbatel.Formats
{
	public enum TransformType
	{
		/// <summary>
		/// This key doesn't need to be modified when transforming its entity.
		/// </summary>
		None,

		/// <summary>
		/// This key represents a 3D point.
		/// </summary>
		Position,

		/// <summary>
		/// This key represents a set of angles in pitch/yaw/roll form.
		/// </summary>
		Angles,

		/// <summary>
		/// This key needs its name fixed up during instance collapse.
		/// </summary>
		Name
	}

	public class QuakeFgd : DefinitionDictionary
	{
		public QuakeFgd() : base()
		{
			// TODO: Either load these from disk, or offer a UI dialog to set these for a given
			// FGD. ericw's _sun_mangle is not in every Quake FGD, and when it is, it's a string
			// type, so it needs to be in the overrides but shouldn't be there all the time, in
			// principle. Or should it? The overrides are just things one might find, not things
			// to depend on, so if they're not in a map it's no big deal.
			TransformTypeOverrides = new Dictionary<string, TransformType>()
			{
				{ "origin", TransformType.Position },
				{ "angles", TransformType.Angles },
				{ "mangle", TransformType.Angles },
				{ "_sun_mangle", TransformType.Angles },
				{ "_project_mangle", TransformType.Angles }
			};

			TransformTypes = new Dictionary<string, TransformType>()
			{
				{ "target_source", TransformType.Name },
				{ "target_destination", TransformType.Name }
			};
		}
		public QuakeFgd(string filename) : this(new FileStream(filename, FileMode.Open, FileAccess.Read))
		{
		}
		public QuakeFgd(Stream stream) : this()
		{
			using (var sr = new StreamReader(stream))
			{
				Parse(sr);
			}
		}

		public override void Parse(StreamReader sr)
		{
			List<string> raw = Preprocess(sr);

			List<Definition> flat = GetFlatClassList(raw);

			List<Definition> resolved = ResolveClassInheritance(flat);

			foreach (var definition in resolved)
			{
				Add(definition.ClassName, definition);
			}
		}

		/// <summary>
		/// Prepare an input FGD for subsequent parsing.
		/// </summary>
		/// <param name="sr">The StreamReader to pull input from.</param>
		/// <returns></returns>
		public override List<string> Preprocess(StreamReader sr)
		{
			string all = sr.ReadToEnd();

			// It's important to strip out comments first, in case they include
			// any delimiters used in the subsequent split. Waiting until after
			// splitting would make eliminating them a pain.
			string noComments = Regex.Replace(all, "//.*?[\r\n]", "");

			// Split on and keep absolutely everything in an FGD that can be
			// treated as a delimiter. Some things will be reassembled later.
			var delimiters = "(\\[|\\]|\\r|\\n|\\t|=|:|,|\\(|\\)|\\s)";
			return Regex.Split(noComments, delimiters).Select(s => s.Trim()).Where(s => s != "").ToList();
		}

		private List<string> ExtractHeaderProperty(string name, List<string> header)
		{
			var values = new List<string>();

			int openParenthesis = header.IndexOf(name) + 1;
			int closeParenthesis = FindClosingDelimiter(header, openParenthesis);

			for (var i = openParenthesis + 1; i < closeParenthesis; i++)
			{
				var value = header[i];

				if (value != ",")
				{
					values.Add(header[i]);
				}
			}

			return values;
		}

		/// <summary>
		/// Given an input list and the index of an opening delimiter, find the
		/// matching closing delimiter.
		/// </summary>
		/// <param name="raw">The list to search.</param>
		/// <param name="openIndex">The index of the open delimiter to match.</param>
		/// <returns></returns>
		private int FindClosingDelimiter(List<string> raw, int openIndex)
		{
			string openDelimiter = raw[openIndex];
			string closeDelimiter;
			if (openDelimiter == "[")
			{
				closeDelimiter = "]";
			}
			else if (openDelimiter == "(")
			{
				closeDelimiter = ")";
			}
			else if (openDelimiter == "{")
			{
				closeDelimiter = "}";
			}
			else
			{
				throw new ArgumentException("Unrecognized open delimiter!");
			}

			var closeIndex = 0;

			var count = 0;

			for (var i = openIndex; i < raw.Count; i++)
			{
				if (raw[i] == openDelimiter)
				{
					count++;
				}
				else if (raw[i] == closeDelimiter)
				{
					count--;
				}

				if (count == 0)
				{
					closeIndex = i;
					break;
				}
			}

			return closeIndex;
		}

		private int GetBlockLength(List<string> raw, int start)
		{
			var length = 0;

			var braces = 0;

			for (var i = start; i < raw.Count; i++)
			{
				if (raw[i] == "[")
				{
					length++;
					braces++;
					break;
				}
				else
				{
					length++;
				}
			}

			for (var i = start + length; i < raw.Count; i++)
			{
				if (raw[i] == "[")
				{
					braces++;
				}
				else if (raw[i] == "]")
				{
					braces--;
				}

				length++;

				if (braces == 0)
				{
					break;
				}
			}

			return length;
		}

		/// <summary>
		/// Build a list of all the classes in an FGD, without resolving inheritance.
		/// </summary>
		private List<Definition> GetFlatClassList(List<string> raw)
		{
			var flat = new List<Definition>();

			var blockStart = 0;
			while (blockStart < raw.Count)
			{
				int blockLength = GetBlockLength(raw, blockStart);

				List<string> block = raw.GetRange(blockStart, blockLength);

				var def = new Definition()
				{
					DefinitionCollection = this,
					Saveability = Saveability.All
				};

				var blockOffset = 0;
				while (blockOffset < block.Count - 1)
				{
					if (block[blockOffset].StartsWith("@"))
					{
						var header = block.GetRange(blockOffset, block.IndexOf("[", blockOffset));

						string type = header[0].Substring(1).ToLower();
						if (type == "pointclass")
						{
							def.ClassType = ClassType.Point;
							def.RenderableTransformability = Transformability.Translate;
						}
						else if (type == "solidclass")
						{
							def.ClassType = ClassType.Solid;
							def.RenderableSources.Add(RenderableSource.Solids, "");
							def.RenderableTransformability = Transformability.All;
						}
						else
						{
							def.ClassType = ClassType.Base;
							def.Saveability = Saveability.None;
							def.RenderableTransformability = Transformability.None;
						}

						int classnameStart = header.IndexOf("=") + 1;
						int classnameLength = header.IndexOf(":") - classnameStart;
						if (classnameLength < 0)
						{
							classnameLength = header.Count - classnameStart;
						}
						def.ClassName = String.Join("", header.GetRange(classnameStart, classnameLength));

						int descriptionStart = header.IndexOf(":") + 1;
						if (descriptionStart > 0)
						{
							int descriptionLength = header.Count - descriptionStart;
							def.Description = String.Join(" ", header.GetRange(descriptionStart, descriptionLength));
						}

						if (header.Contains("base"))
						{
							List<string> vals = ExtractHeaderProperty("base", header);

							foreach (var value in vals)
							{
								def.BaseNames.Add(value);
							}
						}

						if (header.Contains("color"))
						{
							List<string> vals = ExtractHeaderProperty("color", header);

							int.TryParse(vals[0], out int red);
							int.TryParse(vals[1], out int green);
							int.TryParse(vals[2], out int blue);

							var color = new Color4()
							{
								R = red / 255.0f,
								G = green / 255.0f,
								B = blue / 255.0f,
								A = 1.0f
							};

							def.Color = color;
						}
						else
						{
							// Color can't be assigned null, so there's no easy
							// way to determine whether the FGD provided a color
							// or not; since FGDs don't have any facility for
							// dictating alpha, however, setting it to 0 works.
							def.Color = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
						}

						if (header.Contains("flags"))
						{
							List<string> flags = ExtractHeaderProperty("flags", header);

							if (flags.Contains("Angle"))
							{
								def.KeyValsTemplate.Add("angles", new Option() { TransformType = TransformType.Angles });
							}
						}

						if (header.Contains("iconsprite"))
						{
							List<string> path = ExtractHeaderProperty("iconsprite", header);

							def.RenderableSources.Add(RenderableSource.Sprite, path[0]);
						}

						if (header.Contains("offset"))
						{
							List<string> vals = ExtractHeaderProperty("offset", header);

							var offset = new Vector3();
							float.TryParse(vals[0], out offset.X);
							float.TryParse(vals[1], out offset.Y);
							float.TryParse(vals[2], out offset.Z);

							def.Offset = offset;
						}

						if (header.Contains("size"))
						{
							List<string> vals = ExtractHeaderProperty("size", header);

							var size = new Aabb();

							// Size defined by custom min and max.
							if (vals.Count == 6)
							{
								var min = new Vector3();
								float.TryParse(vals[0], out min.X);
								float.TryParse(vals[1], out min.Y);
								float.TryParse(vals[2], out min.Z);

								var max = new Vector3();
								float.TryParse(vals[3], out max.X);
								float.TryParse(vals[4], out max.Y);
								float.TryParse(vals[5], out max.Z);

								size.Min = min;
								size.Max = max;
							}
							// Size defined by width, depth, and height.
							else
							{
								float.TryParse(vals[0], out float width);
								float.TryParse(vals[1], out float depth);
								float.TryParse(vals[2], out float height);

								size.Max = new Vector3()
								{
									X = width / 2.0f,
									Y = depth / 2.0f,
									Z = height / 2.0f
								};

								size.Min = -size.Max;
							}

							def.Size = size;

							def.RenderableSources.Add(RenderableSource.Size, String.Empty);
						}

						if (header.Contains("sprite"))
						{
							List<string> path = ExtractHeaderProperty("sprite", header);

							def.RenderableSources.Add(RenderableSource.Sprite, path[0]);
						}

						if (header.Contains("studio"))
						{
							string path = ExtractHeaderProperty("studio", header)[0];

							def.RenderableSources.Add(RenderableSource.Model, path);
						}

						if (header.Contains("instance"))
						{
							string value = ExtractHeaderProperty("instance", header)[0];

							def.RenderableSources.Add(RenderableSource.Key, value);

							def.Saveability = Saveability.Entity;
						}

						blockOffset += header.Count;
					}
					else if (block[blockOffset].ToLower() == "spawnflags")
					{
						blockOffset = block.IndexOf("[", blockOffset);

						int closeBracket = FindClosingDelimiter(block, blockOffset);

						while (blockOffset != closeBracket)
						{
							if (block[blockOffset] != "[")
							{
								string flagKey = block[blockOffset];
								blockOffset += 2;

								string description = block[blockOffset];
								blockOffset++;
								while (!description.EndsWith("\""))
								{
									description += " " + block[blockOffset];
									blockOffset++;
								}
								blockOffset++;

								string defaultValue = block[blockOffset];
								blockOffset++;

								var flag = new Spawnflag
								{
									Description = description,
									Default = defaultValue
								};

								def.Flags.Add(flagKey, flag);
							}
							else
							{
								blockOffset++;
							}
						}

						blockOffset++;
					}
					else if (block[blockOffset + 1] == "(")
					{
						var option = new Option();

						string key = block[blockOffset].ToLower();
						blockOffset += 2;

						option.Type = block[blockOffset].ToLower();
						blockOffset += 3;

						if (TransformTypeOverrides.ContainsKey(key))
						{
							option.TransformType = TransformTypeOverrides[key];
						}
						else if (TransformTypes.ContainsKey(option.Type))
						{
							option.TransformType = TransformTypes[option.Type];
						}
						else
						{
							option.TransformType = TransformType.None;
						}

						option.Description = block[blockOffset];
						blockOffset++;

						for (var i = 1; !option.Description.EndsWith("\""); i++)
						{
							option.Description += " " + block[blockOffset];
							blockOffset++;
						}

						// If there's nothing after the description, there's no
						// more work to do for this option.
						if (block[blockOffset] != ":")
						{
							def.KeyValsTemplate.Add(key, option);
							continue;
						}
						blockOffset++;

						// If there is a colon after the description, there's at
						// least space for a default value, even if it's blank.
						bool defaultIsBlank = block[blockOffset] == ":";
						if (!defaultIsBlank)
						{
							string defaultValue = block[blockOffset];
							blockOffset++;

							// If the default is delimited by a double quote, it
							// needs to be assembled from separated pieces.
							if (defaultValue.StartsWith("\""))
							{
								while (block[blockOffset] != ":" && block[blockOffset] != "=")
								{
									defaultValue += " " + block[blockOffset];
									blockOffset++;
								}
							}

							option.Default = defaultValue;
						}

						bool hasRemarks = block[blockOffset] == ":" && block[blockOffset + 1].StartsWith("\"");
						if (hasRemarks)
						{
							option.Remarks = block[blockOffset];
							blockOffset++;
							while (!option.Remarks.EndsWith("\""))
							{
								option.Remarks += " " + block[blockOffset];
								blockOffset++;
							}
						}

						bool hasChoices = block[blockOffset] == "=";

						if (hasChoices)
						{
							blockOffset++;

							int closeBracket = FindClosingDelimiter(block, blockOffset);

							while (blockOffset != closeBracket)
							{
								if (block[blockOffset] != "[")
								{
									string choice = block[blockOffset];
									blockOffset += 2;

									string text = block[blockOffset];
									blockOffset++;

									// Spaces were otherwise stripped out, but
									// there should be one in this case.
									if (!text.EndsWith("\""))
									{
										text += " ";
									}

									while (!text.EndsWith("\""))
									{
										text += block[blockOffset];
										blockOffset++;
									}

									option.Choices.Add(choice, text);
								}
								else
								{
									blockOffset++;
								}
							}
						}

						if (!def.KeyValsTemplate.ContainsKey(key))
						{
							def.KeyValsTemplate.Add(key, option);
						}
					}
					else
					{
						blockOffset++;
					}
				}

				if (def.ClassType == ClassType.Point && !def.KeyValsTemplate.ContainsKey("origin"))
				{
					def.KeyValsTemplate.Add("origin", new Option() { TransformType = TransformType.Position });
				}

				if (!def.KeyValsTemplate.ContainsKey("classname"))
				{
					def.KeyValsTemplate.Add("classname", new Option());
				}

				flat.Add(def);

				blockStart += blockLength;
			}

			return flat;
		}

		private List<Definition> ResolveClassInheritance(List<Definition> flat)
		{
			var resolved = new List<Definition>();

			var bases = new List<Definition>();
			var points = new List<Definition>();
			var solids = new List<Definition>();

			foreach (Definition def in flat)
			{
				if (def.ClassType == ClassType.Solid)
				{
					solids.Add(def);
				}
				else if (def.ClassType == ClassType.Point)
				{
					points.Add(def);
				}
				else
				{
					bases.Add(def);
				}
			}

			resolved.AddRange(bases.OrderBy(d => d.BaseNames.Count).ToList());
			resolved.AddRange(points.OrderBy(d => d.BaseNames.Count).ToList());
			resolved.AddRange(solids.OrderBy(d => d.BaseNames.Count).ToList());

			foreach (var def in resolved)
			{
				foreach (var name in def.BaseNames)
				{
					Definition baseClass = resolved.Find(d => d.ClassName == name);

					foreach (var flag in baseClass.Flags)
					{
						if (!def.Flags.ContainsKey(flag.Key))
						{
							def.Flags.Add(flag.Key, flag.Value);
						}
					}

					foreach (var keyval in baseClass.KeyValsTemplate)
					{
						if (!def.KeyValsTemplate.ContainsKey(keyval.Key))
						{
							def.KeyValsTemplate.Add(keyval.Key, keyval.Value);
						}
					}

					// This is the easiest way to check whether this entity has
					// a color defined; checks for null won't work.
					if (def.Color.R == 0.0f && def.Color.G == 0.0f && def.Color.B == 0.0f && def.Color.A == 0.0f)
					{
						var c = baseClass.Color;

						// It is of course possible the base class also has no
						// color, in which case white will do nicely.
						if (c.R == 0.0f && c.G == 0.0f && c.B == 0.0f && c.A == 0.0f)
						{
							def.Color = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
						}
						else
						{
							def.Color = baseClass.Color;
						}
					}

					def.Offset += baseClass.Offset;

					if (def.Size == null)
					{
						def.Size = baseClass.Size;
					}

					foreach (var source in baseClass.RenderableSources)
					{
						if (!def.RenderableSources.ContainsKey(source.Key))
						{
							def.RenderableSources.Add(source.Key, source.Value);
						}
					}
				}
			}

			return resolved;
		}
	}
}
