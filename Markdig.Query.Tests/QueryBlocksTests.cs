using Markdig.Syntax;

namespace Markdig.Query.Tests;

public class QueryBlocksTests
{
    [Fact]
    public void Should_Return_Links()
    {
        // Arrange
        var fileToTest = TestSetup.TestSuite_01;
        var markdownDocument = Markdown.Parse(fileToTest);
        var links = markdownDocument.QueryBlocks("link");

        // Assert
        Assert.NotNull(links);
        Assert.NotEmpty(links);

        const int expectedLinkCount = 5;

        Assert.Equal(expectedLinkCount, links.Count());
    }

    [Fact]
    public void Should_Return_Link()
    {
        // Arrange
        var fileToTest = TestSetup.TestSuite_01;
        var markdownDocument = Markdown.Parse(fileToTest);
        var link = markdownDocument.QueryBlock("link");

        // Assert
        Assert.NotNull(link);
        Assert.Single([link]);

        const string expectedLinkText = "regular link";
        Assert.Equal(expectedLinkText, link.InnerText);

        const string expectedLinkHref = "https://example.com";
        Assert.Equal(expectedLinkHref, link.Attr("href"));

        const string expectedLinkTitle = "Example";
        Assert.Equal(expectedLinkTitle, link.Attr("title"));

        const bool expectedNoImage = false;
        Assert.Equal(expectedNoImage, link.Attr("isImage"));
    }

    [Fact]
    public void Should_Return_Images()
    {
        // Arrange
        var fileToTest = TestSetup.TestSuite_01;
        var markdownDocument = Markdown.Parse(fileToTest);
        var images = markdownDocument.QueryBlocks("image");

        // Assert
        Assert.NotNull(images);
        Assert.NotEmpty(images);

        const int expectedImageCount = 2;
        Assert.Equal(expectedImageCount, images.Count());
    }

    [Fact]
    public void Should_Return_Image()
    {
        // Arrange
        var fileToTest = TestSetup.TestSuite_01;
        var markdownDocument = Markdown.Parse(fileToTest);
        var image = markdownDocument.QueryBlock("image");

        // Assert
        Assert.NotNull(image);
        Assert.Single([image]);

        const string expectedImageSrc =
            "https://media.veefriends.com/image/upload/v1700083094/veefriends/specials/series2/characters/reflective-rhinoceros-manifesting-shrinkwrapped.png";
        Assert.Equal(expectedImageSrc, image.Attr("src"));

        const string expectedImageAlt = "Rhino";
        Assert.Equal(expectedImageAlt, image.Attr("alt"));

        const bool expectedIsImage = true;
        Assert.Equal(expectedIsImage, image.Attr("isImage"));
    }

    [Fact]
    public void Should_Return_Headings()
    {
        // Arrange
        var fileToTest = TestSetup.TestSuite_01;
        var markdownDocument = Markdown.Parse(fileToTest);
        var headings = markdownDocument.QueryBlocks("heading");
        // Assert
        Assert.NotNull(headings);
        Assert.NotEmpty(headings);
        const int expectedHeadingCount = 14;
        Assert.Equal(expectedHeadingCount, headings.Count());
    }

    [Fact]
    public void Should_Return_Heading()
    {
        // Arrange
        var fileToTest = TestSetup.TestSuite_01;
        var markdownDocument = Markdown.Parse(fileToTest);
        var heading = markdownDocument.QueryBlock("heading");
        // Assert
        Assert.NotNull(heading);
        Assert.Single([heading]);

        const string expectedHeadingText = "Welcome to the Test Suite";
        Assert.Equal(expectedHeadingText, heading.InnerText);

        const int expectedHeadingLevel = 1;
        Assert.Equal(expectedHeadingLevel, heading.Attr("level"));
    }

    [Fact]
    public void Should_Return_All_Blocks()
    {
        // Arrange
        var fileToTest = TestSetup.TestSuite_01;
        var markdownDocument = Markdown.Parse(fileToTest);
        var blocks = markdownDocument.QueryBlocks("*");

        // Assert
        Assert.NotNull(blocks);
        Assert.NotEmpty(blocks);

        const int expectedBlockCount = 166; // Adjust this number based on the actual number of blocks in your test suite
        Assert.Equal(expectedBlockCount, blocks.Count());
    }

    [Fact]
    public void Should_Return_All_Blocks_Under_Headings()
    {
        // Arrange
        var fileToTest = TestSetup.TestSuite_01;
        var markdownDocument = Markdown.Parse(fileToTest);
        var blocks = markdownDocument.QueryBlocks("h2 > *");

        // Assert
        Assert.NotNull(blocks);
        Assert.NotEmpty(blocks);

        const int expectedBlockCount = 12; // Adjust this number based on the actual number of blocks in your test suite
        Assert.Equal(expectedBlockCount, blocks.Count());
    }

    [Fact]
    public void Should_Return_Empty_For_NonExistent_Selector()
    {
        // Arrange
        var fileToTest = TestSetup.TestSuite_01;
        var markdownDocument = Markdown.Parse(fileToTest);
        var blocks = markdownDocument.QueryBlocks("nonexistent");
        // Assert
        Assert.NotNull(blocks);
        Assert.Empty(blocks);
    }

    [Fact]
    public void Should_Return_Empty_For_Invalid_Selector()
    {
        // Arrange
        var fileToTest = TestSetup.TestSuite_01;
        var markdownDocument = Markdown.Parse(fileToTest);
        var blocks = markdownDocument.QueryBlocks("h1 > h2"); // Invalid selector: h1 cannot have h2 as a direct child
        // Assert
        Assert.NotNull(blocks);
        Assert.Empty(blocks);
    }

    [Fact]
    public void Should_Return_First_Child_Of_Selector()
    {
        // Arrange
        var fileToTest = TestSetup.TestSuite_01;
        var markdownDocument = Markdown.Parse(fileToTest);
        var blocks = markdownDocument.QueryBlocks("h1:first-child");

        // Assert
        Assert.NotNull(blocks);
        Assert.NotEmpty(blocks);
        Assert.Single(blocks);
    }

    [Fact]
    public void Should_Return_Blocks_Between_Two_Blocks()
    {
        // Arrange
        var fileToTest = """
            # Welcome to the Test Suite

            This document is designed to test all the features of your Markdown query engine.  
            It includes examples of every supported selector, attribute, and combinator.

            ---
            """;

        var markdownDocument = Markdown.Parse(fileToTest);

        // Assert
        var blocks = markdownDocument
            .QueryBlock("h1")
            ?.NextUntil("hr", includeStart: true, includeEnd: true);

        Assert.NotNull(blocks);
        Assert.NotEmpty(blocks);

        const int expectedBlockCount = 3; // Adjust this number based on the actual number of blocks between h1 and hr in your test suite
        Assert.Equal(expectedBlockCount, blocks.Count());

        // Check if the first block is indeed the h1 block
        var firstBlock = blocks.First();
        Assert.True(firstBlock.Node is HeadingBlock);
        Assert.Equal(1, ((HeadingBlock)firstBlock.Node).Level);
        Assert.Equal("Welcome to the Test Suite", firstBlock.InnerText);

        // Check if the last block is indeed the hr block
        var lastBlock = blocks.Last();
        Assert.True(lastBlock.Node is ThematicBreakBlock);
        Assert.Equal(0, lastBlock.InnerText.Length); // hr block should not have inner text
        //Assert.Equal("hr", lastBlock.Node?.GetType().Name);
    }
}
