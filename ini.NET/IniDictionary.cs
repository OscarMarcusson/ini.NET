using System.Reflection.Metadata;
using System.Text;

namespace System.IO.Ini
{
	public class IniFields
	{
		public bool IsEmpty => Fields.Count == 0;
		public int NumberOfFields => Fields.Count;
		internal readonly Dictionary<string, string> Fields = new Dictionary<string, string>();

		public string? GetField(string key, string? defaultValue = null)
		{
			if (Fields.TryGetValue(key, out var value))
				return value;

			return defaultValue;
		}
	}








	public class IniDictionary : IniFields
	{
		public bool HasAnySections => Sections.Count > 0;
		public int NumberOfSections => Sections.Count;
		readonly Dictionary<string, IniFields> Sections = new Dictionary<string, IniFields>();

		public IniFields? GetSection(string key, IniFields? defaultValue = null)
		{
			if (Sections.TryGetValue(key, out var section))
				return section;

			return defaultValue;
		}

		// "Constructors"
		public static IniDictionary FromStream(Stream stream) => FromStream(stream, Encoding.UTF8);
		public static IniDictionary FromStream(Stream stream, Encoding encoding)
		{
			var reader = new StreamReader(stream, encoding);
			var dictionary = new IniDictionary();

			string? line;
			int startIndex, endIndex;
			while ((line = reader.ReadLine()) != null)
			{
				if (FindStartOfContent(line, out startIndex))
				{
					switch (line[startIndex])
					{
						// [Sections]
						case SectionStart:
							startIndex++;
							endIndex = FindEndOfLine(line);
							if (line[endIndex] != SectionEnd)
							{
								// TODO:: Error handling
								continue;
							}

							break;


						// [Ensure valid start character]
						case SectionEnd:
							// TODO:: Error handling
							continue;


						// [Comments]
						case LineComment:
						case KeyValueComment:
							// TODO:: We ignore comments for now, but they should be saved into some
							//        header info for the next field to be parsed
							break;


						// [Key value pairs]
						default:
							// TODO:: Some setting for if digits are allowed?
							if (FindEndOfKey(line, startIndex, out endIndex))
							{
								var key = line.Substring(startIndex, endIndex - startIndex);
								
								if (dictionary.Fields.ContainsKey(key))
								{
									// TODO:: Error handling
									continue;
								}

								// Resolve the start of the value
								if (line[endIndex] != '=')
								{
									endIndex = line.IndexOf('=', endIndex+1);
									if(endIndex == -1)
									{
										// TODO:: Error handling
										continue;
									}
								}
								endIndex++;
								while (char.IsWhiteSpace(line[endIndex]))
									endIndex++;

								// Everything from this point on is our value, but we should trim the end
								startIndex = endIndex;
								endIndex = FindEndOfLine(line);
								var value = line.Substring(startIndex, endIndex - startIndex + 1);

								dictionary.Fields[key] = value;
							}
							else
							{
								// TODO:: Error, 
							}
							break;
					}
				}
			}

			return dictionary;
		}







		// Constans
		internal const char SectionStart = '[';
		internal const char SectionEnd = ']';
		internal const char LineComment = ';';
		internal const char KeyValueComment = '#';


		// Helpers
		static bool FindStartOfContent(string line, out int index)
		{
			for (index = 0; index < line.Length; index++)
			{
				if (!char.IsWhiteSpace(line[index]))
					return true;
			}

			index = -1;
			return false;
		}

		static bool FindEndOfKey(string line, int startIndex, out int index)
		{
			index = startIndex;
			while (index < line.Length)
			{
				index++;
				if (char.IsWhiteSpace(line[index]) || line[index] == '=')
					return true;
			}

			index = -1;
			return false;
		}

		static int FindEndOfLine(string line)
		{
			for (var i = line.Length - 1; i > 0; i--)
			{
				if (!char.IsWhiteSpace(line[i]))
					return i;
			}
			return 0;
		}
	}
}