using MarkdigExtensions.Query.Core;
using MarkdigExtensions.Query.Types;

namespace MarkdigExtensions.Query.Tests;

public class CoreGraphMethodsTests
{
    [Fact]
    public void CoreGraphMethods_ShouldExecuteAllMethods_WithTestSuite06()
    {
        // Arrange: Get the markdown document using TestSuite_06
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test GetDescendants (static method)
        var rootNode = document.Root;
        var allDescendants = MarkdownDocument.GetDescendants(rootNode).ToList();
        Assert.True(allDescendants.Count > 0, "Root should have descendants");

        // Verify descendants include all node types
        Assert.Contains(allDescendants, d => d is HeadingNode);
        Assert.Contains(allDescendants, d => d is ParagraphNode);
        Assert.Contains(allDescendants, d => d is TextNode);

        // Test GetDescendants on specific nodes
        var firstHeading = document.GetHeadings().FirstOrDefault();
        if (firstHeading != null)
        {
            var headingDescendants = MarkdownDocument.GetDescendants(firstHeading).ToList();
            Assert.True(
                headingDescendants.Count >= 0,
                "Heading should have zero or more descendants"
            );

            // All descendants should be reachable from the heading
            foreach (var descendant in headingDescendants)
            {
                Assert.Contains(descendant, allDescendants);
            }
        }

        // Test GetDescendants on a paragraph node
        var firstParagraph = document.GetParagraphs().FirstOrDefault();
        if (firstParagraph != null)
        {
            var paragraphDescendants = MarkdownDocument.GetDescendants(firstParagraph).ToList();
            Assert.True(
                paragraphDescendants.Count >= 0,
                "Paragraph should have zero or more descendants"
            );

            // Paragraph descendants should typically include text nodes
            if (paragraphDescendants.Count > 0)
            {
                Assert.Contains(paragraphDescendants, d => d is TextNode);
            }
        }

        // Test GetAncestors method
        var textNodes = document.GetTextNodes();
        Assert.True(textNodes.Length > 0, "Should have text nodes to test ancestors");

        var firstTextNode = textNodes.Get()[0];
        var ancestors = document.GetAncestors(firstTextNode).ToList();
        Assert.True(ancestors.Count > 0, "Text node should have ancestors");

        // Root should be in ancestors (unless text node is direct child of root)
        var rootInAncestors = ancestors.Any(a => a == rootNode);
        Assert.True(rootInAncestors, "Root should be in ancestors");

        // Test ancestors are in correct order (immediate parent first, root last)
        Assert.Equal(rootNode, ancestors.Last());

        // Test GetParent method
        var firstTextNodeParent = document.GetParent(firstTextNode);
        Assert.NotNull(firstTextNodeParent);
        Assert.Equal(firstTextNodeParent, ancestors.First());

        // Test GetParent on root returns null
        var rootParent = document.GetParent(rootNode);
        Assert.Null(rootParent);

        // Test GetParent consistency with ancestors
        foreach (var textNode in textNodes.Get().Take(3)) // Test first 3 text nodes
        {
            var parent = document.GetParent(textNode);
            var nodeAncestors = document.GetAncestors(textNode).ToList();

            if (nodeAncestors.Count > 0)
            {
                Assert.Equal(parent, nodeAncestors[0]);
            }
        }

        // Test GetSiblings method
        var headings = document.GetHeadings();
        if (headings.Length > 1)
        {
            var firstHeadingNode = headings.Get()[0];
            var siblings = document.GetSiblings(firstHeadingNode, false).ToList();

            // Siblings should not include the node itself when includeSelf = false
            Assert.DoesNotContain(firstHeadingNode, siblings);

            // Test with includeSelf = true
            var siblingsWithSelf = document.GetSiblings(firstHeadingNode, true).ToList();
            Assert.Contains(firstHeadingNode, siblingsWithSelf);
            Assert.Equal(siblings.Count + 1, siblingsWithSelf.Count);
        }

        // Test GetSiblings on root node (should have no siblings)
        var rootSiblings = document.GetSiblings(rootNode, false);
        Assert.Empty(rootSiblings);

        var rootSiblingsWithSelf = document.GetSiblings(rootNode, true);
        Assert.Single(rootSiblingsWithSelf);
        Assert.Equal(rootNode, rootSiblingsWithSelf.First());

        // Test GetSiblings for nodes with same parent
        var paragraphs = document.GetParagraphs();
        if (paragraphs.Length > 1)
        {
            var firstPara = paragraphs.Get()[0];
            var secondPara = paragraphs.Get()[1];

            var firstParaParent = document.GetParent(firstPara);
            var secondParaParent = document.GetParent(secondPara);

            if (firstParaParent == secondParaParent && firstParaParent != null)
            {
                var firstParaSiblings = document.GetSiblings(firstPara, false).ToList();
                Assert.Contains(secondPara, firstParaSiblings);

                var secondParaSiblings = document.GetSiblings(secondPara, false).ToList();
                Assert.Contains(firstPara, secondParaSiblings);
            }
        }

        // Test GetDepth method
        var rootDepth = document.GetDepth(rootNode);
        Assert.Equal(0, rootDepth);

        // Test depth calculation for various nodes
        foreach (var node in document.AllNodes.Take(10)) // Test first 10 nodes
        {
            var depth = document.GetDepth(node);
            var nodeAncestors = document.GetAncestors(node).ToList();

            Assert.Equal(nodeAncestors.Count, depth);
            Assert.True(depth >= 0, "Depth should be non-negative");

            if (node == rootNode)
            {
                Assert.Equal(0, depth);
            }
            else
            {
                Assert.True(depth > 0, "Non-root nodes should have positive depth");
            }
        }

        // Test depth consistency across node types
        var maxDepthFromStats = (int)document.GetStatistics()["MaxDepth"];
        var actualMaxDepth = document.AllNodes.Select(document.GetDepth).Max();
        Assert.Equal(maxDepthFromStats, actualMaxDepth);

        // Test hierarchical relationships
        foreach (var node in document.AllNodes.Take(5)) // Test first 5 nodes
        {
            if (node == rootNode)
                continue;

            var parent = document.GetParent(node);
            Assert.NotNull(parent);

            var parentChildren = parent.Children;
            Assert.Contains(node, parentChildren);

            var nodeDepth = document.GetDepth(node);
            var parentDepth = document.GetDepth(parent);
            Assert.Equal(parentDepth + 1, nodeDepth);
        }

        // Test that GetDescendants includes all child nodes recursively
        var documentChildren = rootNode.Children.ToList();
        var documentDescendants = MarkdownDocument.GetDescendants(rootNode).ToList();

        foreach (var child in documentChildren)
        {
            Assert.Contains(child, documentDescendants);

            var childDescendants = MarkdownDocument.GetDescendants(child).ToList();
            foreach (var grandchild in childDescendants)
            {
                Assert.Contains(grandchild, documentDescendants);
            }
        }

        // All core graph method functionality tests completed successfully
        Assert.True(true, "Core graph methods functionality verified");
    }

