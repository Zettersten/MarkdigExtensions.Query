using MarkdigExtensions.Query.Types;

namespace MarkdigExtensions.Query.Tests;

public class BasicTests
{
    [Fact]
    public void BasicTests_TestSuite01_ShouldParseAndAnalyzeCorrectly()
    {
        // Arrange
        var markdown = TestSetup.TestSuite_01;
        var document = markdown.AsQueryable();

        // Basic document structure validation
        Assert.True(document.Count > 0, "Document should have nodes");
        Assert.True(document.Length > 0, "Document should have selected nodes");

        // Test headings count and content
        var headings = document.GetHeadings();
        Assert.True(headings.Length > 0, "Should have headings");

        var h1Headings = document.GetHeadings(1);
        Assert.True(h1Headings.Length > 0, "Should have H1 headings");

        var firstH1 = h1Headings.Get()[0] as HeadingNode;
        Assert.NotNull(firstH1);
        Assert.Contains("Welcome", firstH1.Value ?? "");

        // Test links
        var links = document.GetLinks();
        Assert.True(links.Length > 0, "Should have links");

        // Test images
        var images = document.GetImages();
        Assert.True(images.Length > 0, "Should have images");

        // Test paragraphs
        var paragraphs = document.GetParagraphs();
        Assert.True(paragraphs.Length > 0, "Should have paragraphs");

        // Test lists
        var lists = document.GetLists();
        Assert.True(lists.Length > 0, "Should have lists");

        // Test code blocks
        var codeBlocks = document.GetCodeBlocks();
        Assert.True(codeBlocks.Length > 0, "Should have code blocks");

        // Test tables
        var tables = document.GetTables();
        Assert.True(tables.Length > 0, "Should have tables");

        // Test text content extraction
        var textContent = document.GetTextContent();
        Assert.NotNull(textContent);
        Assert.Contains("Welcome", textContent);
        Assert.Contains("Test Suite", textContent);

        // Test document statistics
        var stats = document.GetStatistics();
        Assert.NotNull(stats);
        Assert.True((int)stats["TotalNodes"] > 0);
        Assert.True((int)stats["HeadingCount"] > 0);
        Assert.True((int)stats["ParagraphCount"] > 0);
        Assert.True((int)stats["LinkCount"] > 0);
        Assert.True((int)stats["ImageCount"] > 0);
        Assert.True((int)stats["CodeBlockCount"] > 0);
        Assert.True((int)stats["TableCount"] > 0);

        // Test document outline
        var outline = document.GetDocumentOutline();
        Assert.NotNull(outline);
        Assert.True(outline.Count > 0);
        Assert.Contains(outline, item => item.Title.Contains("Welcome"));

        // Test link analysis
        var linkAnalysis = document.AnalyzeLinks();
        Assert.NotNull(linkAnalysis);
        Assert.True(linkAnalysis.Count > 0);
        Assert.Contains(linkAnalysis, link => link.IsExternal);
    }

    [Fact]
    public void BasicTests_TestSuite02_ShouldParseAndAnalyzeCorrectly()
    {
        // Arrange
        var markdown = TestSetup.TestSuite_02;
        var document = markdown.AsQueryable();

        // Basic validation
        Assert.True(document.Count > 0, "Document should have nodes");

        // Test specific content elements
        var textContent = document.GetTextContent();
        Assert.Contains("Robot", textContent);
        Assert.Contains("Release Date", textContent);

        // Test paragraphs and text structure
        var paragraphs = document.GetParagraphs();
        Assert.True(paragraphs.Length > 0, "Should have paragraphs");

        // Test lists (should have media links)
        var lists = document.GetLists();
        Assert.True(lists.Length > 0, "Should have lists");

        // Test links
        var links = document.GetLinks();
        Assert.True(links.Length > 0, "Should have links");

        // Verify link analysis
        var linkAnalysis = document.AnalyzeLinks();
        Assert.True(linkAnalysis.Count > 0);
        Assert.Contains(linkAnalysis, link => link.Url.Contains("drive.google.com"));

        // Test statistics
        var stats = document.GetStatistics();
        Assert.True((int)stats["TotalNodes"] > 0);
        Assert.True((int)stats["ParagraphCount"] > 0);
        Assert.True((int)stats["LinkCount"] > 0);
    }

    [Fact]
    public void BasicTests_TestSuite03_ShouldParseAndAnalyzeCorrectly()
    {
        // Arrange
        var markdown = TestSetup.TestSuite_03;
        var document = markdown.AsQueryable();

        // Basic validation
        Assert.True(document.Count > 0, "Document should have nodes");

        // Test document structure
        var stats = document.GetStatistics();
        Assert.NotNull(stats);
        Assert.True((int)stats["TotalNodes"] > 0);

        // Test text extraction
        var textContent = document.GetTextContent();
        Assert.NotNull(textContent);
        Assert.NotEmpty(textContent);

        // Test outline generation
        var outline = document.GetDocumentOutline();
        Assert.NotNull(outline);
    }

