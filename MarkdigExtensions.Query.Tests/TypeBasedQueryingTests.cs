using MarkdigExtensions.Query.Types;

namespace MarkdigExtensions.Query.Tests;

public class TypeBasedQueryingTests
{
    [Fact]
    public void TypeBasedQuerying_ShouldExecuteAllMethods_WithTestSuite06()
    {
        // Arrange: Get the markdown document using TestSuite_06
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test GetNodes<T> with various types and predicates
        var allHeadings = document.GetNodes<HeadingNode>();
        Assert.True(allHeadings.Length > 0, "Should find heading nodes");

        var h1Headings = document.GetNodes<HeadingNode>(h => h.Level == 1);
        Assert.True(h1Headings.Length > 0, "Should find H1 heading nodes");

        var allParagraphs = document.GetNodes<ParagraphNode>();
        Assert.True(allParagraphs.Length > 0, "Should find paragraph nodes");

        var allTextNodes = document.GetNodes<TextNode>();
        Assert.True(allTextNodes.Length > 0, "Should find text nodes");

        var textNodesWithContent = document.GetNodes<TextNode>(t =>
            !string.IsNullOrWhiteSpace(t.Value)
        );
        Assert.True(textNodesWithContent.Length > 0, "Should find text nodes with content");

        // Test GetNodesByType<T> with enumerable return
        var headingsEnum = document.GetNodesByType<HeadingNode>();
        Assert.True(headingsEnum.Any(), "Should return heading nodes as enumerable");

        var h2HeadingsEnum = document.GetNodesByType<HeadingNode>(h => h.Level == 2);
        Assert.True(h2HeadingsEnum.Any(), "Should return filtered H2 heading nodes as enumerable");

        var paragraphsEnum = document.GetNodesByType<ParagraphNode>();
        Assert.True(paragraphsEnum.Any(), "Should return paragraph nodes as enumerable");

        var codeBlocksEnum = document.GetNodesByType<CodeBlockNode>();
        Assert.True(codeBlocksEnum.Any(), "Should return code block nodes as enumerable");

        // Test GetHeadings with optional level filtering
        var allHeadingsMethod = document.GetHeadings();
        Assert.True(allHeadingsMethod.Length > 0, "GetHeadings should find heading nodes");

        var h1HeadingsMethod = document.GetHeadings(1);
        Assert.True(h1HeadingsMethod.Length > 0, "GetHeadings(1) should find H1 nodes");

        var h2HeadingsMethod = document.GetHeadings(2);
        Assert.True(h2HeadingsMethod.Length > 0, "GetHeadings(2) should find H2 nodes");

        var h3HeadingsMethod = document.GetHeadings(3);
        Assert.True(h3HeadingsMethod.Length > 0, "GetHeadings(3) should find H3 nodes");

        // Test non-existent heading level
        var h6HeadingsMethod = document.GetHeadings(6);
        Assert.Equal(0, h6HeadingsMethod.Length);

        // Test GetLinks
        var links = document.GetLinks();
        Assert.True(links.Length > 0, "Should find link nodes");

        // Verify links are actually LinkNode instances
        var linkNodes = links.Get();
        Assert.All(linkNodes, node => Assert.IsType<LinkNode>(node));

        // Test GetImages
        var images = document.GetImages();
        Assert.True(images.Length > 0, "Should find image nodes");

        // Verify images are actually ImageNode instances
        var imageNodes = images.Get();
        Assert.All(imageNodes, node => Assert.IsType<ImageNode>(node));

        // Test GetTextNodes
        var textNodes = document.GetTextNodes();
        Assert.True(textNodes.Length > 0, "Should find text nodes");

        // Verify text nodes are actually TextNode instances
        var textNodeInstances = textNodes.Get();
        Assert.All(textNodeInstances, node => Assert.IsType<TextNode>(node));

        // Test GetParagraphs
        var paragraphs = document.GetParagraphs();
        Assert.True(paragraphs.Length > 0, "Should find paragraph nodes");

        // Verify paragraphs are actually ParagraphNode instances
        var paragraphNodes = paragraphs.Get();
        Assert.All(paragraphNodes, node => Assert.IsType<ParagraphNode>(node));

        // Test GetCodeBlocks
        var codeBlocks = document.GetCodeBlocks();
        Assert.True(codeBlocks.Length > 0, "Should find code block nodes");

        // Verify code blocks are actually CodeBlockNode instances
        var codeBlockNodes = codeBlocks.Get();
        Assert.All(codeBlockNodes, node => Assert.IsType<CodeBlockNode>(node));

        // Test GetLists
        var lists = document.GetLists();
        Assert.True(lists.Length > 0, "Should find list nodes");

        // Verify lists are actually ListNode instances
        var listNodes = lists.Get();
        Assert.All(listNodes, node => Assert.IsType<ListNode>(node));

        // Test GetTables
        var tables = document.GetTables();
        Assert.True(tables.Length > 0, "Should find table nodes");

        // Verify tables are actually TableNode instances
        var tableNodes = tables.Get();
        Assert.All(tableNodes, node => Assert.IsType<TableNode>(node));

        // Test method chaining and filtering
        var headingTexts = document
            .GetHeadings()
            .GetTextNodes()
            .Select(n => ((TextNode)n).Value)
            .Where(v => !string.IsNullOrWhiteSpace(v));

        Assert.True(
            headingTexts.Any(),
            "Should be able to chain methods and get heading text content"
        );

        // Test that results are consistent between different approaches
        var headingsFromGeneric = document.GetNodes<HeadingNode>().Length;
        var headingsFromSpecific = document.GetHeadings().Length;
        Assert.Equal(headingsFromGeneric, headingsFromSpecific);

        var paragraphsFromGeneric = document.GetNodes<ParagraphNode>().Length;
        var paragraphsFromSpecific = document.GetParagraphs().Length;
        Assert.Equal(paragraphsFromGeneric, paragraphsFromSpecific);

        // Test with complex predicate filtering
        var nonEmptyTextNodes = document.GetNodes<TextNode>(t =>
            !string.IsNullOrWhiteSpace(t.Value) && t.Value.Length > 5
        );
        Assert.True(
            nonEmptyTextNodes.Length > 0,
            "Should find text nodes with substantial content"
        );

        // Test that filtered results are subsets
        Assert.True(
            h1Headings.Length <= allHeadings.Length,
            "Filtered headings should be subset of all headings"
        );
        Assert.True(
            textNodesWithContent.Length <= allTextNodes.Length,
            "Filtered text nodes should be subset of all text nodes"
        );

        // Test enumerable vs MarkdownDocument return types
        var enumerableCount = document.GetNodesByType<LinkNode>().Count();
        var documentCount = document.GetLinks().Length;
        Assert.Equal(enumerableCount, documentCount);
    }

