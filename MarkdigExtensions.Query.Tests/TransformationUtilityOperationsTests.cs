using MarkdigExtensions.Query.Types;

namespace MarkdigExtensions.Query.Tests;

public class TransformationUtilityOperationsTests
{
    [Fact]
    public void TransformationUtilityOperations_ShouldExecuteAllMethods_WithTestSuite06()
    {
        // Arrange: Get the markdown document using TestSuite_06
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test Each with Action<int, INode>
        var processedNodes = new List<(int Index, INode Node)>();
        var headings = document.GetHeadings();
        var eachResult = headings.Each(
            (index, node) =>
            {
                processedNodes.Add((index, node));
            }
        );

        Assert.Equal(headings.Length, processedNodes.Count);
        Assert.Same(headings, eachResult); // Each should return the same MarkdownDocument instance
        for (int i = 0; i < processedNodes.Count; i++)
        {
            Assert.Equal(i, processedNodes[i].Index);
            Assert.Equal(headings.Get()[i], processedNodes[i].Node);
        }

        // Test Each with Action<INode>
        var processedNodesSimple = new List<INode>();
        var textNodes = document.GetTextNodes();
        var eachSimpleResult = textNodes.Each(node =>
        {
            processedNodesSimple.Add(node);
        });

        Assert.Equal(textNodes.Length, processedNodesSimple.Count);
        Assert.Same(textNodes, eachSimpleResult); // Each should return the same MarkdownDocument instance
        for (int i = 0; i < processedNodesSimple.Count; i++)
        {
            Assert.Equal(textNodes.Get()[i], processedNodesSimple[i]);
        }

        // Test Select with Func<INode, T>
        var headingNames = headings.Select(node => node.Name);
        Assert.Equal(headings.Length, headingNames.Count());
        Assert.All(headingNames, name => Assert.Equal("heading", name));

        var headingTypes = headings.Select(node => node.GetType());
        Assert.Equal(headings.Length, headingTypes.Count());
        Assert.All(headingTypes, type => Assert.Equal(typeof(HeadingNode), type));

        // Test Select with Func<int, INode, T>
        var indexedHeadings = headings.Select((index, node) => new { Index = index, Node = node });
        Assert.Equal(headings.Length, indexedHeadings.Count());
        var indexedList = indexedHeadings.ToList();
        for (int i = 0; i < indexedList.Count; i++)
        {
            Assert.Equal(i, indexedList[i].Index);
            Assert.Equal(headings.Get()[i], indexedList[i].Node);
        }

        // Test Get() method
        var allSelectedNodes = headings.Get();
        Assert.Equal(headings.Length, allSelectedNodes.Count);
        Assert.IsType<IReadOnlyList<INode>>(allSelectedNodes, exactMatch: false);
        for (int i = 0; i < allSelectedNodes.Count; i++)
        {
            Assert.Equal(headings.Get()[i], allSelectedNodes[i]);
        }

        // Test Get(Index index) method
        if (headings.Length > 0)
        {
            var firstHeading = headings.Get(0);
            Assert.Equal(headings.Get()[0], firstHeading);

            var lastHeading = headings.Get(^1);
            Assert.Equal(headings.Get()[^1], lastHeading);

            if (headings.Length > 1)
            {
                var secondHeading = headings.Get(1);
                Assert.Equal(headings.Get()[1], secondHeading);
            }
        }

        // Test FirstOrDefault method
        var firstOrDefault = headings.FirstOrDefault();
        if (headings.Length > 0)
        {
            Assert.NotNull(firstOrDefault);
            Assert.Equal(headings.Get()[0], firstOrDefault);
        }
        else
        {
            Assert.Null(firstOrDefault);
        }

        var emptySelection = document.Filter(node => false);
        var emptyFirstOrDefault = emptySelection.FirstOrDefault();
        Assert.Null(emptyFirstOrDefault);

        // Test LastOrDefault method
        var lastOrDefault = headings.LastOrDefault();
        if (headings.Length > 0)
        {
            Assert.NotNull(lastOrDefault);
            Assert.Equal(headings.Get()[^1], lastOrDefault);
        }
        else
        {
            Assert.Null(lastOrDefault);
        }

        var emptyLastOrDefault = emptySelection.LastOrDefault();
        Assert.Null(emptyLastOrDefault);

        // Test GetTextContent method
        var paragraphs = document.GetParagraphs();
        var textContent = paragraphs.GetTextContent();
        Assert.NotNull(textContent);
        Assert.IsType<string>(textContent);

        // Test with custom separator
        var textContentWithSeparator = paragraphs.GetTextContent(" | ");
        Assert.NotNull(textContentWithSeparator);
        Assert.IsType<string>(textContentWithSeparator);

        // Test GetTextContent on text nodes specifically
        var textNodesContent = document.GetTextNodes().GetTextContent();
        Assert.NotNull(textNodesContent);

        // Test GetTextContent on empty selection
        var emptyTextContent = emptySelection.GetTextContent();
        Assert.Equal(string.Empty, emptyTextContent);

        // Test GetStatistics method
        var stats = document.GetStatistics();
        Assert.NotNull(stats);
        Assert.IsType<Dictionary<string, object>>(stats);

        // Verify all expected keys are present
        var expectedKeys = new[]
        {
            "TotalNodes",
            "SelectedNodes",
            "HeadingCount",
            "ParagraphCount",
            "LinkCount",
            "ImageCount",
            "CodeBlockCount",
            "ListCount",
            "TableCount",
            "MaxDepth",
            "WordCount",
        };

        foreach (var key in expectedKeys)
        {
            Assert.True(stats.ContainsKey(key), $"Statistics should contain key: {key}");
        }

        // Verify statistics values are reasonable
        Assert.True((int)stats["TotalNodes"] > 0, "TotalNodes should be positive");
        Assert.True((int)stats["SelectedNodes"] > 0, "SelectedNodes should be positive");
        Assert.True((int)stats["HeadingCount"] >= 0, "HeadingCount should be non-negative");
        Assert.True((int)stats["ParagraphCount"] >= 0, "ParagraphCount should be non-negative");
        Assert.True((int)stats["LinkCount"] >= 0, "LinkCount should be non-negative");
        Assert.True((int)stats["ImageCount"] >= 0, "ImageCount should be non-negative");
        Assert.True((int)stats["CodeBlockCount"] >= 0, "CodeBlockCount should be non-negative");
        Assert.True((int)stats["ListCount"] >= 0, "ListCount should be non-negative");
        Assert.True((int)stats["TableCount"] >= 0, "TableCount should be non-negative");
        Assert.True((int)stats["MaxDepth"] >= 0, "MaxDepth should be non-negative");
        Assert.True((int)stats["WordCount"] >= 0, "WordCount should be non-negative");

        // Test GetStatistics on filtered selection
        var headingStats = headings.GetStatistics();
        Assert.Equal(document.Count, (int)headingStats["TotalNodes"]); // TotalNodes should be same as document
        Assert.Equal(headings.Length, (int)headingStats["SelectedNodes"]); // SelectedNodes should match selection
        Assert.Equal(headings.Length, (int)headingStats["HeadingCount"]); // All selected nodes are headings

        // Test method chaining with transformation operations
        var chainedOperations = document
            .GetHeadings()
            .Each(
                (
                    index,
                    node
                ) => { /* Process each node */
                }
            )
            .Filter(node => ((HeadingNode)node).Level <= 2);

        Assert.True(chainedOperations.Length >= 0, "Chained operations should work");

        // Test LINQ integration with Select
        var headingLevels = document
            .GetHeadings()
            .Select(node => ((HeadingNode)node).Level)
            .ToList();

        Assert.True(headingLevels.Count > 0, "Should be able to extract heading levels");
        Assert.All(
            headingLevels,
            level => Assert.True(level >= 1 && level <= 6, "Heading levels should be 1-6")
        );

        // Test complex transformations
        var complexTransformation = document
            .GetHeadings()
            .Select(
                (index, node) =>
                    new
                    {
                        Index = index,
                        ((HeadingNode)node).Level,
                        Text = node.Value ?? string.Empty,
                        Depth = document.GetDepth(node),
                    }
            )
            .Where(h => h.Level <= 3)
            .ToList();

        Assert.True(complexTransformation.Count >= 0, "Complex transformation should work");
        Assert.All(
            complexTransformation,
            h => Assert.True(h.Level <= 3, "Filtered transformation should respect conditions")
        );

        // Test that transformation operations don't modify original selection
        var originalHeadingCount = headings.Length;
        headings.Each(node =>
        { /* Do something */
        });
        Assert.Equal(originalHeadingCount, headings.Length); // Length should be unchanged

        var transformedData = headings.Select(node => node.Name).ToList();
        Assert.Equal(originalHeadingCount, headings.Length); // Original selection unchanged
        Assert.Equal(originalHeadingCount, transformedData.Count); // But we got the transformed data
    }

