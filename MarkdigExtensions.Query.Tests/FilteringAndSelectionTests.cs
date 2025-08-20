using MarkdigExtensions.Query.Core;
using MarkdigExtensions.Query.Types;

namespace MarkdigExtensions.Query.Tests;

public class FilteringAndSelectionTests
{
    [Fact]
    public void FilteringAndSelection_ShouldExecuteAllMethods_WithTestSuite06()
    {
        // Arrange: Get the markdown document using TestSuite_06
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test Filter with predicate
        var filteredByPredicate = document.Filter(node => node is HeadingNode);
        Assert.True(
            filteredByPredicate.Length > 0,
            "Filter with predicate should find heading nodes"
        );
        Assert.All(filteredByPredicate.Get(), node => Assert.IsType<HeadingNode>(node));

        var filteredTextNodes = document.Filter(node =>
            node is TextNode textNode && !string.IsNullOrWhiteSpace(textNode.Value)
        );
        Assert.True(filteredTextNodes.Length > 0, "Filter should find text nodes with content");

        // Test Filter with CSS selector
        var filteredBySelector = document.Filter("h1");
        Assert.True(filteredBySelector.Length > 0, "Filter with CSS selector should find H1 nodes");

        var filteredLinks = document.Filter("link");
        Assert.True(filteredLinks.Length > 0, "Filter should find link nodes");

        var filteredImages = document.Filter("image");
        Assert.True(filteredImages.Length > 0, "Filter should find image nodes");

        // Test chaining filters
        var chainedFilter = document.GetHeadings().Filter(node => ((HeadingNode)node).Level == 1);
        Assert.True(chainedFilter.Length > 0, "Chained filter should work");
        Assert.All(
            chainedFilter.Get(),
            node =>
            {
                var heading = Assert.IsType<HeadingNode>(node);
                Assert.Equal(1, heading.Level);
            }
        );

        // Test Not with predicate
        var notHeadings = document.Not(node => node is HeadingNode);
        Assert.True(notHeadings.Length > 0, "Not with predicate should exclude heading nodes");
        Assert.All(notHeadings.Get(), node => Assert.IsNotType<HeadingNode>(node));

        var notEmptyText = document
            .GetTextNodes()
            .Not(node => string.IsNullOrWhiteSpace(((TextNode)node).Value));
        Assert.True(notEmptyText.Length > 0, "Not should exclude empty text nodes");

        // Test Not with CSS selector
        var notH1 = document.GetHeadings().Not("h1");
        Assert.True(notH1.Length > 0, "Not with CSS selector should exclude H1 nodes");
        Assert.All(
            notH1.Get(),
            node =>
            {
                var heading = Assert.IsType<HeadingNode>(node);
                Assert.NotEqual(1, heading.Level);
            }
        );

        // Test Not with IEnumerable<INode>
        var h1Nodes = document.Query("h1").Get();
        var notH1Exclusions = document.GetHeadings().Not(h1Nodes);
        Assert.True(notH1Exclusions.Length > 0, "Not with node exclusions should work");
        Assert.All(
            notH1Exclusions.Get(),
            node =>
            {
                var heading = Assert.IsType<HeadingNode>(node);
                Assert.NotEqual(1, heading.Level);
            }
        );

        // Test Has with predicate
        var nodesWithTextChildren = document.Has(node => node is TextNode);
        Assert.True(
            nodesWithTextChildren.Length > 0,
            "Has with predicate should find nodes with text children"
        );

        var nodesWithLinkChildren = document.Has(node => node is LinkNode);
        Assert.True(nodesWithLinkChildren.Length > 0, "Has should find nodes with link children");

        // Test Has with CSS selector
        var nodesWithH1Descendants = document.Has("h1");
        Assert.True(
            nodesWithH1Descendants.Length > 0,
            "Has with CSS selector should find nodes with H1 descendants"
        );

        var nodesWithLinkDescendants = document.Has("link");
        Assert.True(
            nodesWithLinkDescendants.Length > 0,
            "Has should find nodes with link descendants"
        );

        var nodesWithImageDescendants = document.Has("image");
        Assert.True(
            nodesWithImageDescendants.Length > 0,
            "Has should find nodes with image descendants"
        );

        // Test Is with predicate
        var headingsSelection = document.GetHeadings();
        var isHeading = headingsSelection.Is(node => node is HeadingNode);
        Assert.True(isHeading, "Is with predicate should return true for heading selection");

        var textSelection = document.GetTextNodes();
        var isHeadingForText = textSelection.Is(node => node is HeadingNode);
        Assert.False(
            isHeadingForText,
            "Is should return false when testing text nodes for heading type"
        );

        var hasNonEmptyText = document
            .GetTextNodes()
            .Is(node => !string.IsNullOrWhiteSpace(((TextNode)node).Value));
        Assert.True(hasNonEmptyText, "Is should find non-empty text nodes");

        // Test Is with CSS selector
        var isH1 = document.GetHeadings().Is("h1");
        Assert.True(
            isH1,
            "Is with CSS selector should return true when H1 exists in heading selection"
        );

        var paragraphsIsH1 = document.GetParagraphs().Is("h1");
        Assert.False(paragraphsIsH1, "Is should return false when testing paragraphs for H1");

        var isLink = document.Is("link");
        Assert.True(isLink, "Is should return true when links exist in document");

        var isImage = document.Is("image");
        Assert.True(isImage, "Is should return true when images exist in document");

        // Test complex filtering scenarios
        var complexFilter = document
            .Filter(node => node is ParagraphNode)
            .Has(node => node is LinkNode)
            .Not(node => node is ImageNode);
        Assert.True(complexFilter.Length >= 0, "Complex filtering should work without errors");

        // Test filtering maintains document structure
        var filteredHeadings = document.Filter(node => node is HeadingNode);
        foreach (var heading in filteredHeadings.Get())
        {
            var parent = document.GetParent(heading);
            Assert.NotNull(parent);
        }

        // Test that filtered results are subsets of original
        var originalCount = document.Length;
        var filteredCount = document.Filter(node => node is HeadingNode).Length;
        Assert.True(
            filteredCount <= originalCount,
            "Filtered results should be subset of original"
        );

        // Test edge cases
        var emptyFilter = document.Filter(node => false);
        Assert.Equal(0, emptyFilter.Length);

        var allNodesFilter = document.Filter(node => true);
        Assert.Equal(document.Length, allNodesFilter.Length);

        // Test selector-based operations with complex selectors
        var complexSelectorFilter = document.Filter("h1, h2, h3");
        Assert.True(complexSelectorFilter.Length > 0, "Complex selector should work");

        var hasComplexSelector = document.Has("strong, emphasis");
        Assert.True(hasComplexSelector.Length >= 0, "Has with complex selector should work");

        // Test method chaining preserves selection stack
        var chainedOperations = document
            .GetHeadings()
            .Filter(node => ((HeadingNode)node).Level <= 2)
            .Not(node => ((HeadingNode)node).Level == 3)
            .Has(node => node is TextNode);

        Assert.True(chainedOperations.Length >= 0, "Complex method chaining should work");

        // Verify that operations return new MarkdownDocument instances
        var originalDoc = document;
        var filteredDoc = document.Filter(node => node is HeadingNode);
        Assert.NotSame(originalDoc, filteredDoc);

        var notDoc = document.Not(node => node is HeadingNode);
        Assert.NotSame(originalDoc, notDoc);
        Assert.NotSame(filteredDoc, notDoc);
    }

