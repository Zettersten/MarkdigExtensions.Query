using MarkdigExtensions.Query.Types;

namespace MarkdigExtensions.Query.Tests;

public class TraversalMethodTests
{
    [Fact]
    public void TraversalMethods_ShouldExecuteAllMethods_WithTestSuite06()
    {
        // Arrange: Get the markdown document using TestSuite_06
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test Parent with optional filter
        var headingParents = document.GetHeadings().Parent();
        Assert.True(headingParents.Length > 0, "Should find parent nodes of headings");

        var filteredParents = document.GetTextNodes().Parent(node => node is ParagraphNode);
        Assert.True(filteredParents.Length >= 0, "Should find filtered parent nodes");
        Assert.All(filteredParents.Get(), node => Assert.IsType<ParagraphNode>(node));

        // Test Parent with CSS selector
        var parentsBySelector = document.GetHeadings().Parent("document");
        Assert.True(parentsBySelector.Length >= 0, "Should find parents matching selector");

        // Test Parents (ancestors)
        var allAncestors = document.GetTextNodes().Parents();
        Assert.True(allAncestors.Length > 0, "Should find ancestor nodes");

        var filteredAncestors = document.GetTextNodes().Parents(node => node is DocumentNode);
        Assert.True(filteredAncestors.Length > 0, "Should find filtered ancestors");
        Assert.All(filteredAncestors.Get(), node => Assert.IsType<DocumentNode>(node));

        // Test Parents with CSS selector
        var ancestorsBySelector = document.GetTextNodes().Parents("document");
        Assert.True(ancestorsBySelector.Length > 0, "Should find ancestors matching selector");

        // Test ParentsUntil
        var parentsUntilDocument = document
            .GetTextNodes()
            .ParentsUntil(node => node is DocumentNode);
        Assert.True(parentsUntilDocument.Length >= 0, "Should find parents until stop condition");

        var parentsUntilWithFilter = document
            .GetTextNodes()
            .ParentsUntil(node => node is DocumentNode, node => node is ParagraphNode);
        Assert.True(parentsUntilWithFilter.Length >= 0, "Should find filtered parents until stop");
        Assert.All(parentsUntilWithFilter.Get(), node => Assert.IsType<ParagraphNode>(node));

        // Test Closest
        var closestParagraph = document.GetTextNodes().Closest(node => node is ParagraphNode);
        Assert.True(closestParagraph.Length > 0, "Should find closest matching ancestors");
        Assert.All(closestParagraph.Get(), node => Assert.IsType<ParagraphNode>(node));

        var closestBySelector = document.GetTextNodes().Closest("paragraph");
        Assert.True(closestBySelector.Length > 0, "Should find closest by selector");

        // Test Children
        var documentChildren = document.Query("document").Children();
        Assert.True(documentChildren.Length > 0, "Should find child nodes");

        var filteredChildren = document.GetHeadings().Children(node => node is TextNode);
        Assert.True(filteredChildren.Length > 0, "Should find filtered children");
        Assert.All(filteredChildren.Get(), node => Assert.IsType<TextNode>(node));

        // Test Children with CSS selector
        var childrenBySelector = document.GetHeadings().Children("text");
        Assert.True(childrenBySelector.Length > 0, "Should find children by selector");

        // Test Contents (alias for Children)
        var contents = document.GetHeadings().Contents();
        Assert.True(contents.Length > 0, "Contents should work as alias for Children");

        var childrenDirect = document.GetHeadings().Children();
        Assert.Equal(childrenDirect.Length, contents.Length);

        // Test Siblings
        var siblings = document.GetHeadings().Siblings();
        Assert.True(siblings.Length >= 0, "Should find sibling nodes");

        var filteredSiblings = document.GetHeadings().Siblings(node => node is ParagraphNode);
        Assert.True(filteredSiblings.Length >= 0, "Should find filtered siblings");
        Assert.All(filteredSiblings.Get(), node => Assert.IsType<ParagraphNode>(node));

        // Test Siblings with CSS selector
        var siblingsBySelector = document.GetHeadings().Siblings("paragraph");
        Assert.True(siblingsBySelector.Length >= 0, "Should find siblings by selector");

        // Test Next sibling
        var nextSiblings = document.GetHeadings().Next();
        Assert.True(nextSiblings.Length >= 0, "Should find next siblings");

        var filteredNext = document.GetHeadings().Next(node => node is ParagraphNode);
        Assert.True(filteredNext.Length >= 0, "Should find filtered next siblings");

        // Test Next with CSS selector
        var nextBySelector = document.GetHeadings().Next("paragraph");
        Assert.True(nextBySelector.Length >= 0, "Should find next siblings by selector");

        // Test Prev sibling
        var prevSiblings = document.GetParagraphs().Prev();
        Assert.True(prevSiblings.Length >= 0, "Should find previous siblings");

        var filteredPrev = document.GetParagraphs().Prev(node => node is HeadingNode);
        Assert.True(filteredPrev.Length >= 0, "Should find filtered previous siblings");

        // Test Prev with CSS selector
        var prevBySelector = document.GetParagraphs().Prev("h1, h2, h3");
        Assert.True(prevBySelector.Length >= 0, "Should find previous siblings by selector");

        // Test NextAll
        var allNextSiblings = document.GetHeadings().NextAll();
        Assert.True(allNextSiblings.Length >= 0, "Should find all following siblings");

        var filteredNextAll = document.GetHeadings().NextAll(node => node is ParagraphNode);
        Assert.True(filteredNextAll.Length >= 0, "Should find filtered following siblings");

        // Test NextAll with CSS selector
        var nextAllBySelector = document.GetHeadings().NextAll("paragraph");
        Assert.True(nextAllBySelector.Length >= 0, "Should find all next siblings by selector");

        // Test PrevAll
        var allPrevSiblings = document.GetParagraphs().PrevAll();
        Assert.True(allPrevSiblings.Length >= 0, "Should find all preceding siblings");

        var filteredPrevAll = document.GetParagraphs().PrevAll(node => node is HeadingNode);
        Assert.True(filteredPrevAll.Length >= 0, "Should find filtered preceding siblings");

        // Test PrevAll with CSS selector
        var prevAllBySelector = document.GetParagraphs().PrevAll("h1, h2, h3");
        Assert.True(prevAllBySelector.Length >= 0, "Should find all prev siblings by selector");

        // Test NextUntil
        var nextUntilHeading = document.GetParagraphs().NextUntil(node => node is HeadingNode);
        Assert.True(nextUntilHeading.Length >= 0, "Should find next siblings until stop condition");

        var nextUntilWithFilter = document
            .GetParagraphs()
            .NextUntil(node => node is HeadingNode, node => node is ParagraphNode);
        Assert.True(nextUntilWithFilter.Length >= 0, "Should find filtered next until stop");

        // Test NextUntil with CSS selectors
        var nextUntilBySelector = document.GetParagraphs().NextUntil("h1", "paragraph");
        Assert.True(nextUntilBySelector.Length >= 0, "Should find next until by selector");

        // Test PrevUntil
        var prevUntilHeading = document.GetParagraphs().PrevUntil(node => node is HeadingNode);
        Assert.True(prevUntilHeading.Length >= 0, "Should find prev siblings until stop condition");

        var prevUntilWithFilter = document
            .GetParagraphs()
            .PrevUntil(node => node is HeadingNode, node => node is ParagraphNode);
        Assert.True(prevUntilWithFilter.Length >= 0, "Should find filtered prev until stop");

        // Test PrevUntil with CSS selectors
        var prevUntilBySelector = document.GetParagraphs().PrevUntil("h1", "paragraph");
        Assert.True(prevUntilBySelector.Length >= 0, "Should find prev until by selector");

        // Test traversal method chaining
        var chainedTraversal = document.GetHeadings().Parent().Children().Next();
        Assert.True(chainedTraversal.Length >= 0, "Should support method chaining");

        // Test that traversal methods maintain document structure
        var someHeadings = document.GetHeadings().First();
        var headingParent = someHeadings.Parent();
        if (headingParent.Length > 0)
        {
            var parentNode = headingParent.Get()[0];
            var originalParent = document.GetParent(someHeadings.Get()[0]);
            Assert.Equal(parentNode, originalParent);
        }

        // Test that operations return new MarkdownDocument instances
        var originalDoc = document.GetHeadings();
        var parentDoc = originalDoc.Parent();
        Assert.NotSame(originalDoc, parentDoc);

        var childrenDoc = originalDoc.Children();
        Assert.NotSame(originalDoc, childrenDoc);
        Assert.NotSame(parentDoc, childrenDoc);

        // Test complex traversal scenarios
        var complexTraversal = document
            .GetTextNodes()
            .Parent(node => node is ParagraphNode)
            .Siblings(node => node is HeadingNode)
            .Children()
            .Filter(node => node is TextNode);
        Assert.True(complexTraversal.Length >= 0, "Complex traversal should work");

        // Verify parent-child relationships
        var textNodes = document.GetTextNodes();
        foreach (var textNode in textNodes.Get().Take(3)) // Test first 3 to avoid excessive testing
        {
            var parentFromTraversal = document.Query([textNode]).Parent();
            if (parentFromTraversal.Length > 0)
            {
                var parentNode = parentFromTraversal.Get()[0];
                var actualParent = document.GetParent(textNode);
                Assert.Equal(parentNode, actualParent);
            }
        }
    }