    [Fact]
    public void TypeBasedQuerying_ShouldHandleEmptyResults_Gracefully()
    {
        // Arrange: Create a simple document with limited content
        var markdown = "# Just a heading\n\nSimple paragraph.";
        var document = markdown.AsQueryable();

        // Test methods that should return empty results
        var tables = document.GetTables();
        Assert.Equal(0, tables.Length);

        var images = document.GetImages();
        Assert.Equal(0, images.Length);

        var codeBlocks = document.GetCodeBlocks();
        Assert.Equal(0, codeBlocks.Length);

        var lists = document.GetLists();
        Assert.Equal(0, lists.Length);

        // Test with predicate that matches nothing
        var impossibleHeadings = document.GetNodes<HeadingNode>(h => h.Level > 10);
        Assert.Equal(0, impossibleHeadings.Length);

        var impossibleTextNodes = document.GetNodesByType<TextNode>(t =>
            t.Value == "impossible_text_12345"
        );
        Assert.Empty(impossibleTextNodes);

        // Test heading level filtering with non-existent levels
        var h5Headings = document.GetHeadings(5);
        Assert.Equal(0, h5Headings.Length);

        // Verify methods don't throw on empty results - test by accessing the results
        var emptyTables = document.GetTables().Get();
        Assert.Empty(emptyTables);

        var emptyImages = document.GetImages().Get();
        Assert.Empty(emptyImages);

        var emptyCodeBlocks = document.GetCodeBlocks().Get();
        Assert.Empty(emptyCodeBlocks);
    }

    [Fact]
    public void TypeBasedQuerying_ShouldMaintainDocumentStructure()
    {
        // Arrange
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test that type-based queries maintain proper parent-child relationships
        var headings = document.GetHeadings();
        foreach (var heading in headings.Get())
        {
            var headingNode = (HeadingNode)heading;
            var parent = document.GetParent(headingNode);

            // Headings should have a parent (document root or container)
            Assert.NotNull(parent);
        }

        // Test that text nodes belong to their proper parents
        var textNodes = document.GetTextNodes();
        foreach (var textNode in textNodes.Get().Take(5)) // Test first 5 to avoid excessive testing
        {
            var parent = document.GetParent(textNode);
            Assert.NotNull(parent);
        }

        // Test that filtered results maintain structure
        var h1Headings = document.GetHeadings(1);
        foreach (var h1 in h1Headings.Get())
        {
            var headingNode = (HeadingNode)h1;
            Assert.Equal(1, headingNode.Level);

            // Should still be connected to document structure
            var depth = document.GetDepth(headingNode);
            Assert.True(depth >= 0);
        }
    }
}
