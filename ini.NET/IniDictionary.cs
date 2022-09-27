using System.ComponentModel;
using System.Data;
using System.Reflection.Metadata;
using System.Text;
using System.Xml.Linq;

namespace System.IO
{
	public static class Ini
	{
		static Dictionary<Type, InstanceData> InstanceSetters = new Dictionary<Type, InstanceData>();

		class InstanceData
		{
			readonly Type Type;
			readonly Dictionary<string, Action<object, object?>> Values = new Dictionary<string, Action<object, object?>>();


			public InstanceData(Type type)
			{
				Type = type;
				var test = new Dictionary<string, Action<object, object?>>();

				var properties = type.GetProperties().Where(x => x.CanWrite);
				foreach (var property in properties)
					test.Add(property.Name, (instance, value) => property.SetValue(instance, value));

				var fields = type.GetFields().Where(x => x.IsPublic).ToDictionary(x => x.Name);
			}


			public void SetValue<TInstance, TValue>(string name, TInstance instance, TValue value)
			{
				if (instance == null)
					throw new ArgumentNullException($"Failed to set the field \"{name}\" value on instance type \"{Type.Name}\", instance was null");
				
				if (instance.GetType() != Type)
					throw new ArgumentException($"Type missmatch, tried setting the field \"{name}\" to \"{value}\" on expected instance type \"{Type.Name}\" but got an instance with type \"{instance.GetType().Name}\"");

				if (!Values.TryGetValue(name, out var setter))
					throw new KeyNotFoundException($"Could not find a field or property named \"{name}\" in \"{Type.Name}\"");
				
				setter(instance, value);
			}
		}







		public static T DeserializeStream<T>(Stream stream) where T : new() => DeserializeStream<T>(stream, null);
		public static T DeserializeStream<T>(Stream stream, Encoding? encoding) where T : new()
		{
			var instance = new T();
			if (!InstanceSetters.TryGetValue(instance.GetType(), out var setter))
				InstanceSetters[instance.GetType()] = setter = new InstanceData(instance.GetType());

			ParseStream(
				stream, 
				encoding,
				parseSection: name =>
				{

				},
				assignValue: (key, value) =>
				{
					// TODO:: Parse the value without using the Parse<T> call, or at least call it dynamically
					var parsedValue = null as object;
					setter.SetValue(key, instance, parsedValue);
				});

			return instance;
		}

		static void ParseStream(Stream stream, Encoding? encoding, Action<string> parseSection, Action<string,string> assignValue)
		{
			var reader = encoding == null
				? new StreamReader(stream, detectEncodingFromByteOrderMarks: true)
				: new StreamReader(stream, encoding)
				;

			string? line;
			int startIndex, endIndex;
			var duplicateFieldChecker = new HashSet<string>(32);
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

							var sectionName = line.Substring(startIndex, endIndex - startIndex);
							parseSection(sectionName);
							duplicateFieldChecker.Clear();
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

								// Check for duplicates
								if (duplicateFieldChecker.Contains(key))
									throw new DuplicateNameException($"A field called \"{key}\" already exists");
								duplicateFieldChecker.Add(key);

								// Resolve the start of the value
								if (line[endIndex] != '=')
								{
									endIndex = line.IndexOf('=', endIndex + 1);
									if (endIndex == -1)
										throw new SyntaxErrorException($"The \"{key}\" field did not have the assignment character \"=\"");
								}
								endIndex++;
								while (char.IsWhiteSpace(line[endIndex]))
									endIndex++;

								// Everything from this point on is our value, but we should trim the end
								startIndex = endIndex;
								endIndex = Ini.FindEndOfLine(line);
								var value = line.Substring(startIndex, endIndex - startIndex + 1);

								assignValue(key, value);
							}
							else
							{
								// TODO:: Error, 
							}
							break;
					}
				}
			}
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




		public class Fields
		{
			public bool IsEmpty => Values.Count == 0;
			public int NumberOfFields => Values.Count;
			internal readonly System.Collections.Generic.Dictionary<string, string> Values = new System.Collections.Generic.Dictionary<string, string>();

			public string? GetField(string key, string? defaultValue = null)
			{
				if (Values.TryGetValue(key, out var value))
					return value;

				return defaultValue;
			}


			#pragma warning disable CS8601 // Possible null reference assignment.
			public T GetField<T>(string key, T defaultValue = default)
			#pragma warning restore CS8601 // Possible null reference assignment.
			{
				if (Values.TryGetValue(key, out var value))
				{
					var parsedValue = Parse(value, defaultValue);
					return parsedValue;
				}
				return defaultValue;
			}

		}


		static T Parse<T>(object stringValue, T defaultValue)
		{
			try
			{
				if (Nullable.GetUnderlyingType(typeof(T)) != null)
				{
					var conv = TypeDescriptor.GetConverter(typeof(T));
					return (T)conv.ConvertFrom(stringValue);
				}
				else
				{
					return (T)Convert.ChangeType(stringValue, typeof(T));
				}
			}
			catch (Exception)
			{
				return defaultValue;
			}
		}



		public class Dictionary : Fields
		{
			public bool HasAnySections => Sections.Count > 0;
			public int NumberOfSections => Sections.Count;
			public System.Collections.Generic.Dictionary<string, Fields> Sections = new System.Collections.Generic.Dictionary<string, Fields>();

			public Fields? GetSection(string key, Fields? defaultValue = null)
			{
				if (Sections.TryGetValue(key, out var section))
					return section;

				return defaultValue;
			}
			
			
			
			
			
			
			public static Dictionary FromStream(Stream stream) => FromStream(stream, null);
			public static Dictionary FromStream(Stream stream, Encoding? encoding)
			{
				var dictionary = new Dictionary();
				Fields? currentSection = dictionary;

				ParseStream(
					stream, 
					encoding,
					parseSection: name =>
					{
						if (dictionary.Sections.ContainsKey(name))
						{
							currentSection = null;
							throw new DuplicateNameException($"A section called \"{name}\" already exists");
						}

						currentSection = new Fields();
						dictionary.Sections[name] = currentSection;
					},
					assignValue: (key, value) => currentSection.Values[key] = value);


				var reader = new StreamReader(stream, encoding);


				return dictionary;
			}
		}
	}
}