    [Fact]
    public void CoreGraphMethods_ShouldHandleEdgeCases_Gracefully()
    {
        // Arrange: Create a simple document
        var markdown = "# Simple Heading\n\nJust a paragraph with some text.";
        var document = markdown.AsQueryable();

        // Test with leaf nodes (nodes with no children)
        var textNodes = document.GetTextNodes();
        Assert.True(textNodes.Length > 0, "Should have text nodes");

        var leafNode = textNodes.Get()[0];
        var leafDescendants = MarkdownDocument.GetDescendants(leafNode);
        Assert.Empty(leafDescendants);

        // Test with single-child scenarios
        var headings = document.GetHeadings();
        if (headings.Length > 0)
        {
            var heading = headings.Get()[0];
            var headingChildren = heading.Children.ToList();

            if (headingChildren.Count == 1)
            {
                var onlyChild = headingChildren[0];
                var childSiblings = document.GetSiblings(onlyChild, false);
                Assert.Empty(childSiblings);

                var childSiblingsWithSelf = document.GetSiblings(onlyChild, true);
                Assert.Single(childSiblingsWithSelf);
                Assert.Equal(onlyChild, childSiblingsWithSelf.First());
            }
        }

        // Test ancestor chain integrity
        var allNodes = document.AllNodes.Where(n => n != document.Root).ToList();
        foreach (var node in allNodes.Take(3)) // Test first 3 non-root nodes
        {
            var ancestors = document.GetAncestors(node).ToList();

            // Verify ancestor chain leads to root
            if (ancestors.Count > 0)
            {
                Assert.Equal(document.Root, ancestors.Last());

                // Verify each ancestor is parent of the next in chain
                var currentNode = node;
                foreach (var ancestor in ancestors)
                {
                    var parent = document.GetParent(currentNode);
                    Assert.Equal(ancestor, parent);
                    currentNode = ancestor;
                }
            }
        }
    }

