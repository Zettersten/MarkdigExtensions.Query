namespace Markdig.Query.Tests;

public static class TestSetup
{
    private static readonly Dictionary<string, string> _cache = [];

    private static string GetSampleData(string fileName)
    {
        if (_cache.TryGetValue(fileName, out var cachedResult))
        {
            return cachedResult;
        }

        var assembly = typeof(TestSetup).Assembly;

        using var stream =
            assembly.GetManifestResourceStream($"Markdig.Query.Tests.Samples.{fileName}")
            ?? throw new InvalidOperationException($"Resource '{fileName}' not found.");

        using var reader = new StreamReader(stream);
        var rawResult = reader.ReadToEnd();

        _cache[fileName] = rawResult;

        return rawResult;
    }

    public static string TestSuite_01 => GetSampleData("test-suite-01.md");
}
