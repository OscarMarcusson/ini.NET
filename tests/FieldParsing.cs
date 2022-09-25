using System.IO.Ini;

namespace tests
{
	[TestClass]
	public class FieldParsing
	{
		[TestMethod]
		public void DefaultValues()
		{
			var input = "".ToStream();
			var dictionary = IniDictionary.FromStream(input);
			Assert.IsNull(dictionary.GetField("test1"));
			Assert.AreEqual("ABC123", dictionary.GetField("test2", "ABC123"));
		}
	}
}