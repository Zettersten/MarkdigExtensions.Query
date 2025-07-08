
![Markdig.Query Logo](https://raw.githubusercontent.com/Zettersten/Markdig.Query/main/icon.png)

# Markdig.Query 📄🔍

[![NuGet version](https://badge.fury.io/nu/Markdig.Query.svg)](https://badge.fury.io/nu/Markdig.Query)

**Markdig.Query** is a powerful, jQuery-style query engine for traversing, filtering, and manipulating Markdown documents in .NET. Built on top of the [Markdig](https://github.com/lunet-io/markdig) Markdown parser, it provides an expressive API for searching and transforming Markdown nodes with CSS-like selectors and familiar LINQ-style chaining.

---

## 🚀 Features

* 🔎 **CSS-style Selectors**: Query blocks and inlines with tags (`h1`, `link`, `table`), attributes, wildcards, and combinators.
* 🧩 **Chaining & Filtering**: Use `.Where()`, `.Select()`, `.NextUntil()`, and more for precise control.
* ⚡ **Pseudo-Selectors**: Supports `first-child`, `last-child`, `even`, `odd`, `nth-child(n)`.
* 💬 **Text & HTML Access**: Easily get `.InnerText`, `.InnerHtml`, `.ToMarkdown()` from any node.
* 🔧 **Node Metadata**: Use `.Attr()` to inspect node details like `href`, `level`, or `isImage`.
* 🧵 **Sibling & Linear Traversals**: Navigate using `NextUntil`, `PrevUntil`, or `Between`.

---

## 📦 Installation

Install from NuGet:

```bash
dotnet add package Markdig.Query
```

---

## 🛠️ Getting Started

```csharp
using Markdig;
using Markdig.Query;

// Parse a Markdown document
var markdown = """
# Welcome to the Test Suite

This document is **bold** and ![an image](img.png).

---

## Next Section
""";

var doc = Markdown.Parse(markdown);

// Query all heading blocks
var headings = doc.QueryBlocks("h1, h2");

// Get text from the first heading
var title = headings.First().InnerText; // "Welcome to the Test Suite"

// Find all paragraphs between H1 and HR
var section = doc.QueryBlock("h1")?.NextUntil("hr");
```

---

## 🔍 Selector Syntax

| Selector           | Description                                  |
| ------------------ | -------------------------------------------- |
| `h1`               | Matches all level 1 headings                 |
| `link[href]`       | Matches links with an href attribute         |
| `heading > strong` | Matches strong text directly inside headings |
| `image, link`      | Matches either image or link elements        |
| `*`                | Matches any block or inline node             |

### Pseudo-classes:

* `first-child`
* `last-child`
* `even`
* `odd`
* `nth-child(n)`

---

## 🧠 API Overview

### Query Methods

```csharp
document.QueryBlocks("selector")
```

Returns `IEnumerable<MarkdownNode>`

---

### MarkdownNode Access

```csharp
node.InnerText       // Clean text content
node.InnerHtml       // HTML rendering
node.ToMarkdown()    // Original markdown
node.Attr("href")    // e.g., link URL
node.Attr()          // Dictionary of attributes
```

---

### Traversal

```csharp
node.NextUntil("selector", includeStart: false, includeEnd: false, blockOnly: true)
node.PrevUntil("selector")
node.Between(startNode, endNode)
```

---

## 🔧 Advanced Examples

### Group all paragraphs between two headings

```csharp
var intro = doc
    .QueryBlock("h1")
    ?.NextUntil("h2", includeStart: false, includeEnd: false);
```

### Select all emphasized text inside a paragraph

```csharp
var emphasis = doc.QueryBlock("paragraph emphasis");
```

### Wildcard + pseudo-class combo

```csharp
var firstOfEach = doc.QueryBlock("*:first-child");
```

---

## ✅ Use Cases

* Build markdown content editors or analyzers
* Wrap sections between headings
* Validate document structure
* Extract or transform specific content programmatically

---

## 📄 Requirements

* **.NET 6.0 or higher**
* Works with standard `Markdig.MarkdownDocument`

---

## 🤝 Contributing

Pull requests and issues are welcome!
If you’d like to contribute a new selector, operator, or traversal pattern, open a GitHub issue to propose the API shape first.

---

## 🔗 Links

* GitHub: [Zettersten/Markdig.Query](https://github.com/Zettersten/Markdig.Query)
* NuGet: [Markdig.Query on NuGet](https://www.nuget.org/packages/Markdig.Query)

---

Thanks for using **Markdig.Query**!
Enjoy writing powerful, expressive markdown transformations — just like jQuery for documents.
