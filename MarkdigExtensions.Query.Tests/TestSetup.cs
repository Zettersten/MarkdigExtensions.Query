namespace MarkdigExtensions.Query.Tests;

public static class TestSetup
{
    private static readonly Dictionary<string, string> cache = [];

    private static string GetSampleData(string fileName)
    {
        if (cache.TryGetValue(fileName, out var cachedResult))
        {
            return cachedResult;
        }

        var assembly = typeof(TestSetup).Assembly;

        using var stream =
            assembly.GetManifestResourceStream($"MarkdigExtensions.Query.Tests.Samples.{fileName}")
            ?? throw new InvalidOperationException($"Resource '{fileName}' not found.");

        using var reader = new StreamReader(stream);
        var rawResult = reader.ReadToEnd();

        cache[fileName] = rawResult;

        return rawResult;
    }

    public static string TestSuite_01 => GetSampleData("test-suite-01.md");

    public static string TestSuite_02 => GetSampleData("test-suite-02.md");

    public static string TestSuite_03 => GetSampleData("test-suite-03.md");

    public static string TestSuite_04 => GetSampleData("test-suite-04.md");

    public static string TestSuite_05 => GetSampleData("test-suite-05.md");

    public static string TestSuite_06 => GetSampleData("test-suite-06.md");
}
