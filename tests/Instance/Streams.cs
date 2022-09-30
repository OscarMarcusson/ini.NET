namespace tests.Instance
{
	[TestClass]
	public class Streams
	{
		[TestMethod]
		public void EmptyStream()
		{
			var input = "".ToStream();
			var data = Ini.FromStream<TestData>(input);
			Assert.IsNotNull(data);
			Assert.AreEqual(default(string), data.name);
			Assert.AreEqual(default(int), data.age);
			Assert.AreEqual(default(decimal), data.height);
			Assert.AreEqual(default(bool), data.Amazing);
		}

		[TestMethod]
		public void StreamWithFields()
		{
			var input = TestData.RootVariables.ToStream();
			var data = Ini.FromStream<TestData>(input);
			Assert.IsNotNull(data);
			Assert.AreEqual("Test Testsson", data.name);
			Assert.AreEqual(123, data.age);
			Assert.AreEqual(1.82m, data.height);
			Assert.AreEqual(true, data.Amazing);
		}

		[TestMethod]
		public void StreamWithSections()
		{
			var input = TestData.RootVariablesWithSections.ToStream();
			var data = Ini.FromStream<TestData>(input);
			Assert.IsNotNull(data);
			Assert.AreEqual("Test Testsson", data.name);
			Assert.AreEqual(default(int), data.age);
			Assert.AreEqual(default(decimal), data.height);
			Assert.AreEqual(default(bool), data.Amazing);
			
			Assert.IsNotNull(data.info1);
			Assert.AreEqual(123, data.info1.age);
			Assert.AreEqual(1.82m, data.info1.height);
			Assert.AreEqual(true, data.info1.Amazing);

			Assert.IsNotNull(data.info1);
			Assert.AreEqual(765, data.info1.age);
			Assert.AreEqual(0.78m, data.info1.height);
			Assert.AreEqual(false, data.info1.Amazing);
		}
	}
}