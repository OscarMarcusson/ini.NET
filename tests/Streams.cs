using System.IO;

namespace tests
{
	[TestClass]
	public class Streams
	{
		[TestMethod]
		public void EmptyStream()
		{
			var input = "".ToStream();
			var dictionary = Ini.Dictionary.FromStream(input);
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
			var dictionary = Ini.Dictionary.FromStream(input);
			Assert.IsFalse(dictionary.IsEmpty);
			Assert.AreEqual(4, dictionary.NumberOfFields);
			Assert.AreEqual("Test Testsson", dictionary.GetField("name"));
			Assert.AreEqual("123", dictionary.GetField("age"));
			Assert.AreEqual("1.82", dictionary.GetField("height"));
			Assert.AreEqual("true", dictionary.GetField("amazing"));
		}

		[TestMethod]
		public void StreamWithSections()
		{
			var ini = @"
			          name = Test Testsson

			          [info1]
			          age = 123
			          height = 1.82
			          amazing = true

			          [info2]
			          age = 765
			          height = 0.78
			          amazing = false
			          ";
			var input = ini.ToStream();
			var dictionary = Ini.Dictionary.FromStream(input);
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