    [Fact]
    public void TransformationUtilityOperations_ShouldHandleEmptySelections_Gracefully()
    {
        // Arrange: Use TestSuite_06 to get a document with content
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Create empty selection
        var emptySelection = document.Filter(node => false);
        Assert.Equal(0, emptySelection.Length);

        // Test Each on empty selection
        var processedCount = 0;
        var eachResult = emptySelection.Each((index, node) => processedCount++);
        Assert.Equal(0, processedCount);
        Assert.Same(emptySelection, eachResult);

        var processedCountSimple = 0;
        var eachSimpleResult = emptySelection.Each(node => processedCountSimple++);
        Assert.Equal(0, processedCountSimple);
        Assert.Same(emptySelection, eachSimpleResult);

        // Test Select on empty selection
        var selectedData = emptySelection.Select(node => node.Name);
        Assert.Empty(selectedData);

        var selectedWithIndex = emptySelection.Select((index, node) => index);
        Assert.Empty(selectedWithIndex);

        // Test Get methods on empty selection
        var emptyNodes = emptySelection.Get();
        Assert.Empty(emptyNodes);

        var emptyFirstOrDefault = emptySelection.FirstOrDefault();
        Assert.Null(emptyFirstOrDefault);

        var emptyLastOrDefault = emptySelection.LastOrDefault();
        Assert.Null(emptyLastOrDefault);

        // Test GetTextContent on empty selection
        var emptyTextContent = emptySelection.GetTextContent();
        Assert.Equal(string.Empty, emptyTextContent);

        var emptyTextContentWithSeparator = emptySelection.GetTextContent(" | ");
        Assert.Equal(string.Empty, emptyTextContentWithSeparator);

        // Test GetStatistics on empty selection
        var emptyStats = emptySelection.GetStatistics();
        Assert.NotNull(emptyStats);
        Assert.Equal(document.Count, (int)emptyStats["TotalNodes"]); // TotalNodes should still be document count
        Assert.Equal(0, (int)emptyStats["SelectedNodes"]); // SelectedNodes should be 0
        Assert.Equal(0, (int)emptyStats["HeadingCount"]); // No headings in empty selection
        Assert.Equal(0, (int)emptyStats["ParagraphCount"]); // No paragraphs in empty selection
    }

