using System.IO;

namespace tests
{
	[TestClass]
	public class Strings
	{
		[TestMethod]
		public void Empty()
		{
			var dictionary = Ini.Dictionary.FromString("");
			Assert.IsTrue(dictionary.IsEmpty);
			Assert.AreEqual(0, dictionary.NumberOfFields);
		}
		
		[TestMethod]
		public void Fields()
		{
			var dictionary = Ini.Dictionary.FromString(TestData.RootVariables);
			Assert.IsFalse(dictionary.IsEmpty);
			Assert.AreEqual(4, dictionary.NumberOfFields);
			Assert.AreEqual("Test Testsson", dictionary.GetField("name"));
			Assert.AreEqual("123", dictionary.GetField("age"));
			Assert.AreEqual("1.82", dictionary.GetField("height"));
			Assert.AreEqual("true", dictionary.GetField("amazing"));
		}

		[TestMethod]
		public void Sections()
		{
			var dictionary = Ini.Dictionary.FromString(TestData.RootVariablesWithSections);
			Assert.IsFalse(dictionary.IsEmpty);
			Assert.AreEqual(1, dictionary.NumberOfFields);
			Assert.AreEqual("Test Testsson", dictionary.GetField("name"));
			Assert.AreEqual(2, dictionary.NumberOfSections);

			var info = dictionary.GetSection("info1");
			Assert.IsNotNull(info);
			Assert.IsFalse(info.IsEmpty);
			Assert.AreEqual(3, info.NumberOfFields);
			Assert.AreEqual("123", info.GetField("age"));
			Assert.AreEqual("1.82", info.GetField("height"));
			Assert.AreEqual("true", info.GetField("amazing"));

			info = dictionary.GetSection("info2");
			Assert.IsNotNull(info);
			Assert.IsFalse(info.IsEmpty);
			Assert.AreEqual(3, info.NumberOfFields);
			Assert.AreEqual("765", info.GetField("age"));
			Assert.AreEqual("0.78", info.GetField("height"));
			Assert.AreEqual("false", info.GetField("amazing"));
		}
	}
}