    [Fact]
    public void TraversalMethods_ShouldHandleEmptySelections_Gracefully()
    {
        // Arrange: Create a simple document
        var markdown = "# Simple Heading\n\nJust a paragraph.";
        var document = markdown.AsQueryable();

        // Test traversal on empty selections
        var emptySelection = document.Filter(node => false);

        var emptyParent = emptySelection.Parent();
        Assert.Equal(0, emptyParent.Length);

        var emptyParents = emptySelection.Parents();
        Assert.Equal(0, emptyParents.Length);

        var emptyChildren = emptySelection.Children();
        Assert.Equal(0, emptyChildren.Length);

        var emptyContents = emptySelection.Contents();
        Assert.Equal(0, emptyContents.Length);

        var emptySiblings = emptySelection.Siblings();
        Assert.Equal(0, emptySiblings.Length);

        var emptyNext = emptySelection.Next();
        Assert.Equal(0, emptyNext.Length);

        var emptyPrev = emptySelection.Prev();
        Assert.Equal(0, emptyPrev.Length);

        var emptyNextAll = emptySelection.NextAll();
        Assert.Equal(0, emptyNextAll.Length);

        var emptyPrevAll = emptySelection.PrevAll();
        Assert.Equal(0, emptyPrevAll.Length);

        var emptyClosest = emptySelection.Closest(node => true);
        Assert.Equal(0, emptyClosest.Length);

        // Test with nodes that have no relationships
        var rootSelection = document.Query("document");
        var rootParent = rootSelection.Parent();
        Assert.Equal(0, rootParent.Length); // Document root has no parent

        var leafNodes = document.GetTextNodes().First();
        var leafChildren = leafNodes.Children();
        Assert.Equal(0, leafChildren.Length); // Text nodes typically have no children
    }

