![MarkdigExtensions.Query Logo](https://raw.githubusercontent.com/Zettersten/MarkdigExtensions.Query/refs/heads/main/icon.png)

# MarkdigExtensions.Query 📄🔍

[![NuGet version](https://badge.fury.io/nu/MarkdigExtensions.Query.svg)](https://badge.fury.io/nu/MarkdigExtensions.Query)

**MarkdigExtensions.Query** is a powerful, jQuery-style query engine for traversing, filtering, and manipulating Markdown documents in .NET. Built on top of the [Markdig](https://github.com/xoofx/markdig) Markdown parser, it provides an expressive API for searching and transforming Markdown nodes with CSS-like selectors and familiar LINQ-style chaining.

## ✨ Features

- 🔍 **jQuery-style Querying**: Familiar syntax with CSS selectors and method chaining
- 🎯 **Type-safe Node Access**: Strongly-typed access to all Markdown elements
- 📊 **Document Analysis**: Built-in statistics, outline generation, and link analysis
- 🔄 **LINQ Integration**: Seamless integration with LINQ operations
- 🌲 **DOM-like Traversal**: Parent, child, sibling, and ancestor navigation
- 🎨 **CSS Selector Support**: Full CSS selector syntax with combinators and pseudo-classes
- 📈 **Performance Optimized**: Indexed lookups and efficient querying
- 🔗 **Extension Methods**: Easy integration with existing Markdig workflows

## 🚀 Quick Start

### Installation

```bash
dotnet add package MarkdigExtensions.Query
```

### Basic Usage

```csharp
using MarkdigExtensions.Query;

// Convert any markdown string to a queryable document
var markdown = """
# Welcome to My Document

This is a paragraph with a [link](https://example.com) and **bold text**.

## Section 2

- Item 1
- Item 2 with *emphasis*


\```
Console.WriteLine("Hello World!");
\```

| Column 1 | Column 2 |
|----------|----------|
| Value A  | Value B  |
""";

// Create queryable document
var document = markdown.AsQueryable();

// Now you can query like jQuery!
var headings = document.GetHeadings();
var links = document.GetLinks();
var codeBlocks = document.GetCodeBlocks();
```

## 📖 Core Concepts

### Document Conversion

Convert markdown to queryable documents using extension methods:

```csharp
// From string
var document = markdown.AsQueryable();

// From Markdig document
var markdigDoc = Markdown.Parse(markdown);
var document = markdigDoc.AsQueryable();
```

### Element Selection

Access different types of markdown elements:

```csharp
// Type-based selection
var headings = document.GetHeadings();           // All headings
var h1s = document.GetHeadings(1);              // Only H1 headings
var paragraphs = document.GetParagraphs();      // All paragraphs
var links = document.GetLinks();                // All links
var images = document.GetImages();              // All images
var lists = document.GetLists();                // All lists
var codeBlocks = document.GetCodeBlocks();      // All code blocks
var tables = document.GetTables();              // All tables
var textNodes = document.GetTextNodes();        // All text nodes

// Generic selection with filtering
var strongNodes = document.GetNodes<StrongNode>();
var emphasizedText = document.GetNodes<EmphasisNode>(e => e.Children.Any());
```

## 🎯 CSS Selector Querying

Use familiar CSS selectors to find elements:

```csharp
// Basic selectors
var h1s = document.Query("h1");                 // All H1 headings
var links = document.Query("link");             // All links
var images = document.Query("image");           // All images
var codeBlocks = document.Query("codeblock");   // All code blocks

// Attribute selectors
var csharpCode = document.Query("codeblock[language=csharp]");
var externalLinks = document.Query("link[url^='https://']");
var level2Headings = document.Query("heading[level='2']");

// Combinators
var linkTexts = document.Query("link text");    // Text nodes inside links
var directChildren = document.Query("ul > li"); // Direct list item children
var adjacentSiblings = document.Query("h1 + p"); // Paragraphs after H1s

// Pseudo-classes
var firstHeading = document.Query("heading:first");
var lastParagraph = document.Query("paragraph:last");
var evenItems = document.Query("li:even");
var nthChild = document.Query("li:nth-child(2n+1)");

// Multiple selectors
var headingsAndLinks = document.Query("heading, link");
```

## 🌲 DOM-style Traversal

Navigate the document tree like a DOM:

```csharp
var textNodes = document.GetTextNodes();

// Parent navigation
var parents = textNodes.Parent();               // Direct parents
var ancestors = textNodes.Parents();            // All ancestors
var closestParagraph = textNodes.Closest("paragraph");

// Child navigation
var children = document.Children();             // Direct children
var descendants = document.Find("text");       // All descendant text nodes

// Sibling navigation
var nextSiblings = headings.Next();            // Next siblings
var prevSiblings = headings.Prev();           // Previous siblings
var allNextSiblings = headings.NextAll();     // All following siblings
var allPrevSiblings = headings.PrevAll();     // All preceding siblings

// Conditional traversal
var nextParagraphs = headings.Next("paragraph");
var parentsUntilDocument = textNodes.ParentsUntil(n => n is DocumentNode);
```

## 🔍 Filtering and Selection

Filter and refine your selections:

```csharp
// Predicate filtering
var longParagraphs = document.GetParagraphs()
    .Filter(p => p.Children.Count > 10);

var externalLinks = document.GetLinks()
    .Filter(link => ((LinkNode)link).Url?.StartsWith("http") == true);

// CSS selector filtering
var h1AndH2 = document.GetHeadings().Filter("h1, h2");
var notCodeBlocks = document.Not("codeblock");

// Exclusion filtering
var nonEmptyParagraphs = document.GetParagraphs()
    .Not(p => p.Children.Count == 0);

// Has filtering (contains descendants)
var paragraphsWithLinks = document.GetParagraphs()
    .Has("link");

var itemsWithCode = document.Query("li")
    .Has(item => item.Descendants().Any(d => d is CodeSpanNode));

// Is testing
bool hasH1 = document.Is("h1");
bool hasExternalLinks = document.GetLinks()
    .Is(link => ((LinkNode)link).Url?.StartsWith("http") == true);
```

## 📍 Index-based Selection

Access elements by position:

```csharp
var headings = document.GetHeadings();

// Index access
var firstHeading = headings.First();           // First element
var lastHeading = headings.Last();             // Last element
var thirdHeading = headings.ElementAt(2);      // Zero-based index
var secondToLast = headings.ElementAt(^2);     // From end

// Range slicing
var firstThree = headings.Slice(0..3);         // First 3 elements
var lastTwo = headings.Slice(^2..);            // Last 2 elements
var middle = headings.Slice(1..4);             // Elements 1-3
var skipTwo = headings.Slice(2);               // Skip first 2

// Safe access
var maybeFirst = headings.FirstOrDefault();    // Null if empty
var maybeLast = headings.LastOrDefault();      // Null if empty
```

## 🔄 LINQ Integration and Transformations

Seamlessly integrate with LINQ:

```csharp
// Transform to other types
var headingTexts = document.GetHeadings()
    .Select(h => ((HeadingNode)h).Value)
    .Where(text => !string.IsNullOrEmpty(text))
    .ToList();

var linkUrls = document.GetLinks()
    .Select(link => ((LinkNode)link).Url)
    .Where(url => url?.StartsWith("https://") == true)
    .ToArray();

// Complex transformations with index
var headingInfo = document.GetHeadings()
    .Select((index, node) => new {
        Index = index,
        Level = ((HeadingNode)node).Level,
        Text = node.Value,
        Depth = document.GetDepth(node)
    })
    .OrderBy(info => info.Level)
    .ToList();

// Iterate with actions
document.GetHeadings().Each((index, heading) => {
    Console.WriteLine($"Heading {index}: {heading.Value}");
});

document.GetLinks().Each(link => {
    var linkNode = (LinkNode)link;
    Console.WriteLine($"Link: {linkNode.Value} -> {linkNode.Url}");
});
```

## 📊 Document Analysis

Analyze and extract insights from your documents:

### Statistics

```csharp
var stats = document.GetStatistics();
// or: var stats = document.GetDocumentStatistics();

Console.WriteLine($"Total nodes: {stats["TotalNodes"]}");
Console.WriteLine($"Headings: {stats["HeadingCount"]}");
Console.WriteLine($"Paragraphs: {stats["ParagraphCount"]}");
Console.WriteLine($"Links: {stats["LinkCount"]}");
Console.WriteLine($"Images: {stats["ImageCount"]}");
Console.WriteLine($"Code blocks: {stats["CodeBlockCount"]}");
Console.WriteLine($"Lists: {stats["ListCount"]}");
Console.WriteLine($"Tables: {stats["TableCount"]}");
Console.WriteLine($"Max depth: {stats["MaxDepth"]}");
Console.WriteLine($"Word count: {stats["WordCount"]}");
```

### Document Outline

```csharp
var outline = document.GetDocumentOutline();

foreach (var item in outline)
{
    var indent = new string(' ', (item.Level - 1) * 2);
    Console.WriteLine($"{indent}- {item.Title} (Level {item.Level})");
}
```

### Link Analysis

```csharp
var linkAnalysis = document.AnalyzeLinks();

foreach (var link in linkAnalysis)
{
    Console.WriteLine($"Link: {link.Text}");
    Console.WriteLine($"  URL: {link.Url}");
    Console.WriteLine($"  External: {link.IsExternal}");
    Console.WriteLine($"  Relative: {link.IsRelative}");
    Console.WriteLine($"  Anchor: {link.IsAnchor}");
    if (link.Title != null)
        Console.WriteLine($"  Title: {link.Title}");
}
```

## 🔗 Text Content Extraction

Extract text content from selections:

```csharp
// Default space separator
var allText = document.GetTextContent();

// Custom separator
var commaSeparated = document.GetTextContent(", ");

// From specific selections
var headingText = document.GetHeadings().GetTextContent();
var paragraphText = document.GetParagraphs().GetTextContent(" | ");

// Extract from complex selections
var linkTexts = document.Query("link text").GetTextContent();
```

## 🌳 Core Graph Operations

Work with the document's tree structure:

```csharp
var textNode = document.GetTextNodes().First().Get()[0];

// Tree navigation
var parent = document.GetParent(textNode);
var ancestors = document.GetAncestors(textNode);
var descendants = MarkdownDocument.GetDescendants(parent);
var siblings = document.GetSiblings(textNode);
var depth = document.GetDepth(textNode);

// Tree relationships
var allNodes = document.AllNodes;
var rootNode = document.Root;
```

## 🔄 Method Chaining

Chain operations fluently like jQuery:

```csharp
// Complex chaining example
var result = document
    .GetHeadings()                                  // Get all headings
    .Filter(h => ((HeadingNode)h).Level <= 3)      // Only H1-H3
    .Not("h1")                                      // Exclude H1s
    .Parent()                                       // Get their parents
    .Children("paragraph")                          // Find paragraph children
    .Has("link")                                    // That contain links
    .Each((index, node) => {                       // Process each
        Console.WriteLine($"Paragraph {index}: {node.Value}");
    })
    .End()                                          // Return to previous selection
    .Slice(0..5);                                  // Take first 5

// Statistical analysis chain
var linkStats = document
    .GetLinks()
    .Select(link => (LinkNode)link)
    .Where(link => !string.IsNullOrEmpty(link.Url))
    .GroupBy(link => link.Url.StartsWith("http") ? "External" : "Internal")
    .ToDictionary(g => g.Key, g => g.Count());
```

## 🎨 Advanced Examples

### Table of Contents Generation

```csharp
var toc = document.GetHeadings()
    .Select(h => (HeadingNode)h)
    .Select(h => new {
        Level = h.Level,
        Title = h.Value ?? "",
        Anchor = h.Value?.ToLower().Replace(" ", "-") ?? ""
    })
    .ToList();

foreach (var item in toc)
{
    var indent = new string(' ', (item.Level - 1) * 2);
    Console.WriteLine($"{indent}- [{item.Title}](#{item.anchor})");
}
```

### Link Validation

```csharp
var brokenLinks = document.AnalyzeLinks()
    .Where(link => link.IsExternal)
    .Where(link => !IsValidUrl(link.Url))  // Your validation logic
    .ToList();

foreach (var link in brokenLinks)
{
    Console.WriteLine($"Broken link: {link.Text} -> {link.Url}");
}
```

### Content Analysis

```csharp
var analysis = new {
    WordCount = (int)document.GetStatistics()["WordCount"],
    ReadingTime = Math.Ceiling((int)document.GetStatistics()["WordCount"] / 200.0),
    
    Structure = new {
        HasToc = document.GetHeadings().Length > 3,
        HasCodeExamples = document.GetCodeBlocks().Length > 0,
        HasTables = document.GetTables().Length > 0,
        HasImages = document.GetImages().Length > 0
    },
    
    LinkMetrics = new {
        Total = document.GetLinks().Length,
        External = document.AnalyzeLinks().Count(l => l.IsExternal),
        Internal = document.AnalyzeLinks().Count(l => !l.IsExternal && !l.IsAnchor),
        Anchors = document.AnalyzeLinks().Count(l => l.IsAnchor)
    }
};
```

### Document Transformation

```csharp
// Extract all code examples
var codeExamples = document.GetCodeBlocks()
    .Select(cb => (CodeBlockNode)cb)
    .Where(cb => !string.IsNullOrEmpty(cb.Language))
    .GroupBy(cb => cb.Language)
    .ToDictionary(g => g.Key, g => g.Select(cb => cb.Value).ToList());

// Find all TODO items in comments
var todos = document.GetCodeBlocks()
    .SelectMany(cb => cb.Value?.Split('\n') ?? [])
    .Where(line => line.Contains("TODO", StringComparison.OrdinalIgnoreCase))
    .ToList();

// Extract definition lists (heading + paragraph patterns)
var definitions = document.GetHeadings()
    .Where(h => ((HeadingNode)h).Level >= 3)
    .Select(h => new {
        Term = h.Value,
        Definition = h.Next("paragraph").GetTextContent()
    })
    .Where(d => !string.IsNullOrEmpty(d.Definition))
    .ToList();
```

## 🛠️ Supported Markdown Elements

| Element Type | CSS Selector | Type-safe Access | Description |
|--------------|--------------|------------------|-------------|
| Headings | `h1`, `h2`, `h3`, `h4`, `h5`, `h6`, `heading` | `GetHeadings()` | All heading levels |
| Paragraphs | `paragraph`, `p` | `GetParagraphs()` | Text paragraphs |
| Links | `link`, `a` | `GetLinks()` | Hyperlinks |
| Images | `image`, `img` | `GetImages()` | Images |
| Code Blocks | `codeblock`, `pre` | `GetCodeBlocks()` | Fenced and indented code |
| Code Spans | `code` | `GetNodes<CodeSpanNode>()` | Inline code |
| Lists | `list`, `ul`, `ol` | `GetLists()` | Ordered and unordered lists |
| List Items | `li`, `listitem` | `GetNodes<ListItemNode>()` | Individual list items |
| Tables | `table` | `GetTables()` | Table structures |
| Table Rows | `tr` | `GetNodes<TableRowNode>()` | Table rows |
| Table Cells | `td`, `th` | `GetNodes<TableCellNode>()` | Table cells |
| Emphasis | `em`, `emphasis` | `GetNodes<EmphasisNode>()` | Italic text |
| Strong | `strong` | `GetNodes<StrongNode>()` | Bold text |
| Blockquotes | `blockquote` | `GetNodes<QuoteBlockNode>()` | Quote blocks |
| Text | `text` | `GetTextNodes()` | Raw text content |
| Line Breaks | `br` | `GetNodes<HardLineBreakNode>()` | Line breaks |
| Thematic Breaks | `hr`, `thematicbreak` | `GetNodes<ThematicBreakNode>()` | Horizontal rules |

## 📚 API Reference

### Document Creation
- `string.AsQueryable()` 
- `MarkdownDocument.AsQueryable()` 

### Selection Methods
- `Query(string selector)` - CSS selector query
- `GetHeadings(int? level = null)` - Get heading elements
- `GetParagraphs()` - Get paragraph elements
- `GetLinks()` - Get link elements
- `GetImages()` - Get image elements
- `GetCodeBlocks()` - Get code block elements
- `GetLists()` - Get list elements
- `GetTables()` - Get table elements
- `GetTextNodes()` - Get text nodes
- `GetNodes<T>(Func<T, bool>? predicate = null)` - Generic type-based selection

### Filtering Methods
- `Filter(Func<INode, bool> predicate)` - Filter by predicate
- `Filter(string selector)` - Filter by CSS selector
- `Not(Func<INode, bool> predicate)` - Exclude by predicate
- `Not(string selector)` - Exclude by CSS selector
- `Has(Func<INode, bool> predicate)` - Has descendant matching predicate
- `Has(string selector)` - Has descendant matching selector
- `Is(Func<INode, bool> predicate)` - Test if any match predicate
- `Is(string selector)` - Test if any match selector

### Traversal Methods
- `Parent()` - Get parent elements
- `Parents()` - Get all ancestors
- `Closest(Func<INode, bool> predicate)` - Get closest ancestor
- `Children()` - Get child elements
- `Find(string selector)` - Find descendants
- `Siblings()` - Get sibling elements
- `Next()` / `Prev()` - Get adjacent siblings
- `NextAll()` / `PrevAll()` - Get all following/preceding siblings

### Index-based Selection
- `First()` / `Last()` - Get first/last element
- `ElementAt(Index index)` - Get element at index
- `Slice(Range range)` - Get range of elements
- `FirstOrDefault()` / `LastOrDefault()` - Safe access methods

### Transformation Methods
- `Select<T>(Func<INode, T> selector)` - Transform elements
- `Each(Action<INode> action)` - Iterate over elements
- `Get()` - Get underlying node collection
- `GetTextContent(string separator = " ")` - Extract text content

### Analysis Methods
- `GetStatistics()` - Get document statistics
- `GetDocumentStatistics()` - Alias for GetStatistics
- `GetDocumentOutline()` - Get heading-based outline
- `AnalyzeLinks()` - Analyze all links in document

### Graph Operations
- `GetParent(INode node)` - Get parent of specific node
- `GetAncestors(INode node)` - Get ancestors of specific node
- `GetDescendants(INode node)` - Get descendants of specific node (static)
- `GetSiblings(INode node, bool includeSelf = false)` - Get siblings
- `GetDepth(INode node)` - Get node depth in tree

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built on the excellent [Markdig](https://github.com/xoofx/markdig) library
- Inspired by jQuery's fluent API design
- Supports all GitHub Flavored Markdown features

---

**Happy querying! 🎉**
