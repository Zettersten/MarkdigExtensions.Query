# Welcome to the Test Suite

This document is designed to test all the features of your Markdown query engine.  
It includes examples of every supported selector, attribute, and combinator.

---

## 🔗 Links and Images

This is a [regular link](https://example.com "Example").

This is another [inline link](https://another.com) with no title.

![An image](https://media.veefriends.com/image/upload/v1700083094/veefriends/specials/series2/characters/reflective-rhinoceros-manifesting-shrinkwrapped.png "Rhino")

---

## 🅱️ Bold and *Italic* (Emphasis)

This line has **bold text** and *italic text* inside a paragraph.

> This blockquote contains **nested bold** and *nested italic*.

---

## 📋 Lists

### Unordered List

- Item 1
  - Nested Item 1.1
  - Nested Item 1.2 with a [link](https://nested.com)
- Item 2

### Ordered List

1. First item
2. Second item with an [ordered link](https://ordered.com)

---

## 🧱 Code Blocks

```csharp
// This is a fenced code block
Console.WriteLine("Hello from C#");
````

```js
// JavaScript example
alert("Hi!");
```

---

## 🏛️ Blockquote

> This is a blockquote with some *italic text* and a **bold statement**.

---

## 🔠 Headings

### Heading Level 2

Some text inside this H2. Also includes *italic text* and **bold text**.

![Image inside heading](https://example.com/head-img.png)

---

## 🧮 Table

| Name      | Value    |
| --------- | -------- |
| Bold Cell | **BOLD** |
| Italic    | *italic* |

---

## 🔥 Deep Nesting

* Outer UL

  * Inner UL

    * Paragraph with [deep link](https://deep.com)

> Quote with list:
>
> * Item in blockquote
> * Another item with *style*

---

## 🧪 HTML Block

<div>This is a raw HTML block</div>

---

## 🧹 Horizontal Rule

---

````

---

## ✅ What This Covers

| Feature                      | Example Line(s)                      |
|------------------------------|--------------------------------------|
| `link`                       | Regular, inline, nested              |
| `image`                      | Inline and inside heading            |
| `strong`, `emphasis`         | Inline, nested, inside heading       |
| `ul`, `ol`, `list`           | Flat, nested, inside blockquote      |
| `blockquote`                 | With styling and nested elements     |
| `heading[level=2]`, `h2`     | Includes nested styles & images      |
| `codeblock[language=csharp]`| C# fenced block                       |
| `table`                      | With styled cell contents            |
| `html`                       | Raw `<div>`                          |
| `thematicbreak`              | Horizontal rule                      |
| Descendant: `heading emphasis` | Bold/italic inside heading         |
| Child: `ul > link`           | Link directly inside UL item         |
| OR: `link, image`            | All hyperlinks and images            |
