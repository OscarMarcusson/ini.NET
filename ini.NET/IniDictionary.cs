using System.Text;

namespace ini.NET
{
	public class IniDictionary
	{
		public bool IsEmpty { get; private set; }


		private IniDictionary() => IsEmpty = true;



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
					Console.WriteLine(line.Substring(startIndex));
				}
			}

			var result = reader.ReadToEnd();

			if (string.IsNullOrWhiteSpace(result))
				return new IniDictionary();

			return dictionary;
		}



		// Helper methods
		static bool FindStartOfContent(string line, out int index)
		{
			for (index = 0; index < line.Length; index++)
			{
				if (line[index] != ' ' && line[index] != '\t')
					return true;
			}

			index = -1;
			return false;
		}
	}
}