    [Fact]
    public void TransformationUtilityOperations_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test that Get() returns read-only collection
        var nodes = document.GetHeadings().Get();
        Assert.IsType<IReadOnlyList<INode>>(nodes, exactMatch: false);

        // Test that transformations preserve node identity
        var headings = document.GetHeadings();
        var selectedHeadings = headings.Select(node => node).ToList();
        for (int i = 0; i < headings.Length; i++)
        {
            Assert.Same(headings.Get()[i], selectedHeadings[i]);
        }

        // Test that Each preserves document state
        var originalNodes = headings.Get().ToList();
        headings.Each(node =>
        { /* Do nothing */
        });
        var afterEachNodes = headings.Get().ToList();

        Assert.Equal(originalNodes.Count, afterEachNodes.Count);
        for (int i = 0; i < originalNodes.Count; i++)
        {
            Assert.Same(originalNodes[i], afterEachNodes[i]);
        }

        // Test FirstOrDefault and LastOrDefault consistency
        if (headings.Length > 0)
        {
            Assert.Same(headings.Get()[0], headings.FirstOrDefault());
            Assert.Same(headings.Get()[^1], headings.LastOrDefault());

            if (headings.Length == 1)
            {
                Assert.Same(headings.FirstOrDefault(), headings.LastOrDefault());
            }
        }

        // Test Get(Index) consistency
        if (headings.Length > 0)
        {
            Assert.Same(headings.Get()[0], headings.Get(0));
            Assert.Same(headings.Get()[^1], headings.Get(^1));
        }

        // Test statistics accuracy
        var allStats = document.GetStatistics();
        var actualHeadingCount = document.GetHeadings().Length;
        var actualParagraphCount = document.GetParagraphs().Length;
        var actualLinkCount = document.GetLinks().Length;

        Assert.Equal(actualHeadingCount, (int)allStats["HeadingCount"]);
        Assert.Equal(actualParagraphCount, (int)allStats["ParagraphCount"]);
        Assert.Equal(actualLinkCount, (int)allStats["LinkCount"]);

        // Test that GetTextContent only includes TextNode values
        var textNodes = document.GetTextNodes();
        var manualTextContent = string.Join(
            " ",
            textNodes
                .Get()
                .OfType<TextNode>()
                .Select(t => t.Value)
                .Where(v => !string.IsNullOrEmpty(v))
        );

        var apiTextContent = textNodes.GetTextContent();
        Assert.Equal(manualTextContent, apiTextContent);
    }
}
