using System.Collections;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using MarkdigExtensions.Query.Types;

namespace MarkdigExtensions.Query.Core;

/// <summary>
/// Represents a markdown document with DOM-like structure and jQuery-style querying capabilities.
/// Provides a unified interface for traversing, filtering, and manipulating markdown nodes.
/// </summary>
public partial class MarkdownDocument : IEnumerable<INode>
{
    private readonly DocumentNode root;
    private readonly Dictionary<string, List<INode>> selectorIndex;
    private readonly Dictionary<string, List<INode>> attributeIndex;
    private readonly List<INode> allNodes;
    private readonly Stack<MarkdownDocument> previousSelections;
    private readonly ReadOnlyCollection<INode> currentNodes;

    // CSS selector parsing regex patterns
    private static readonly Regex attributeSelectorRegex = AttributeSelectorRegexPattern();
    private static readonly Regex attributeValueRegex = AttributeRegexPattern();

    // Internal constructor for creating filtered document views
    private MarkdownDocument(
        IEnumerable<INode> nodes,
        MarkdownDocument sourceDocument,
        Stack<MarkdownDocument>? previousSelections = null
    )
    {
        this.currentNodes = nodes.ToList().AsReadOnly();
        this.root = sourceDocument.root;
        this.selectorIndex = sourceDocument.selectorIndex;
        this.attributeIndex = sourceDocument.attributeIndex;
        this.allNodes = sourceDocument.allNodes;
        this.previousSelections = previousSelections ?? new Stack<MarkdownDocument>();
    }

    // Primary constructor for creating document from root
    public MarkdownDocument(DocumentNode root)
    {
        this.root = root ?? throw new ArgumentNullException(nameof(root));
        this.selectorIndex = new Dictionary<string, List<INode>>(StringComparer.OrdinalIgnoreCase);
        this.attributeIndex = new Dictionary<string, List<INode>>(StringComparer.OrdinalIgnoreCase);
        this.allNodes = [];
        this.previousSelections = new Stack<MarkdownDocument>();

        this.BuildIndexes();
        this.currentNodes = this.allNodes.AsReadOnly();
    }

    #region Properties

    /// <summary>Gets the root document node.</summary>
    public DocumentNode Root => this.root;

    /// <summary>Gets all nodes in the document.</summary>
    public IReadOnlyList<INode> AllNodes => this.allNodes.AsReadOnly();

    /// <summary>Gets the currently selected nodes.</summary>
    public IReadOnlyList<INode> SelectedNodes => this.currentNodes;

    /// <summary>Gets the total number of nodes in the document.</summary>
    public int Count => this.allNodes.Count;

    /// <summary>Gets the number of currently selected nodes.</summary>
    public int Length => this.currentNodes.Count;

    #endregion

    #region CSS Selector Querying (Integrated QueryEngine functionality)

