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
			readonly Dictionary<string, Action<object, string?>> Values = new Dictionary<string, Action<object, string?>>();


			public InstanceData(Type type)
			{
				Type = type;

				var properties = type.GetProperties().Where(x => x.CanWrite);
				foreach (var property in properties)
					AddSetter(property.Name, property.PropertyType, property.SetValue);

				var fields = type.GetFields().Where(x => x.IsPublic);
				foreach (var field in fields)
					AddSetter(field.Name, field.FieldType, field.SetValue);
			}

			void AddSetter(string key, Type type, Action<object, object> setter)
			{
				// TODO:: Pre-calculate the type of parser to use based on type, to ensure we only do it once
				Values.Add(key, (instance, stringValue) =>
				{
					setter(instance, ParseObject(stringValue, type));
				});
			}


			public void SetValue<TInstance>(string name, TInstance instance, string value)
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




		public static T FromString<T>(string ini) where T : new()
		{
			if (string.IsNullOrWhiteSpace(ini))
				return new T();

			using var reader = new StringReader(ini);
			return InstanceFromReader<T>(reader);
		}


		public static T FromStream<T>(Stream stream) where T : new() => FromStream<T>(stream, null);
		public static T FromStream<T>(Stream stream, Encoding? encoding) where T : new()
		{
			var reader = CreateReader(stream, encoding);
			return InstanceFromReader<T>(reader);
		}

		static T InstanceFromReader<T>(TextReader reader) where T : new()
		{
			var instance = new T();
			var currentInstance = instance as object;
			var setter = GetInstanceSetter(instance.GetType());
			var currentSetter = setter;

			ParseReader(
				reader, 
				parseSection: name =>
				{
					var property = instance.GetType().GetProperties().FirstOrDefault(x => x.Name == name);
					if (property != null)
					{
						ResolveSection(property.PropertyType);
						return;
					}

					var field = instance.GetType().GetFields().FirstOrDefault(x => x.Name == name);
					if(field != null)
					{
						ResolveSection(field.FieldType);
						return;
					}

					throw new NotSupportedException($"Could not create a section for \"{name}\", no such field or property exists");


					void ResolveSection(Type type)
					{
						if (!type.IsClass)
							throw new NotSupportedException($"Could not create a section for \"{name}\" since the \"{type}\" type is not a class");

						if (!type.GetConstructors().Any(x => x.GetParameters().Length == 0))
							throw new NotSupportedException($"Could not create a section for \"{name}\" since the \"{type}\" type does not contain a parameterless constructor");

						currentInstance = Activator.CreateInstance(type);
						SetValue(instance, name, currentInstance);

						currentSetter = GetInstanceSetter(type);
					}
				},
				assignValue: (key, value) =>
				{
					currentSetter.SetValue(key, currentInstance, value);
				});

			return instance;
		}

		static void SetValue(object instance, string name, object? value)
		{
			var property = instance.GetType().GetProperties().FirstOrDefault(x => x.Name == name);
			if (property != null)
			{
				property.SetValue(instance, value);
				return;
			}

			var field = instance.GetType().GetFields().FirstOrDefault(x => x.Name == name);
			if (field != null)
			{
				field.SetValue(instance, value);
				return;
			}
		}

		static InstanceData GetInstanceSetter(Type type)
		{
			if (!InstanceSetters.TryGetValue(type, out var setter))
				InstanceSetters[type] = setter = new InstanceData(type);
			return setter;
		}

		static TextReader CreateReader(Stream stream, Encoding? encoding)
			=> encoding == null
					? new StreamReader(stream, detectEncodingFromByteOrderMarks: true)
					: new StreamReader(stream, encoding)
					;


		static void ParseReader(TextReader reader, Action<string> parseSection, Action<string,string> assignValue)
		{
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

				return (T)Convert.ChangeType(stringValue, typeof(T));
			}
			catch (Exception)
			{
				return defaultValue;
			}
		}
		
		static object ParseObject(string stringValue, Type type)
		{
			try
			{
				if (Nullable.GetUnderlyingType(type) != null)
				{
					var conv = TypeDescriptor.GetConverter(type);
					return conv.ConvertFrom(stringValue);
				}

				return Convert.ChangeType(stringValue, type);
			}
			catch (Exception)
			{
				return type.IsValueType
					? Activator.CreateInstance(type)
					: null
					;
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



			// Public functions for creating the dictionaries
			public static Dictionary FromString(string ini)
			{
				if (string.IsNullOrWhiteSpace(ini))
					return new Dictionary();

				using var reader = new StringReader(ini);
				return FromReader(reader);
			}
			
			public static Dictionary FromFile(string path, Encoding? encoding = null)
			{
				if (path == null)
					throw new ArgumentNullException(nameof(path));

				if (string.IsNullOrWhiteSpace(path))
					throw new ArgumentException("Expected a path, but input string was empty", nameof(path));
				
				if (!File.Exists(path))
					throw new FileNotFoundException(path);

				using var reader = File.OpenRead(path);
				return FromStream(reader, encoding);
			}

			public static Dictionary FromStream(Stream stream, Encoding? encoding = null)
			{
				var reader = CreateReader(stream, encoding);
				return FromReader(reader);
			}
			
			static Dictionary FromReader(TextReader reader)
			{
				var dictionary = new Dictionary();
				var currentSection = dictionary as Fields;
				ParseReader(
					reader,
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

				return dictionary;
			}
		}
	}
}