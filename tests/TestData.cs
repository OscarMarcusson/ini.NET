namespace tests;

public static class TestData
{
	public const string RootVariables = @"
		name = Test Testsson
		age = 123
		height = 1.82
		amazing = true
		";

	public const string RootVariablesWithSections = @"
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
}