    /// <summary>Executes a CSS-like selector query against the entire document.</summary>
    public MarkdownDocument Query(string selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);
        var results = this.ExecuteCssQuery(selector);
        return new MarkdownDocument(results, this, this.PushCurrentSelection());
    }

    /// <summary>Creates a new document view with specific nodes selected.</summary>
    public MarkdownDocument Query(IEnumerable<INode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        return new MarkdownDocument(nodes, this, this.PushCurrentSelection());
    }

    /// <summary>Finds the first node matching the CSS selector.</summary>
    public INode? QueryFirst(string selector) => this.ExecuteCssQuery(selector).FirstOrDefault();

    /// <summary>Checks if any nodes in the document match the CSS selector.</summary>
    public bool HasMatch(string selector) => this.ExecuteCssQuery(selector).Any();

    /// <summary>Finds descendants of current selection matching the CSS selector.</summary>
    public MarkdownDocument Find(string selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);
        var descendants = this.currentNodes.SelectMany(GetDescendants).ToList();
        var tempDoc = new MarkdownDocument(new DocumentNode([0], children: descendants));
        var results = tempDoc.ExecuteCssQuery(selector);
        return new MarkdownDocument(results.Distinct(), this, this.PushCurrentSelection());
    }

    private IEnumerable<INode> ExecuteCssQuery(string selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);
        selector = selector.Trim();

        // Handle multiple selectors separated by commas
        if (selector.Contains(','))
        {
            var selectors = selector.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
            return selectors.SelectMany(s => this.ExecuteCssQuery(s)).Distinct();
        }

        // Handle combinators - check for specific combinators first, then fall back to descendant (space)
        if (selector.Contains('>'))
            return this.HandleChildCombinator(selector);
        if (selector.Contains('+'))
            return this.HandleAdjacentSiblingCombinator(selector);
        if (selector.Contains('~'))
            return this.HandleGeneralSiblingCombinator(selector);
        if (selector.Contains(' '))
            return this.HandleDescendantCombinator(selector);

        return this.ExecuteSingleSelector(selector);
    }

    private IEnumerable<INode> ExecuteSingleSelector(string selector)
    {
        var (elementSelector, attributeSelectors, pseudoClass) = ParseSelector(selector);

        IEnumerable<INode> result = !string.IsNullOrEmpty(elementSelector)
            ? elementSelector == "*"
                ? this.allNodes
                : this.QueryBySelector(elementSelector)
            : this.allNodes;

        // Apply attribute filters
        result = attributeSelectors.Aggregate(result, this.ApplyAttributeSelector);

        // Apply pseudo-class filter
        if (!string.IsNullOrEmpty(pseudoClass))
            result = ApplyPseudoClass(result, pseudoClass);

        return result;
    }

    private static (
        string ElementSelector,
        List<string> AttributeSelectors,
        string PseudoClass
    ) ParseSelector(string selector)
    {
        var attributeSelectors = new List<string>();
        var pseudoClass = "";

        // Extract attribute selectors
        var attributeMatches = attributeSelectorRegex.Matches(selector);
        foreach (Match match in attributeMatches)
        {
            attributeSelectors.Add(match.Groups[1].Value);
            selector = selector.Replace(match.Value, "");
        }

        // Extract pseudo-class
        var colonIndex = selector.IndexOf(':');
        if (colonIndex >= 0)
        {
            pseudoClass = selector[colonIndex..];
            selector = selector[..colonIndex];
        }

        return (selector.Trim(), attributeSelectors, pseudoClass);
    }

    private IEnumerable<INode> ApplyAttributeSelector(
        IEnumerable<INode> nodes,
        string attributeSelector
    )
    {
        var match = attributeValueRegex.Match(attributeSelector);
        if (!match.Success)
            return nodes;

        var attributeName = match.Groups[1].Value.Trim();
        var attributeValue = match.Groups[2].Success ? match.Groups[2].Value.Trim('"', '\'') : null;

        return nodes.Where(node =>
        {
            if (!node.Attributes.ContainsKey(attributeName))
                return false;
            if (attributeValue == null)
                return true;
            var nodeValue = node.Attributes[attributeName];
            return string.Equals(nodeValue, attributeValue, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static IEnumerable<INode> ApplyPseudoClass(IEnumerable<INode> nodes, string pseudoClass)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentException.ThrowIfNullOrWhiteSpace(pseudoClass);

        var nodeList = nodes.ToList();
        return pseudoClass.ToLowerInvariant() switch
        {
            ":first" or ":first-child" => nodeList.Take(1),
            ":last" or ":last-child" => nodeList.TakeLast(1),
            ":even" => nodeList.Where((_, index) => index % 2 == 0),
            ":odd" => nodeList.Where((_, index) => index % 2 == 1),
            _ when pseudoClass.StartsWith(":nth-child(") => ApplyNthChild(nodeList, pseudoClass),
            _ when pseudoClass.StartsWith(":nth-of-type(") => ApplyNthOfType(nodeList, pseudoClass),
            _ => nodeList,
        };
    }

    private IEnumerable<INode> HandleDescendantCombinator(string selector) =>
        this.HandleCombinator(selector, ' ', node => GetDescendants(node));

    private IEnumerable<INode> HandleChildCombinator(string selector) =>
        this.HandleCombinator(selector, '>', node => node.Children);

    private IEnumerable<INode> HandleCombinator(
        string selector,
        char combinator,
        Func<INode, IEnumerable<INode>> getRelatedNodes
    )
    {
        var parts = selector.Split(
            combinator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        if (parts.Length < 2)
            return this.ExecuteSingleSelector(selector);

        var sourceElements = this.ExecuteSingleSelector(parts[0]);
        var targetSelector = string.Join(combinator, parts[1..]);

        var results = new List<INode>();

        foreach (var element in sourceElements)
        {
            var relatedNodes = getRelatedNodes(element);
            
            if (combinator == ' ')
            {
                // For descendant combinator, use full CSS query
                var tempDoc = new MarkdownDocument(new DocumentNode([0], children: [.. relatedNodes]));
                results.AddRange(tempDoc.ExecuteCssQuery(targetSelector));
            }
            else
            {
                // For child combinator (>), directly filter the related nodes
                // This avoids the temp document complexity for simple child relationships
                var (elementSelector, attributeSelectors, pseudoClass) = ParseSelector(targetSelector);
                
                IEnumerable<INode> candidateNodes = !string.IsNullOrEmpty(elementSelector)
                    ? elementSelector == "*"
                        ? relatedNodes
                        : relatedNodes.Where(node => 
                            node is Node concreteNode && concreteNode.Selectors.Contains(elementSelector, StringComparer.OrdinalIgnoreCase))
                    : relatedNodes;

                // Apply attribute filters
                candidateNodes = attributeSelectors.Aggregate(candidateNodes, this.ApplyAttributeSelector);

                // Apply pseudo-class filter
                if (!string.IsNullOrEmpty(pseudoClass))
                    candidateNodes = ApplyPseudoClass(candidateNodes, pseudoClass);

                results.AddRange(candidateNodes);
            }
        }

        return results.Distinct();
    }

    private IEnumerable<INode> HandleAdjacentSiblingCombinator(string selector)
    {
        var parts = selector.Split(
            '+',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        if (parts.Length < 2)
            return this.ExecuteSingleSelector(selector);

        var firstElements = this.ExecuteSingleSelector(parts[0]);
        var siblingSelector = parts[1];

        var siblings = new List<INode>();
        foreach (var element in firstElements)
        {
            var elementSiblings = this.GetSiblings(element, true).ToList();
            var elementIndex = elementSiblings.IndexOf(element);

            if (elementIndex >= 0 && elementIndex < elementSiblings.Count - 1)
            {
                var nextSibling = elementSiblings[elementIndex + 1];
                var tempDoc = new MarkdownDocument(new DocumentNode([0], children: [nextSibling]));
                siblings.AddRange(tempDoc.ExecuteSingleSelector(siblingSelector));
            }
        }

        return siblings.Distinct();
    }

    private IEnumerable<INode> HandleGeneralSiblingCombinator(string selector)
    {
        var parts = selector.Split(
            '~',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        if (parts.Length < 2)
        {
            return this.ExecuteSingleSelector(selector);
        }

        var firstElements = this.ExecuteSingleSelector(parts[0]);
        var siblingSelector = parts[1];

        var siblings = new List<INode>();
        foreach (var element in firstElements)
        {
            var elementSiblings = this.GetSiblings(element, true).ToList();
            var elementIndex = elementSiblings.IndexOf(element);

            if (elementIndex >= 0)
            {
                var followingSiblings = elementSiblings.Skip(elementIndex + 1);

                var tempDoc = new MarkdownDocument(
                    new DocumentNode([0], children: [.. followingSiblings])
                );

                siblings.AddRange(tempDoc.ExecuteSingleSelector(siblingSelector));
            }
        }

        return siblings.Distinct();
    }

    private static IEnumerable<INode> ApplyNthChild(List<INode> nodes, string pseudoClass)
    {
        var formula = pseudoClass[12..^1]; // Remove ":nth-child(" and ")"

        if (int.TryParse(formula, out var index))
        {
            return index > 0 && index <= nodes.Count ? [nodes[index - 1]] : [];
        }

        return formula.ToLowerInvariant() switch
        {
            "odd" => nodes.Where((_, i) => (i + 1) % 2 == 1),
            "even" => nodes.Where((_, i) => (i + 1) % 2 == 0),
            _ => ParseNthFormula(nodes, formula),
        };
    }

    private static List<INode> ApplyNthOfType(List<INode> nodes, string pseudoClass)
    {
        var nodesByType = nodes.GroupBy(n => n.Name);
        var result = new List<INode>();

        foreach (var group in nodesByType)
        {
            var typeNodes = group.ToList();
            var formula = pseudoClass[14..^1]; // Remove ":nth-of-type(" and ")"
            result.AddRange(ApplyNthChild(typeNodes, $":nth-child({formula})"));
        }

        return result;
    }

    private static List<INode> ParseNthFormula(List<INode> nodes, string formula)
    {
        var nMatch = ParseNthFormulaPattern().Match(formula);

        if (!nMatch.Success)
        {
            return [];
        }

        var coefficient = string.IsNullOrEmpty(nMatch.Groups[1].Value)
            ? 1
            : int.Parse(nMatch.Groups[1].Value);
        var operation = nMatch.Groups[2].Value;
        var constant = nMatch.Groups[3].Success ? int.Parse(nMatch.Groups[3].Value) : 0;

        if (operation == "-")
            constant = -constant;

        var result = new List<INode>();
        for (int n = 0; ; n++)
        {
            var index = coefficient * n + constant;
            if (index <= 0)
                continue;
            if (index > nodes.Count)
                break;
            result.Add(nodes[index - 1]);
        }

        return result;
    }

    #endregion

    #region Type-based Querying

    /// <summary>Gets all nodes of a specific type with optional filtering.</summary>
    public MarkdownDocument GetNodes<T>(Func<T, bool>? predicate = null)
        where T : class, INode
    {
        var nodes = this.allNodes.OfType<T>();
        if (predicate != null)
            nodes = nodes.Where(predicate);
        return new MarkdownDocument(nodes, this, this.PushCurrentSelection());
    }

    /// <summary>Gets all nodes of a specific type as enumerable with optional filtering.</summary>
    public IEnumerable<T> GetNodesByType<T>(Func<T, bool>? predicate = null)
        where T : class, INode
    {
        var nodes = this.allNodes.OfType<T>();
        return predicate != null ? nodes.Where(predicate) : nodes;
    }

    /// <summary>Gets all heading nodes with optional level filtering.</summary>
    public MarkdownDocument GetHeadings(int? headingLevel = null) =>
        headingLevel.HasValue
            ? this.GetNodes<HeadingNode>(h => h.HeadingLevel == headingLevel.Value)
            : this.GetNodes<HeadingNode>();

    /// <summary>Gets all link nodes.</summary>
    public MarkdownDocument GetLinks() => this.GetNodes<LinkNode>();

    /// <summary>Gets all image nodes.</summary>
    public MarkdownDocument GetImages() => this.GetNodes<ImageNode>();

    /// <summary>Gets all text nodes.</summary>
    public MarkdownDocument GetTextNodes() => this.GetNodes<TextNode>();

    /// <summary>Gets all paragraph nodes.</summary>
    public MarkdownDocument GetParagraphs() => this.GetNodes<ParagraphNode>();

    /// <summary>Gets all code block nodes.</summary>
    public MarkdownDocument GetCodeBlocks() => this.GetNodes<CodeBlockNode>();

    /// <summary>Gets all list nodes.</summary>
    public MarkdownDocument GetLists() => this.GetNodes<ListNode>();

    /// <summary>Gets all table nodes.</summary>
    public MarkdownDocument GetTables() => this.GetNodes<TableNode>();

    #endregion

    #region Filtering & Selection

    /// <summary>Filters current selection using predicate or selector.</summary>
    public MarkdownDocument Filter(Func<INode, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new MarkdownDocument(
            this.currentNodes.Where(predicate),
            this,
            this.PushCurrentSelection()
        );
    }

    public MarkdownDocument Filter(string selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);
        var matchingNodes = this.ExecuteCssQuery(selector).ToHashSet();
        return new MarkdownDocument(
            this.currentNodes.Where(matchingNodes.Contains),
            this,
            this.PushCurrentSelection()
        );
    }

    /// <summary>Excludes nodes from current selection.</summary>
    public MarkdownDocument Not(Func<INode, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new MarkdownDocument(
            this.currentNodes.Where(n => !predicate(n)),
            this,
            this.PushCurrentSelection()
        );
    }

    public MarkdownDocument Not(string selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);
        var excludeNodes = this.ExecuteCssQuery(selector).ToHashSet();
        return new MarkdownDocument(
            this.currentNodes.Where(n => !excludeNodes.Contains(n)),
            this,
            this.PushCurrentSelection()
        );
    }

    public MarkdownDocument Not(IEnumerable<INode> exclusions)
    {
        ArgumentNullException.ThrowIfNull(exclusions);
        var excludeSet = exclusions.ToHashSet();
        return new MarkdownDocument(
            this.currentNodes.Where(n => !excludeSet.Contains(n)),
            this,
            this.PushCurrentSelection()
        );
    }

    /// <summary>Filters nodes that contain matching descendants.</summary>
    public MarkdownDocument Has(Func<INode, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var matching = this.currentNodes.Where(node => GetDescendants(node).Any(predicate));
        return new MarkdownDocument(matching, this, this.PushCurrentSelection());
    }

    public MarkdownDocument Has(string selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);

        var matching = this.currentNodes.Where(node =>
        {
            var descendants = GetDescendants(node);
            var tempDoc = new MarkdownDocument(new DocumentNode([0], children: [.. descendants]));
            return tempDoc.HasMatch(selector);
        });

        return new MarkdownDocument(matching, this, this.PushCurrentSelection());
    }

    /// <summary>Tests if any selected element matches predicate/selector.</summary>
    public bool Is(Func<INode, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return this.currentNodes.Any(predicate);
    }

    public bool Is(string selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);
        var matchingNodes = this.ExecuteCssQuery(selector).ToHashSet();
        return this.currentNodes.Any(matchingNodes.Contains);
    }

    #endregion

    #region Traversal Methods

    /// <summary>Gets parent elements of current selection.</summary>
    public MarkdownDocument Parent(Func<INode, bool>? filter = null) =>
        this.TraverseRelated(node => new[] { this.GetParent(node) }.Where(p => p != null)!, filter);

    public MarkdownDocument Parent(string selector) =>
        this.Parent(this.ExecuteCssQuery(selector).ToHashSet().Contains);

    /// <summary>Gets ancestor elements of current selection.</summary>
    public MarkdownDocument Parents(Func<INode, bool>? filter = null) =>
        this.TraverseRelated(this.GetAncestors, filter);

    public MarkdownDocument Parents(string selector) =>
        this.Parents(this.ExecuteCssQuery(selector).ToHashSet().Contains);

    /// <summary>Gets ancestors until stop condition.</summary>
    public MarkdownDocument ParentsUntil(
        Func<INode, bool> stopPredicate,
        Func<INode, bool>? filter = null
    ) => this.TraverseUntil(this.GetAncestors, stopPredicate, filter);

    /// <summary>Gets closest ancestor (including self) matching condition.</summary>
    public MarkdownDocument Closest(Func<INode, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var closest = this
            .currentNodes.Select(node =>
                predicate(node) ? node : this.GetAncestors(node).FirstOrDefault(predicate)
            )
            .Where(n => n != null)
            .Cast<INode>();
        return new MarkdownDocument(closest.Distinct(), this, this.PushCurrentSelection());
    }

    public MarkdownDocument Closest(string selector) =>
        this.Closest(this.ExecuteCssQuery(selector).ToHashSet().Contains);

    /// <summary>Gets child elements of current selection.</summary>
    public MarkdownDocument Children(Func<INode, bool>? filter = null) =>
        this.TraverseRelated(node => node.Children, filter);

    public MarkdownDocument Children(string selector) =>
        this.Children(this.ExecuteCssQuery(selector).ToHashSet().Contains);

    /// <summary>Gets all child elements (alias for Children).</summary>
    public MarkdownDocument Contents() => this.Children();

    /// <summary>Gets sibling elements of current selection.</summary>
    public MarkdownDocument Siblings(Func<INode, bool>? filter = null) =>
        this.TraverseRelated(node => this.GetSiblings(node, false), filter);

    public MarkdownDocument Siblings(string selector) =>
        this.Siblings(this.ExecuteCssQuery(selector).ToHashSet().Contains);

    /// <summary>Gets next/previous siblings.</summary>
    public MarkdownDocument Next(Func<INode, bool>? filter = null) =>
        this.GetDirectionalSibling(1, filter);

    public MarkdownDocument Next(string selector) =>
        this.Next(this.ExecuteCssQuery(selector).ToHashSet().Contains);

    public MarkdownDocument Prev(Func<INode, bool>? filter = null) =>
        this.GetDirectionalSibling(-1, filter);

    public MarkdownDocument Prev(string selector) =>
        this.Prev(this.ExecuteCssQuery(selector).ToHashSet().Contains);

    /// <summary>Gets all following/preceding siblings.</summary>
    public MarkdownDocument NextAll(Func<INode, bool>? filter = null) =>
        this.GetRangeSiblings(1, int.MaxValue, filter);

    public MarkdownDocument NextAll(string selector) =>
        this.NextAll(this.ExecuteCssQuery(selector).ToHashSet().Contains);

    public MarkdownDocument PrevAll(Func<INode, bool>? filter = null) =>
        this.GetRangeSiblings(-int.MaxValue, -1, filter);

    public MarkdownDocument PrevAll(string selector) =>
        this.PrevAll(this.ExecuteCssQuery(selector).ToHashSet().Contains);

    /// <summary>Gets following/preceding siblings until stop condition.</summary>
    public MarkdownDocument NextUntil(
        Func<INode, bool> stopPredicate,
        Func<INode, bool>? filter = null
    ) => this.GetSiblingsUntil(1, stopPredicate, filter);

    public MarkdownDocument NextUntil(string stopSelector, string? filter = null) =>
        this.GetSiblingsUntilSelector(1, stopSelector, filter);

    public MarkdownDocument PrevUntil(
        Func<INode, bool> stopPredicate,
        Func<INode, bool>? filter = null
    ) => this.GetSiblingsUntil(-1, stopPredicate, filter);

    public MarkdownDocument PrevUntil(string stopSelector, string? filter = null) =>
        this.GetSiblingsUntilSelector(-1, stopSelector, filter);

    #endregion

    #region Index-based Selection

    /// <summary>Gets element at specified index.</summary>
    public MarkdownDocument ElementAt(Index index)
    {
        if (this.currentNodes.Count == 0)
            return new MarkdownDocument([], this, this.PushCurrentSelection());
        try
        {
            var element = this.currentNodes[index];
            return new MarkdownDocument([element], this, this.PushCurrentSelection());
        }
        catch (ArgumentOutOfRangeException)
        {
            return new MarkdownDocument([], this, this.PushCurrentSelection());
        }
    }

    public MarkdownDocument First() => this.ElementAt(0);

    public MarkdownDocument Last() => this.ElementAt(^1);

    /// <summary>Gets subset of elements in range.</summary>
    public MarkdownDocument Slice(Range range)
    {
        if (this.currentNodes.Count == 0)
            return new MarkdownDocument([], this, this.PushCurrentSelection());
        try
        {
            var (start, length) = range.GetOffsetAndLength(this.currentNodes.Count);
            var slice = this.currentNodes.Skip(start).Take(length);
            return new MarkdownDocument(slice, this, this.PushCurrentSelection());
        }
        catch (ArgumentOutOfRangeException)
        {
            return new MarkdownDocument([], this, this.PushCurrentSelection());
        }
    }

    public MarkdownDocument Slice(int start, int? end = null) =>
        this.Slice(start..(end ?? this.currentNodes.Count));

    #endregion

    #region Set Operations

    /// <summary>Adds elements to current selection.</summary>
    public MarkdownDocument Add(IEnumerable<INode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        return new MarkdownDocument(
            this.currentNodes.Concat(nodes).Distinct(),
            this,
            this.PushCurrentSelection()
        );
    }

    public MarkdownDocument Add(string selector) => this.Add(this.ExecuteCssQuery(selector));

    public MarkdownDocument Add(Func<INode, bool> predicate) =>
        this.Add(this.allNodes.Where(predicate));

    /// <summary>Returns to previous selection in chain.</summary>
    public MarkdownDocument End() =>
        this.previousSelections.Count == 0 ? this : this.previousSelections.Pop();

    #endregion

    #region Transformation & Utility Operations

    /// <summary>Executes action for each selected element.</summary>
    public MarkdownDocument Each(Action<int, INode> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        for (int i = 0; i < this.currentNodes.Count; i++)
            action(i, this.currentNodes[i]);
        return this;
    }

    public MarkdownDocument Each(Action<INode> action) => this.Each((_, node) => action(node));

    /// <summary>Transforms selected elements into a new enumerable for LINQ operations.</summary>
    public IEnumerable<T> Select<T>(Func<INode, T> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return this.currentNodes.Select(selector);
    }

    /// <summary>Transforms selected elements with index into a new enumerable for LINQ operations.</summary>
    public IEnumerable<T> Select<T>(Func<int, INode, T> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return this.currentNodes.Select((node, index) => selector(index, node));
    }

    /// <summary>Gets selected nodes or single element.</summary>
    public IReadOnlyList<INode> Get() => this.currentNodes;

    public INode Get(Index index) => this.currentNodes[index];

    public INode? FirstOrDefault() => this.currentNodes.Count > 0 ? this.currentNodes[0] : null;

    public INode? LastOrDefault() => this.currentNodes.Count > 0 ? this.currentNodes[^1] : null;

    /// <summary>Gets all text content from selected nodes as a single string.</summary>
    public string GetTextContent(string separator = " ")
    {
        var textNodes = this.currentNodes.OfType<TextNode>();
        return string.Join(
            separator,
            textNodes.Select(t => t.Value).Where(v => !string.IsNullOrEmpty(v))
        );
    }

    /// <summary>Gets statistics about the current selection.</summary>
    public Dictionary<string, object> GetStatistics() =>
        new()
        {
            ["TotalNodes"] = this.Count,
            ["SelectedNodes"] = this.Length,
            ["HeadingCount"] = this.currentNodes.OfType<HeadingNode>().Count(),
            ["ParagraphCount"] = this.currentNodes.OfType<ParagraphNode>().Count(),
            ["LinkCount"] = this.currentNodes.OfType<LinkNode>().Count(),
            ["ImageCount"] = this.currentNodes.OfType<ImageNode>().Count(),
            ["CodeBlockCount"] = this.currentNodes.OfType<CodeBlockNode>().Count(),
            ["ListCount"] = this.currentNodes.OfType<ListNode>().Count(),
            ["TableCount"] = this.currentNodes.OfType<TableNode>().Count(),
            ["MaxDepth"] = this.currentNodes.Select(this.GetDepth).DefaultIfEmpty(0).Max(),
            ["WordCount"] = EstimateWordCount(this.GetTextContent()),
        };

    private static int EstimateWordCount(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;

    #endregion

    #region Core Graph Methods

    /// <summary>Gets all descendant nodes of the specified node.</summary>
    public static IEnumerable<INode> GetDescendants(INode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        foreach (var child in node.Children)
        {
            yield return child;

            foreach (var descendant in GetDescendants(child))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>Gets all ancestor nodes of the specified node.</summary>
    public IEnumerable<INode> GetAncestors(INode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        var parent = this.GetParent(node);
        while (parent != null)
        {
            yield return parent;
            parent = this.GetParent(parent);
        }
    }

    /// <summary>Gets the immediate parent of the specified node.</summary>
    public INode? GetParent(INode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node == this.root ? null : FindParent(this.root, node);
    }

    /// <summary>Gets sibling nodes of the specified node.</summary>
    public IEnumerable<INode> GetSiblings(INode node, bool includeSelf = false)
    {
        ArgumentNullException.ThrowIfNull(node);
        var parent = this.GetParent(node);
        if (parent == null)
            return includeSelf ? [node] : [];
        return includeSelf ? parent.Children : parent.Children.Where(s => s != node);
    }

    /// <summary>Gets the depth of a node in the document.</summary>
    public int GetDepth(INode node) => node == this.root ? 0 : this.GetAncestors(node).Count();

    #endregion

    #region IEnumerable Implementation

    public IEnumerator<INode> GetEnumerator() => this.currentNodes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    #endregion

    #region Helper Methods

    private Stack<MarkdownDocument> PushCurrentSelection()
    {
        var newStack = new Stack<MarkdownDocument>(this.previousSelections.Reverse());
        newStack.Push(this);
        return newStack;
    }

    private MarkdownDocument TraverseRelated(
        Func<INode, IEnumerable<INode>> getRelated,
        Func<INode, bool>? filter
    )
    {
        var related = this.currentNodes.SelectMany(getRelated).Distinct();
        if (filter != null)
            related = related.Where(filter);
        return new MarkdownDocument(related, this, this.PushCurrentSelection());
    }

    private MarkdownDocument TraverseUntil(
        Func<INode, IEnumerable<INode>> getRelated,
        Func<INode, bool> stopPredicate,
        Func<INode, bool>? filter
    )
    {
        var results = new List<INode>();
        foreach (var node in this.currentNodes)
        {
            foreach (var related in getRelated(node))
            {
                if (stopPredicate(related))
                    break;
                results.Add(related);
            }
        }
        var distinct = results.Distinct();
        if (filter != null)
            distinct = distinct.Where(filter);
        return new MarkdownDocument(distinct, this, this.PushCurrentSelection());
    }

    private MarkdownDocument GetDirectionalSibling(int direction, Func<INode, bool>? filter)
    {
        var siblings = new List<INode>();
        foreach (var node in this.currentNodes)
        {
            var allSiblings = this.GetSiblings(node, true).ToList();
            var currentIndex = allSiblings.IndexOf(node);
            var targetIndex = currentIndex + direction;

            if (targetIndex >= 0 && targetIndex < allSiblings.Count)
            {
                var sibling = allSiblings[targetIndex];
                if (filter == null || filter(sibling))
                    siblings.Add(sibling);
            }
        }
        return new MarkdownDocument(siblings.Distinct(), this, this.PushCurrentSelection());
    }

    private MarkdownDocument GetRangeSiblings(
        int startOffset,
        int endOffset,
        Func<INode, bool>? filter
    )
    {
        var siblings = new List<INode>();
        foreach (var node in this.currentNodes)
        {
            var allSiblings = this.GetSiblings(node, true).ToList();
            var currentIndex = allSiblings.IndexOf(node);

            var start = Math.Max(0, currentIndex + Math.Min(startOffset, 0));
            var end = Math.Min(allSiblings.Count - 1, currentIndex + Math.Max(endOffset, 0));

            for (int i = start; i <= end; i++)
            {
                if (i != currentIndex)
                    siblings.Add(allSiblings[i]);
            }
        }

        var result = siblings.Distinct();
        if (filter != null)
            result = result.Where(filter);
        return new MarkdownDocument(result, this, this.PushCurrentSelection());
    }

    private MarkdownDocument GetSiblingsUntil(
        int direction,
        Func<INode, bool> stopPredicate,
        Func<INode, bool>? filter
    )
    {
        ArgumentNullException.ThrowIfNull(stopPredicate);

        var siblings = new List<INode>();
        foreach (var node in this.currentNodes)
        {
            var allSiblings = this.GetSiblings(node, true).ToList();
            var currentIndex = allSiblings.IndexOf(node);

            if (direction > 0) // Next
            {
                for (int i = currentIndex + 1; i < allSiblings.Count; i++)
                {
                    var sibling = allSiblings[i];
                    if (stopPredicate(sibling))
                        break;
                    siblings.Add(sibling);
                }
            }
            else // Prev
            {
                for (int i = currentIndex - 1; i >= 0; i--)
                {
                    var sibling = allSiblings[i];
                    if (stopPredicate(sibling))
                        break;
                    siblings.Add(sibling);
                }
            }
        }

        var result = siblings.Distinct();
        if (filter != null)
            result = result.Where(filter);
        return new MarkdownDocument(result, this, this.PushCurrentSelection());
    }

    private MarkdownDocument GetSiblingsUntilSelector(
        int direction,
        string stopSelector,
        string? filter
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stopSelector);

        var stopNodes = this.ExecuteCssQuery(stopSelector).ToHashSet();

        Func<INode, bool>? filterPredicate = null;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var filterNodes = this.ExecuteCssQuery(filter).ToHashSet();

            filterPredicate = filterNodes.Contains;
        }

        return this.GetSiblingsUntil(direction, stopNodes.Contains, filterPredicate);
    }

    private void BuildIndexes()
    {
        foreach (var node in this.DepthFirstTraversal())
        {
            this.allNodes.Add(node);

            // Index by selectors
            if (node is Node concreteNode)
            {
                foreach (var selector in concreteNode.Selectors)
                {
                    if (!this.selectorIndex.TryGetValue(selector, out List<INode>? value))
                    {
                        value = [];
                        this.selectorIndex[selector] = value;
                    }

                    value.Add(node);
                }
            }

            // Index by attributes
            foreach (var attribute in node.Attributes)
            {
                var attributeKey = attribute.Key;
                var attributeValueKey = $"{attribute.Key}={attribute.Value}";

                if (!this.attributeIndex.TryGetValue(attributeKey, out List<INode>? value))
                {
                    value = [];
                    this.attributeIndex[attributeKey] = value;
                }

                value.Add(node);

                if (attribute.Value != null)
                {
                    if (
                        !this.attributeIndex.TryGetValue(attributeValueKey, out List<INode>? value2)
                    )
                    {
                        value2 = [];
                        this.attributeIndex[attributeValueKey] = value2;
                    }

                    value2.Add(node);
                }
            }
        }
    }

    private IEnumerable<INode> DepthFirstTraversal(INode? startNode = null)
    {
        var start = startNode ?? this.root;

        yield return start;

        foreach (var child in start.Children)
        {
            foreach (var node in this.DepthFirstTraversal(child))
            {
                yield return node;
            }
        }
    }

    private static INode? FindParent(INode current, INode target)
    {
        foreach (var child in current.Children)
        {
            if (child == target)
            {
                return current;
            }

            var found = FindParent(child, target);

            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private List<INode> QueryBySelector(string selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);
        return this.selectorIndex.TryGetValue(selector, out var nodes) ? nodes : [];
    }

    [GeneratedRegex(@"^([^=]+)(?:=(.+))?$", RegexOptions.Compiled)]
    private static partial Regex AttributeRegexPattern();

    [GeneratedRegex(@"\[([^\]]+)\]", RegexOptions.Compiled)]
    private static partial Regex AttributeSelectorRegexPattern();

    [GeneratedRegex(@"^(\d*)n(?:([+-])(\d+))?$")]
    private static partial Regex ParseNthFormulaPattern();

    #endregion
}