    [Fact]
    public void BasicTests_TestSuite04_ShouldParseAndAnalyzeCorrectly()
    {
        // Arrange
        var markdown = TestSetup.TestSuite_04;
        var document = markdown.AsQueryable();

        // Basic validation
        Assert.True(document.Count > 0, "Document should have nodes");

        // Test structure
        var stats = document.GetStatistics();
        Assert.NotNull(stats);
        Assert.True((int)stats["TotalNodes"] > 0);

        // Test content extraction
        var textContent = document.GetTextContent();
        Assert.NotNull(textContent);

        // Test outline
        var outline = document.GetDocumentOutline();
        Assert.NotNull(outline);

        // Test link analysis if links exist
        var links = document.GetLinks();
        if (links.Length > 0)
        {
            var linkAnalysis = document.AnalyzeLinks();
            Assert.NotNull(linkAnalysis);
        }
    }

    [Fact]
    public void BasicTests_TestSuite05_ShouldParseAndAnalyzeCorrectly()
    {
        // Arrange
        var markdown = TestSetup.TestSuite_05;
        var document = markdown.AsQueryable();

        // Basic validation
        Assert.True(document.Count > 0, "Document should have nodes");

        // Test structure and statistics
        var stats = document.GetStatistics();
        Assert.NotNull(stats);
        Assert.True((int)stats["TotalNodes"] > 0);

        // Test text content
        var textContent = document.GetTextContent();
        Assert.NotNull(textContent);

        // Test outline generation
        var outline = document.GetDocumentOutline();
        Assert.NotNull(outline);

        // Test various element types if they exist
        var headings = document.GetHeadings();
        var paragraphs = document.GetParagraphs();
        var links = document.GetLinks();
        var images = document.GetImages();
        var lists = document.GetLists();
        var codeBlocks = document.GetCodeBlocks();
        var tables = document.GetTables();

        // Verify statistics match actual counts
        Assert.Equal(headings.Length, (int)stats["HeadingCount"]);
        Assert.Equal(paragraphs.Length, (int)stats["ParagraphCount"]);
        Assert.Equal(links.Length, (int)stats["LinkCount"]);
        Assert.Equal(images.Length, (int)stats["ImageCount"]);
        Assert.Equal(lists.Length, (int)stats["ListCount"]);
        Assert.Equal(codeBlocks.Length, (int)stats["CodeBlockCount"]);
        Assert.Equal(tables.Length, (int)stats["TableCount"]);
    }

    [Fact]
    public void BasicTests_TestSuite06_ShouldParseAndAnalyzeCorrectly()
    {
        // Arrange
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Basic validation
        Assert.True(document.Count > 0, "Document should have nodes");

        // Test comprehensive structure
        var stats = document.GetStatistics();
        Assert.NotNull(stats);
        Assert.True((int)stats["TotalNodes"] > 0);
        Assert.True((int)stats["SelectedNodes"] > 0);
        Assert.Equal(document.Count, (int)stats["TotalNodes"]);
        Assert.Equal(document.Length, (int)stats["SelectedNodes"]);

        // Test text content extraction
        var textContent = document.GetTextContent();
        Assert.NotNull(textContent);
        Assert.NotEmpty(textContent);

        // Test text content with custom separator
        var textContentCustomSeparator = document.GetTextContent(" | ");
        Assert.NotNull(textContentCustomSeparator);
        Assert.NotEmpty(textContentCustomSeparator);

        // Test document outline
        var outline = document.GetDocumentOutline();
        Assert.NotNull(outline);

        // Verify outline structure if headings exist
        var headings = document.GetHeadings();
        if (headings.Length > 0)
        {
            Assert.True(outline.Count > 0);
            // Verify outline items have required properties
            foreach (var item in outline)
            {
                Assert.True(item.Level >= 1 && item.Level <= 6);
                Assert.NotNull(item.Title);
                Assert.NotNull(item.Position);
            }
        }

        // Test link analysis
        var links = document.GetLinks();
        if (links.Length > 0)
        {
            var linkAnalysis = document.AnalyzeLinks();
            Assert.NotNull(linkAnalysis);
            Assert.Equal(links.Length, linkAnalysis.Count);

            // Verify link analysis properties
            foreach (var linkInfo in linkAnalysis)
            {
                Assert.NotNull(linkInfo.Url);
                Assert.NotNull(linkInfo.Text);
                Assert.NotNull(linkInfo.Position);
                // IsExternal should be correctly determined
                if (linkInfo.Url.StartsWith("http://") || linkInfo.Url.StartsWith("https://"))
                {
                    Assert.True(linkInfo.IsExternal);
                }
                else
                {
                    Assert.False(linkInfo.IsExternal);
                }
            }
        }

        // Test element type consistency
        var paragraphs = document.GetParagraphs();
        var images = document.GetImages();
        var lists = document.GetLists();
        var codeBlocks = document.GetCodeBlocks();
        var tables = document.GetTables();

        // Verify statistics accuracy
        Assert.Equal(headings.Length, (int)stats["HeadingCount"]);
        Assert.Equal(paragraphs.Length, (int)stats["ParagraphCount"]);
        Assert.Equal(links.Length, (int)stats["LinkCount"]);
        Assert.Equal(images.Length, (int)stats["ImageCount"]);
        Assert.Equal(lists.Length, (int)stats["ListCount"]);
        Assert.Equal(codeBlocks.Length, (int)stats["CodeBlockCount"]);
        Assert.Equal(tables.Length, (int)stats["TableCount"]);

        // Test max depth calculation
        var maxDepth = (int)stats["MaxDepth"];
        Assert.True(maxDepth >= 0);
        var actualMaxDepth = document.AllNodes.Select(document.GetDepth).Max();
        Assert.Equal(actualMaxDepth, maxDepth);

        // Test word count
        var wordCount = (int)stats["WordCount"];
        Assert.True(wordCount >= 0);
    }