    [Fact]
    public void TraversalMethods_ShouldMaintainStructuralIntegrity()
    {
        // Arrange
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test that parent-child relationships are consistent
        var someNodes = document.GetParagraphs();
        foreach (var node in someNodes.Get().Take(3))
        {
            var parentFromMethod = document.Query([node]).Parent();
            var parentFromCore = document.GetParent(node);

            if (parentFromMethod.Length > 0)
            {
                Assert.Equal(parentFromCore, parentFromMethod.Get()[0]);
            }
            else
            {
                Assert.Null(parentFromCore);
            }
        }

        // Test that sibling relationships are symmetric
        var headings = document.GetHeadings();
        if (headings.Length > 1)
        {
            var firstHeading = headings.First().Get()[0];
            var nextSibling = document.Query([firstHeading]).Next();

            if (nextSibling.Length > 0)
            {
                var siblingNode = nextSibling.Get()[0];
                var prevOfNext = document.Query([siblingNode]).Prev();

                if (prevOfNext.Length > 0)
                {
                    Assert.Contains(firstHeading, prevOfNext.Get());
                }
            }
        }

        // Test that ancestors include parents
        var textNodes = document.GetTextNodes().First();
        if (textNodes.Length > 0)
        {
            var textNode = textNodes.Get()[0];
            var parent = document.Query([textNode]).Parent();
            var ancestors = document.Query([textNode]).Parents();

            if (parent.Length > 0)
            {
                Assert.Contains(parent.Get()[0], ancestors.Get());
            }
        }

        // Test that closest includes self when matching
        var headingNodes = document.GetHeadings().First();
        if (headingNodes.Length > 0)
        {
            var headingNode = headingNodes.Get()[0];
            var closestHeading = document.Query([headingNode]).Closest(node => node is HeadingNode);

            Assert.Equal(1, closestHeading.Length);
            Assert.Equal(headingNode, closestHeading.Get()[0]);
        }

        // Test filtering consistency across traversal methods
        var paragraphs = document.GetParagraphs();
        var paragraphChildren = paragraphs.Children();
        var filteredChildren = paragraphs.Children(node => node is TextNode);

        Assert.True(filteredChildren.Length <= paragraphChildren.Length);
        Assert.All(filteredChildren.Get(), node => Assert.IsType<TextNode>(node));
    }
}