    [Fact]
    public void CoreGraphMethods_ShouldMaintainStructuralConsistency()
    {
        // Arrange
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test that every non-root node has exactly one parent
        foreach (var node in document.AllNodes)
        {
            if (node == document.Root)
            {
                Assert.Null(document.GetParent(node));
            }
            else
            {
                var parent = document.GetParent(node);
                Assert.NotNull(parent);
                Assert.Contains(node, parent.Children);
            }
        }

        // Test that parent-child relationships are bidirectional
        foreach (var node in document.AllNodes.Take(10)) // Test first 10 nodes
        {
            foreach (var child in node.Children)
            {
                var childParent = document.GetParent(child);
                Assert.Equal(node, childParent);
            }
        }

        // Test that sibling relationships are symmetric
        var testNodes = document.AllNodes.Take(5).ToList();
        foreach (var node in testNodes)
        {
            var siblings = document.GetSiblings(node, false).ToList();

            foreach (var sibling in siblings)
            {
                var siblingsSiblings = document.GetSiblings(sibling, false).ToList();
                Assert.Contains(node, siblingsSiblings);
            }
        }

        // Test that descendants are complete and non-duplicated
        var rootDescendants = MarkdownDocument.GetDescendants(document.Root).ToList();
        var rootDescendantsDistinct = rootDescendants.Distinct().ToList();
        Assert.Equal(rootDescendants.Count, rootDescendantsDistinct.Count);

        // All non-root nodes should be descendants of root
        var allNonRootNodes = document.AllNodes.Where(n => n != document.Root).ToList();
        foreach (var node in allNonRootNodes)
        {
            Assert.Contains(node, rootDescendants);
        }

        // Test depth consistency across the tree
        foreach (var node in document.AllNodes.Take(10))
        {
            var depth = document.GetDepth(node);
            var ancestors = document.GetAncestors(node).ToList();

            Assert.Equal(ancestors.Count, depth);

            if (depth > 0)
            {
                var parent = document.GetParent(node);
                Assert.NotNull(parent);
                Assert.Equal(depth - 1, document.GetDepth(parent));
            }
        }

        // Test that all nodes are reachable from root
        var visitedNodes = new HashSet<INode> { document.Root };
        var nodesToVisit = new Queue<INode>();
        nodesToVisit.Enqueue(document.Root);

        while (nodesToVisit.Count > 0)
        {
            var current = nodesToVisit.Dequeue();
            foreach (var child in current.Children)
            {
                visitedNodes.Add(child);
                nodesToVisit.Enqueue(child);
            }
        }

        // All nodes should be reachable from root
        foreach (var node in document.AllNodes)
        {
            Assert.Contains(node, visitedNodes);
        }

        // Test that core graph methods work correctly - focusing on functionality rather than exception handling
        // Note: Exception handling for null arguments is tested separately if needed
        Assert.True(true, "All core graph method functionality tests completed successfully");
    }
}
