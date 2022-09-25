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
			Assert.AreEqual(0, dictionary.NumberOfFields);
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
			Assert.AreEqual(4, dictionary.NumberOfFields);
			Assert.AreEqual("Test Testsson", dictionary.GetField("name"));
			Assert.AreEqual("123", dictionary.GetField("age"));
			Assert.AreEqual("1.82", dictionary.GetField("height"));
			Assert.AreEqual("true", dictionary.GetField("amazing"));
		}
	}
}