    [Fact]
    public void BasicTests_AllTestSuites_ShouldHaveConsistentBehavior()
    {
        var testSuites = new[]
        {
            TestSetup.TestSuite_01,
            TestSetup.TestSuite_02,
            TestSetup.TestSuite_03,
            TestSetup.TestSuite_04,
            TestSetup.TestSuite_05,
            TestSetup.TestSuite_06,
        };

        foreach (var markdown in testSuites)
        {
            var document = markdown.AsQueryable();

            // Basic consistency checks
            Assert.True(document.Count > 0, "Every test suite should have nodes");
            Assert.NotNull(document.Root);
            Assert.NotNull(document.AllNodes);
            Assert.NotNull(document.SelectedNodes);

            // Statistics should always be valid
            var stats = document.GetStatistics();
            Assert.NotNull(stats);
            Assert.True((int)stats["TotalNodes"] > 0);
            Assert.True((int)stats["SelectedNodes"] > 0);
            Assert.True((int)stats["MaxDepth"] >= 0);
            Assert.True((int)stats["WordCount"] >= 0);

            // All element counts should be non-negative
            Assert.True((int)stats["HeadingCount"] >= 0);
            Assert.True((int)stats["ParagraphCount"] >= 0);
            Assert.True((int)stats["LinkCount"] >= 0);
            Assert.True((int)stats["ImageCount"] >= 0);
            Assert.True((int)stats["ListCount"] >= 0);
            Assert.True((int)stats["CodeBlockCount"] >= 0);
            Assert.True((int)stats["TableCount"] >= 0);

            // Text content should never be null
            var textContent = document.GetTextContent();
            Assert.NotNull(textContent);

            // Outline should never be null
            var outline = document.GetDocumentOutline();
            Assert.NotNull(outline);

            // Link analysis should never be null
            var linkAnalysis = document.AnalyzeLinks();
            Assert.NotNull(linkAnalysis);

            // Extension methods should work
            var documentStats = document.GetDocumentStatistics();
            Assert.NotNull(documentStats);
            Assert.Equal(stats["TotalNodes"], documentStats["TotalNodes"]);
        }
    }

    [Fact]
    public void BasicTests_DocumentConversion_ShouldWorkWithBothMethods()
    {
        var markdown = TestSetup.TestSuite_01;

        // Test string extension method
        var document1 = markdown.AsQueryable();

        // Test AsQueryable extension method
        var document2 = markdown.AsQueryable();

        // Both should produce equivalent results
        Assert.Equal(document1.Count, document2.Count);
        Assert.Equal(document1.Length, document2.Length);

        var stats1 = document1.GetStatistics();
        var stats2 = document2.GetStatistics();

        Assert.Equal(stats1["TotalNodes"], stats2["TotalNodes"]);
        Assert.Equal(stats1["HeadingCount"], stats2["HeadingCount"]);
        Assert.Equal(stats1["ParagraphCount"], stats2["ParagraphCount"]);
        Assert.Equal(stats1["LinkCount"], stats2["LinkCount"]);
    }
}
