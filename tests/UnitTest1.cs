using System.IO.Ini;

namespace tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void EmptyStream()
		{
			var input = "".ToStream();
			var dictionary = IniDictionary.FromStream(input);
			Assert.IsTrue(dictionary.IsEmpty);
		}
	}
}