    [Fact]
    public void FilteringAndSelection_ShouldHandleEmptySelections_Gracefully()
    {
        // Arrange: Create a simple document
        var markdown = "# Simple Heading\n\nJust a paragraph.";
        var document = markdown.AsQueryable();

        // Test filtering that results in empty selection
        var noTables = document.Filter(node => node is TableNode);
        Assert.Equal(0, noTables.Length);

        var noImages = document.Filter("image");
        Assert.Equal(0, noImages.Length);

        // Test Not operations on empty selections
        var notFromEmpty = noTables.Not(node => true);
        Assert.Equal(0, notFromEmpty.Length);

        // Test Has operations that return empty
        var hasTableDescendants = document.Has("table");
        Assert.Equal(0, hasTableDescendants.Length);

        // Test Is operations on empty selections
        var emptyIsHeading = noTables.Is(node => node is HeadingNode);
        Assert.False(emptyIsHeading);

        var emptyIsSelector = noImages.Is("h1");
        Assert.False(emptyIsSelector);

        // Test chaining operations on empty results
        var chainedEmpty = document.Filter("table").Not("image").Has("link");
        Assert.Equal(0, chainedEmpty.Length);
    }

    [Fact]
    public void FilteringAndSelection_ShouldMaintainSelectionIntegrity()
    {
        // Arrange
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test that filtering maintains proper types
        var headings = document.Filter(node => node is HeadingNode);
        Assert.All(headings.Get(), node => Assert.IsType<HeadingNode>(node));

        var links = document.Filter("link");
        Assert.All(links.Get(), node => Assert.IsType<LinkNode>(node));

        // Test that Not operations maintain complementary sets
        var onlyHeadings = document.Filter(node => node is HeadingNode);
        var notHeadings = document.Not(node => node is HeadingNode);

        // Verify no overlap
        var headingNodes = onlyHeadings.Get().ToHashSet();
        var notHeadingNodes = notHeadings.Get().ToHashSet();
        Assert.Empty(headingNodes.Intersect(notHeadingNodes));

        // Test Has operations find correct parent-child relationships
        var withTextChildren = document.Has(node => node is TextNode);
        foreach (var parent in withTextChildren.Get())
        {
            var descendants = MarkdownDocument.GetDescendants(parent);
            Assert.True(
                descendants.Any(d => d is TextNode),
                "Node marked as having text children should actually have text descendants"
            );
        }

        // Test Is operations are consistent
        var h1Selection = document.Query("h1");
        var isH1ByPredicate = h1Selection.Is(node => node is HeadingNode h && h.Level == 1);
        var isH1BySelector = h1Selection.Is("h1");
        Assert.Equal(isH1ByPredicate, isH1BySelector);
    }
}
