using Arbatel.Graphics;
using Arbatel.Utilities;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
			var sb = new StringBuilder();
			using (var sr = new StreamReader(stream))
			{
				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();

					int commentStart = line.IndexOf("//", StringComparison.OrdinalIgnoreCase);
					switch (commentStart)
					{
						case -1:
							sb.Append(line);
							break;
						case 0:
							continue;
						default:
							sb.Append(line.Remove(commentStart));
							break;
					}
				}
			}

			Parse(sb.ToString());
		}

		public void Parse(string raw)
		{
			List<string> split = raw.SplitAndKeepDelimiters(
				"[", "]", "\t", "=", ":", ",", "(", ")", " ").ToList();
			split.RemoveAll(item => item.Length == 0);

			var pruned = new List<string>();

			int line = 0;
			int quotes = 0;
			while (line < split.Count)
			{
				string item = split[line];
				quotes = item.Count(i => i == '\"');

				if (quotes % 2 == 0)
				{
					if (item.Trim().Length > 0)
					{
						pruned.Add(item);
					}
				}
				else
				{
					while (quotes % 2 != 0)
					{
						line++;
						item += split[line];
						quotes = item.Count(i => i == '\"');
					}

					pruned.Add(item);
				}

				line++;
			}

			List<Definition> flat = GetFlatClassList(pruned);

			List<Definition> resolved = ResolveClassInheritance(flat);

			foreach (Definition definition in resolved)
			{
				Add(definition.ClassName, definition);
			}
		}

		private List<(string, string)> ExtractChoices(List<string> block, int blockOffset)
		{
			var choices = new List<(string, string)>();

			int closeBracket = FindClosingDelimiter(block, blockOffset);

			while (blockOffset != closeBracket)
			{
				if (block[blockOffset] != "[")
				{
					string choice = block[blockOffset];
					blockOffset += 2;

					string text = block[blockOffset].TrimStart('\"').TrimEnd('\"');

					choices.Add((choice, text));
				}

				blockOffset++;
			}

			return choices;
		}

		private List<string> ExtractHeaderProperty(string name, List<string> header)
		{
			var values = new List<string>();

			int openParenthesis = header.IndexOf(name) + 1;
			int closeParenthesis = FindClosingDelimiter(header, openParenthesis);

			for (int i = openParenthesis + 1; i < closeParenthesis; i++)
			{
				string value = header[i];

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

			int closeIndex = 0;

			int count = 0;

			for (int i = openIndex; i < raw.Count; i++)
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
			int length = 0;

			int braces = 0;

			for (int i = start; i < raw.Count; i++)
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

			for (int i = start + length; i < raw.Count; i++)
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

			int blockStart = 0;
			while (blockStart < raw.Count)
			{
				int blockLength = GetBlockLength(raw, blockStart);

				List<string> block = raw.GetRange(blockStart, blockLength);

				var def = new Definition()
				{
					DefinitionCollection = this,
					Saveability = Saveability.All
				};

				int blockOffset = 0;
				while (blockOffset < block.Count - 1)
				{
					if (block[blockOffset].StartsWith("@"))
					{
						List<string> header = block.GetRange(blockOffset, block.IndexOf("[", blockOffset));

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
							def.Description = def.Description.TrimStart('\"').TrimEnd('\"');
						}

						if (header.Contains("base"))
						{
							List<string> vals = ExtractHeaderProperty("base", header);

							foreach (string value in vals)
							{
								def.BaseNames.Add(value);
							}
						}

						if (header.Contains("color"))
						{
							List<string> vals = ExtractHeaderProperty("color", header);

							Int32.TryParse(vals[0], out int red);
							Int32.TryParse(vals[1], out int green);
							Int32.TryParse(vals[2], out int blue);

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
							Single.TryParse(vals[0], out offset.X);
							Single.TryParse(vals[1], out offset.Y);
							Single.TryParse(vals[2], out offset.Z);

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
								Single.TryParse(vals[0], out min.X);
								Single.TryParse(vals[1], out min.Y);
								Single.TryParse(vals[2], out min.Z);

								var max = new Vector3();
								Single.TryParse(vals[3], out max.X);
								Single.TryParse(vals[4], out max.Y);
								Single.TryParse(vals[5], out max.Z);

								size.Min = min;
								size.Max = max;
							}
							// Size defined by width, depth, and height.
							else
							{
								Single.TryParse(vals[0], out float width);
								Single.TryParse(vals[1], out float depth);
								Single.TryParse(vals[2], out float height);

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
							List<string> paths = ExtractHeaderProperty("studio", header);

							string path = paths.Count > 0 ? paths[0] : null;

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
								var flag = new Spawnflag();

								string flagKey = block[blockOffset];
								blockOffset += 2;

								flag.Description = block[blockOffset].TrimStart('\"').TrimEnd('\"');
								blockOffset++;

								bool hasDefault = block[blockOffset] == ":" && block[blockOffset + 1] != ":";
								if (hasDefault)
								{
									blockOffset++;
									flag.Default = block[blockOffset];
									blockOffset++;
								}

								bool hasRemarks = block[blockOffset] == ":" && block[blockOffset + 1].StartsWith("\"");
								if (hasRemarks)
								{
									blockOffset++;
									flag.Remarks = block[blockOffset].TrimStart('\"').TrimEnd('\"');
									blockOffset++;
								}

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

						int choicesFirst = -1;
						int choicesLast = -1;
						if (option.Type == "choices")
						{
							choicesFirst = block.IndexOf("[", blockOffset);
							choicesLast = FindClosingDelimiter(block, choicesFirst);

							foreach ((string k, string v) in ExtractChoices(block, choicesFirst))
							{
								option.Choices.Add(k, v);
							}
						}

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

						option.Description = block[blockOffset].TrimStart('\"').TrimEnd('\"');
						blockOffset++;

						// If there's nothing after the description, there's no
						// more work to do for this option.
						if (block[blockOffset] != ":")
						{
							if (def.KeyValsTemplate.ContainsKey(key))
							{
								def.KeyValsTemplate[key] = option;
							}
							else
							{
								def.KeyValsTemplate.Add(key, option);
							}

							if (choicesLast > -1)
							{
								blockOffset = choicesLast;
							}
							continue;
						}
						blockOffset++;

						// If there is a colon after the description, there's at
						// least space for a default value, even if it's blank.
						bool defaultIsBlank = block[blockOffset] == ":" || block[blockOffset] == "=";
						if (!defaultIsBlank)
						{
							option.Default = block[blockOffset].TrimStart('\"').TrimEnd('\"');
							blockOffset++;
						}

						bool hasRemarks = block[blockOffset] == ":" && block[blockOffset + 1].StartsWith("\"");
						if (hasRemarks)
						{
							blockOffset++;
							option.Remarks = block[blockOffset].TrimStart('\"').TrimEnd('\"');
							blockOffset++;
						}

						if (def.KeyValsTemplate.ContainsKey(key))
						{
							def.KeyValsTemplate[key] = option;
						}
						else
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

			foreach (Definition def in resolved)
			{
				foreach (string name in def.BaseNames)
				{
					Definition baseClass = resolved.Find(d => d.ClassName == name);

					foreach (KeyValuePair<string, Spawnflag> flag in baseClass.Flags)
					{
						if (!def.Flags.ContainsKey(flag.Key))
						{
							def.Flags.Add(flag.Key, flag.Value);
						}
					}

					foreach (KeyValuePair<string, Option> keyval in baseClass.KeyValsTemplate)
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
						Color4 c = baseClass.Color;

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

					foreach (KeyValuePair<RenderableSource, string> source in baseClass.RenderableSources)
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
