using MarkdigExtensions.Query.Types;

namespace MarkdigExtensions.Query.Tests;

public class IndexBasedSelectionTests
{
    [Fact]
    public void IndexBasedSelection_ShouldExecuteAllMethods_WithTestSuite06()
    {
        // Arrange: Get the markdown document using TestSuite_06
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test ElementAt with Index
        var headings = document.GetHeadings();
        Assert.True(headings.Length > 0, "Should have headings to test with");

        // Test ElementAt with first index (0)
        var firstHeading = headings.ElementAt(0);
        Assert.Equal(1, firstHeading.Length);
        Assert.Equal(headings.Get()[0], firstHeading.Get()[0]);

        // Test ElementAt with middle index
        if (headings.Length > 1)
        {
            var secondHeading = headings.ElementAt(1);
            Assert.Equal(1, secondHeading.Length);
            Assert.Equal(headings.Get()[1], secondHeading.Get()[0]);
        }

        // Test ElementAt with last index using ^1 syntax
        var lastHeading = headings.ElementAt(^1);
        Assert.Equal(1, lastHeading.Length);
        Assert.Equal(headings.Get()[^1], lastHeading.Get()[0]);

        // Test ElementAt with second-to-last index using ^2 syntax
        if (headings.Length > 1)
        {
            var secondLastHeading = headings.ElementAt(^2);
            Assert.Equal(1, secondLastHeading.Length);
            Assert.Equal(headings.Get()[^2], secondLastHeading.Get()[0]);
        }

        // Test ElementAt with out-of-range index
        var outOfRangeIndex = headings.ElementAt(999);
        Assert.Equal(0, outOfRangeIndex.Length);

        // Test ElementAt with negative index that's out of range
        var negativeOutOfRange = headings.ElementAt(^999);
        Assert.Equal(0, negativeOutOfRange.Length);

        // Test First() method
        var first = headings.First();
        Assert.Equal(1, first.Length);
        Assert.Equal(headings.Get()[0], first.Get()[0]);
        Assert.Equal(firstHeading.Get()[0], first.Get()[0]);

        // Test Last() method
        var last = headings.Last();
        Assert.Equal(1, last.Length);
        Assert.Equal(headings.Get()[^1], last.Get()[0]);
        Assert.Equal(lastHeading.Get()[0], last.Get()[0]);

        // Test Slice with Range
        var allNodes = document.GetTextNodes();
        Assert.True(allNodes.Length > 0, "Should have text nodes to test with");

        // Test Slice with range 0..3 (first 3 elements)
        var firstThree = allNodes.Slice(0..3);
        var expectedCount = Math.Min(3, allNodes.Length);
        Assert.Equal(expectedCount, firstThree.Length);
        for (int i = 0; i < expectedCount; i++)
        {
            Assert.Equal(allNodes.Get()[i], firstThree.Get()[i]);
        }

        // Test Slice with range 1..4 (elements 1-3)
        if (allNodes.Length > 1)
        {
            var middleSlice = allNodes.Slice(1..Math.Min(4, allNodes.Length));
            var expectedMiddleCount = Math.Min(3, allNodes.Length - 1);
            Assert.Equal(expectedMiddleCount, middleSlice.Length);
            for (int i = 0; i < expectedMiddleCount; i++)
            {
                Assert.Equal(allNodes.Get()[i + 1], middleSlice.Get()[i]);
            }
        }

        // Test Slice with range ^3..^1 (last 3 elements excluding the very last)
        if (allNodes.Length >= 3)
        {
            var lastThreeExceptLast = allNodes.Slice(^3..^1);
            Assert.Equal(2, lastThreeExceptLast.Length);
            Assert.Equal(allNodes.Get()[^3], lastThreeExceptLast.Get()[0]);
            Assert.Equal(allNodes.Get()[^2], lastThreeExceptLast.Get()[1]);
        }

        // Test Slice with range ^2.. (last 2 elements)
        if (allNodes.Length >= 2)
        {
            var lastTwo = allNodes.Slice(^2..);
            Assert.Equal(2, lastTwo.Length);
            Assert.Equal(allNodes.Get()[^2], lastTwo.Get()[0]);
            Assert.Equal(allNodes.Get()[^1], lastTwo.Get()[1]);
        }

        // Test Slice with range ..2 (first 2 elements)
        var firstTwoSlice = allNodes.Slice(..2);
        var expectedFirstTwoCount = Math.Min(2, allNodes.Length);
        Assert.Equal(expectedFirstTwoCount, firstTwoSlice.Length);
        for (int i = 0; i < expectedFirstTwoCount; i++)
        {
            Assert.Equal(allNodes.Get()[i], firstTwoSlice.Get()[i]);
        }

        // Test Slice with int overload
        var sliceIntOverload = allNodes.Slice(1, 3);
        if (allNodes.Length > 1)
        {
            var expectedSliceCount = Math.Min(2, allNodes.Length - 1);
            Assert.Equal(expectedSliceCount, sliceIntOverload.Length);
            for (int i = 0; i < expectedSliceCount; i++)
            {
                Assert.Equal(allNodes.Get()[i + 1], sliceIntOverload.Get()[i]);
            }
        }

        // Test Slice with int overload (start only)
        var sliceFromStart = allNodes.Slice(2);
        if (allNodes.Length > 2)
        {
            var expectedFromStartCount = allNodes.Length - 2;
            Assert.Equal(expectedFromStartCount, sliceFromStart.Length);
            for (int i = 0; i < expectedFromStartCount; i++)
            {
                Assert.Equal(allNodes.Get()[i + 2], sliceFromStart.Get()[i]);
            }
        }

        // Test out-of-range slice operations
        var outOfRangeSlice = allNodes.Slice(999..1000);
        Assert.Equal(0, outOfRangeSlice.Length);

        var invalidRangeSlice = allNodes.Slice(5..3); // end before start
        Assert.Equal(0, invalidRangeSlice.Length);

        // Test method chaining with index-based operations
        var chainedIndexOperations = document.GetHeadings().Slice(0..2).First();
        Assert.Equal(1, chainedIndexOperations.Length);

        // Test that index operations return new MarkdownDocument instances
        var originalSelection = document.GetHeadings();
        var firstElement = originalSelection.First();
        var lastElement = originalSelection.Last();
        var slicedElements = originalSelection.Slice(0..2);

        Assert.NotSame(originalSelection, firstElement);
        Assert.NotSame(originalSelection, lastElement);
        Assert.NotSame(originalSelection, slicedElements);
        Assert.NotSame(firstElement, lastElement);
        Assert.NotSame(firstElement, slicedElements);

        // Test index operations maintain document structure
        var selectedNode = document.GetHeadings().First().Get()[0];
        var parentFromDocument = document.GetParent(selectedNode);
        Assert.NotNull(parentFromDocument);

        // Test complex index-based scenarios
        var complexIndexing = document
            .Filter(node => node is ParagraphNode || node is HeadingNode)
            .Slice(1..5)
            .ElementAt(0);
        Assert.True(complexIndexing.Length <= 1);

        // Verify slice preserves order
        var orderedNodes = document.GetHeadings();
        if (orderedNodes.Length >= 3)
        {
            var slicedOrdered = orderedNodes.Slice(0..3);
            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(orderedNodes.Get()[i], slicedOrdered.Get()[i]);
            }
        }
    }

    [Fact]
    public void IndexBasedSelection_ShouldHandleEmptySelections_Gracefully()
    {
        // Arrange: Use TestSuite_06 which we know has content
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test index operations on empty selections
        var emptySelection = document.Filter(node => false);
        Assert.Equal(0, emptySelection.Length);

        // Test ElementAt on empty selection
        var emptyElementAt = emptySelection.ElementAt(0);
        Assert.Equal(0, emptyElementAt.Length);

        var emptyElementAtLast = emptySelection.ElementAt(^1);
        Assert.Equal(0, emptyElementAtLast.Length);

        // Test First on empty selection
        var emptyFirst = emptySelection.First();
        Assert.Equal(0, emptyFirst.Length);

        // Test Last on empty selection
        var emptyLast = emptySelection.Last();
        Assert.Equal(0, emptyLast.Length);

        // Test Slice on empty selection
        var emptySlice = emptySelection.Slice(0..3);
        Assert.Equal(0, emptySlice.Length);

        var emptySliceRange = emptySelection.Slice(^2..);
        Assert.Equal(0, emptySliceRange.Length);

        var emptySliceInt = emptySelection.Slice(1, 3);
        Assert.Equal(0, emptySliceInt.Length);

        // Test chaining operations on empty results
        var chainedEmpty = document.Filter("nonexistent").Slice(0..5).First().Last();
        Assert.Equal(0, chainedEmpty.Length);

        // Test that all index operations handle empty selections without throwing
        // This is the main purpose of this test - ensure graceful handling
        Assert.True(true, "All empty selection operations completed without throwing exceptions");
    }

    [Fact]
    public void IndexBasedSelection_ShouldMaintainSelectionIntegrity()
    {
        // Arrange
        var markdown = TestSetup.TestSuite_06;
        var document = markdown.AsQueryable();

        // Test that index operations preserve element identity
        var allHeadings = document.GetHeadings();
        if (allHeadings.Length > 0)
        {
            var firstHeading = allHeadings.First().Get()[0];
            var elementAtZero = allHeadings.ElementAt(0).Get()[0];
            var sliceFirstOne = allHeadings.Slice(0..1).Get()[0];

            Assert.Equal(firstHeading, elementAtZero);
            Assert.Equal(firstHeading, sliceFirstOne);
            Assert.Same(firstHeading, elementAtZero);
            Assert.Same(firstHeading, sliceFirstOne);
        }

        // Test that Last() and ElementAt(^1) return the same element
        if (allHeadings.Length > 0)
        {
            var lastHeading = allHeadings.Last().Get()[0];
            var elementAtLast = allHeadings.ElementAt(^1).Get()[0];

            Assert.Equal(lastHeading, elementAtLast);
            Assert.Same(lastHeading, elementAtLast);
        }

        // Test slice boundary conditions
        var textNodes = document.GetTextNodes();
        if (textNodes.Length >= 5)
        {
            var slice1to3 = textNodes.Slice(1..4);
            var slice1to3Alt = textNodes.Slice(1, 4);

            Assert.Equal(slice1to3.Length, slice1to3Alt.Length);
            for (int i = 0; i < slice1to3.Length; i++)
            {
                Assert.Equal(slice1to3.Get()[i], slice1to3Alt.Get()[i]);
                Assert.Same(slice1to3.Get()[i], slice1to3Alt.Get()[i]);
            }
        }

        // Test that slicing maintains relative order
        var paragraphs = document.GetParagraphs();
        if (paragraphs.Length >= 3)
        {
            var originalOrder = paragraphs.Get();
            var slicedOrder = paragraphs.Slice(0..3).Get();

            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(originalOrder[i], slicedOrder[i]);
            }
        }

        // Test index operations consistency across different selection sizes
        var smallSelection = document.GetHeadings().Slice(0..1);
        var largeSelection = document.GetTextNodes();

        // Both should handle First() consistently
        if (smallSelection.Length > 0)
        {
            var smallFirst = smallSelection.First();
            Assert.Equal(1, smallFirst.Length);
        }

        if (largeSelection.Length > 0)
        {
            var largeFirst = largeSelection.First();
            Assert.Equal(1, largeFirst.Length);
        }

        // Test that index-based methods maintain document context
        var selectedNode = document.GetTextNodes().First();
        if (selectedNode.Length > 0)
        {
            var node = selectedNode.Get()[0];
            var parent = document.GetParent(node);
            Assert.NotNull(parent);

            // The selected node should still be connected to the document structure
            var ancestors = document.GetAncestors(node);
            Assert.True(ancestors.Any());
        }
    }
}
