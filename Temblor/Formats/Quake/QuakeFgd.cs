using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Temblor.Graphics;

namespace Temblor.Formats
{
	public class QuakeFgd : DefinitionFile
	{
		public QuakeFgd() : base()
		{
		}
		public QuakeFgd(Stream stream) : base(stream)
		{
			var bases = new List<Definition>();
			var points = new List<Definition>();
			var solids = new List<Definition>();

			foreach (Definition def in Definitions)
			{
				if (def.ClassType == ClassType.Solid && def.BaseNames.Count > 0)
				{
					solids.Add(def);
				}
				else if (def.ClassType == ClassType.Point && def.BaseNames.Count > 0)
				{
					points.Add(def);
				}
				else if (def.BaseNames.Count > 0)
				{
					bases.Add(def);
				}
			}

			var defs = new List<Definition>();
			defs.AddRange(bases.OrderBy(d => d.BaseNames.Count).ToList());
			defs.AddRange(points.OrderBy(d => d.BaseNames.Count).ToList());
			defs.AddRange(solids.OrderBy(d => d.BaseNames.Count).ToList());

			foreach (var d in defs)
			{
				foreach (var name in d.BaseNames)
				{
					var baseClass = this[name];

					foreach (var flag in baseClass.Flags)
					{
						if (!d.Flags.ContainsKey(flag.Key))
						{
							d.Flags.Add(flag.Key, flag.Value);
						}
					}

					foreach (var keyval in baseClass.KeyVals)
					{
						if (!d.KeyVals.ContainsKey(keyval.Key))
						{
							d.KeyVals.Add(keyval.Key, keyval.Value);
						}
					}

					// This is the easiest way to check whether this entity has
					// a color defined; checks for null won't work.
					if (d.Color.R == 0.0f && d.Color.G == 0.0f && d.Color.B == 0.0f && d.Color.A == 0.0f)
					{
						var c = baseClass.Color;

						// It is of course possible the base class also has no
						// color, in which case white will do nicely.
						if (c.R == 0.0f && c.G == 0.0f && c.B == 0.0f && c.A == 0.0f)
						{
							d.Color = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
						}
						else
						{
							d.Color = baseClass.Color;
						}
					}

					d.Offset += baseClass.Offset;

					if (d.Size == null)
					{
						d.Size = baseClass.Size;
					}
				}
			}
		}

		public override void Parse(StreamReader sr)
		{
			Raw = Preprocess(sr);

			var blockStart = 0;
			while (blockStart < Raw.Count)
			{
				int blockLength = GetBlockLength(Raw, blockStart);

				List<string> block = Raw.GetRange(blockStart, blockLength);

				var def = new Definition();

				var blockOffset = 0;
				while (blockOffset < block.Count - 1)
				{
					var item = block[blockOffset];

					if (item.StartsWith("@"))
					{
						var header = block.GetRange(blockOffset, block.IndexOf("[", blockOffset));

						string type = header[0].Substring(1).ToLower();
						if (type == "pointclass")
						{
							def.ClassType = ClassType.Point;
						}
						else if (type == "solidclass")
						{
							def.ClassType = ClassType.Solid;
						}
						else
						{
							def.ClassType = ClassType.Base;
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

						}

						if (header.Contains("iconsprite"))
						{

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

							var size = new AABB();

							// Size defined by custom min and max.
							if (vals.Count == 6)
							{
								float.TryParse(vals[0], out size.Min.X);
								float.TryParse(vals[1], out size.Min.Y);
								float.TryParse(vals[2], out size.Min.Z);

								float.TryParse(vals[3], out size.Max.X);
								float.TryParse(vals[4], out size.Max.Y);
								float.TryParse(vals[5], out size.Max.Z);
							}
							// Size defined by width, depth, and height.
							else
							{
								float.TryParse(vals[0], out float width);
								float.TryParse(vals[1], out float depth);
								float.TryParse(vals[2], out float height);

								size.Max.X = width / 2.0f;
								size.Max.Y = depth / 2.0f;
								size.Max.Z = height / 2.0f;

								size.Min.X = -size.Max.X;
								size.Min.Y = -size.Max.Y;
								size.Min.Z = -size.Max.Z;
							}

							def.Size = size;
						}

						if (header.Contains("sprite"))
						{

						}

						if (header.Contains("studio"))
						{

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

								var flag = new Flag
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

						string key = item.ToLower();
						blockOffset += 2;

						option.Type = block[blockOffset].ToLower();
						blockOffset += 3;

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
							def.KeyVals.Add(key, new List<Option>() { option });
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

						def.KeyVals.Add(key, new List<Option>() { option });
					}
					else
					{
						blockOffset++;
					}
				}

				Definitions.Add(def);

				blockStart += blockLength;
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

			// Split on brackets, carriage return, newline, or tab, keeping
			// those delimiters by surrounding them with a capture group, but
			// also split on end-of-line comments, discarding them instead.
			//var delimiters = "(\\[|\\]|\\r|\\n|\\t)|//.*?[\r\n]";
			//return Regex.Split(sr.ReadToEnd(), delimiters).Select(s => s.Trim()).Where(s => s != "").ToList();

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
	}
}
