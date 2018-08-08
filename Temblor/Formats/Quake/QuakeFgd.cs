using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Temblor.Formats
{
	public class QuakeFgd : DefinitionFile
	{
		public QuakeFgd() : base()
		{
		}
		public QuakeFgd(Stream stream) : base(stream)
		{
		}

		public override void Parse(StreamReader sr)
		{
			Raw = Preprocess(sr);

			var blockStart = 0;
			while (blockStart < Raw.Count)
			{
				int blockLength = GetBlockLength(Raw, blockStart);

				List<string> block = Raw.GetRange(blockStart, blockLength);

				var def = new EntityDefinition();

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
						int descriptionLength = header.Count - descriptionStart;
						def.Description = String.Join(" ", header.GetRange(descriptionStart, descriptionLength));

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

						bool earlyFinish = block[blockOffset] != ":";
						if (earlyFinish)
						{
							continue;
						}
						blockOffset++;

						bool hasDefault = !earlyFinish && (block[blockOffset] != ":" && block[blockOffset] != "=");
						if (hasDefault)
						{
							option.Default = block[blockOffset];
							blockOffset++;
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

				Entities.Add(def);

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
			else
			{
				closeDelimiter = "}";
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
