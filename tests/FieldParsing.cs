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

		[TestMethod]
		public void Booleans()
		{
			var ini = @"
			          true = true
			          false = false
			          ";
			var input = ini.ToStream();
			var dictionary = IniDictionary.FromStream(input);
			Assert.IsTrue(dictionary.GetField<bool?>("true", defaultValue: null));
			Assert.IsFalse(dictionary.GetField<bool?>("false", defaultValue: null));
		}

		[TestMethod]
		public void Integers()
		{
			var ini = @"
			          byte = 128
			          short = 16523
			          int = 7534354
			          long = 45254343235
			          ";
			var input = ini.ToStream();
			var dictionary = IniDictionary.FromStream(input);
			Assert.AreEqual(128, dictionary.GetField<byte>("byte"));
			Assert.AreEqual(16523, dictionary.GetField<short>("short"));
			Assert.AreEqual(7534354, dictionary.GetField<int>("int"));
			Assert.AreEqual(45254343235, dictionary.GetField<long>("long"));
		}

		[TestMethod]
		public void Floats()
		{
			var ini = @"
			          float = 0.3465
			          double = 0.134728347
			          decimal = 0.234723573923
			          ";
			var input = ini.ToStream();
			var dictionary = IniDictionary.FromStream(input);
			Assert.AreEqual(0.3465f, dictionary.GetField<float>("float"));
			Assert.AreEqual(0.134728347, dictionary.GetField<double>("double"));
			Assert.AreEqual(0.234723573923m, dictionary.GetField<decimal>("decimal"));
		}
	}
}