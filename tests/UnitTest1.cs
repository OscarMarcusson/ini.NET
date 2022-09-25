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

		[TestMethod]
		public void StreamWithFields()
		{
			var ini = @"
			          name = Test Testsson
			          age = 123
			          height = 1.82
			          amazing = true
			          ";
			var input = ini.ToStream();
			var dictionary = IniDictionary.FromStream(input);
			Assert.IsFalse(dictionary.IsEmpty);
		}
	}
}