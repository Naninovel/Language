using Naninovel.Metadata;
using Xunit;

namespace Naninovel.Language.Test;

public class SymbolTest
{
    private readonly Project meta = new();

    [Fact]
    public void ResultCountEqualLineCount ()
    {
        Assert.Single(GetSymbols(""));
        Assert.Equal(2, GetSymbols("\n").Length);
        Assert.Equal(4, GetSymbols("@cmd p:{v}\nk: [c]\n#\n;").Length);
    }

    [Fact]
    public void RangeHaveCorrectLineIndex ()
    {
        var symbols = GetSymbols("\n");
        Assert.Equal(0, symbols[0].Range.Start.Line);
        Assert.Equal(0, symbols[0].Range.End.Line);
        Assert.Equal(1, symbols[1].Range.Start.Line);
        Assert.Equal(1, symbols[1].Range.End.Line);
    }

    [Fact]
    public void SelectionRangeEqualRange ()
    {
        var symbols = GetSymbols("@cmd\n# label");
        Assert.Equal(symbols[0].Range, symbols[0].SelectionRange);
        Assert.Equal(symbols[1].Range, symbols[1].SelectionRange);
    }

    [Fact]
    public void EmptyLineSymbolsAraValid ()
    {
        var symbol = GetSymbol("");
        Assert.Equal("EmptyLine", symbol.Name);
        Assert.Equal(SymbolKind.Null, symbol.Kind);
        Assert.Equal(Range.Empty, symbol.Range);
    }

    [Fact]
    public void LabelLineSymbolsAraValid ()
    {
        var symbol = GetSymbol("# label");
        Assert.Equal("LabelLine", symbol.Name);
        Assert.Equal(SymbolKind.Namespace, symbol.Kind);
        Assert.Equal(new Range(new(0, 0), new(0, 7)), symbol.Range);
        Assert.Single(symbol.Children);
        Assert.Equal("LabelText", symbol.Children[0].Name);
        Assert.Equal(SymbolKind.String, symbol.Children[0].Kind);
        Assert.Equal(new Range(new(0, 2), new(0, 7)), symbol.Children[0].Range);
    }

    [Fact]
    public void CommentLineSymbolsAraValid ()
    {
        var symbol = GetSymbol("; comment");
        Assert.Equal("CommentLine", symbol.Name);
        Assert.Equal(SymbolKind.String, symbol.Kind);
        Assert.Equal(new Range(new(0, 0), new(0, 9)), symbol.Range);
        Assert.Single(symbol.Children);
        Assert.Equal("CommentText", symbol.Children[0].Name);
        Assert.Equal(SymbolKind.String, symbol.Children[0].Kind);
        Assert.Equal(new Range(new(0, 2), new(0, 9)), symbol.Children[0].Range);
    }

    [Fact]
    public void CommandLineSymbolsAraValid ()
    {
        var symbol = GetSymbol("@cmd nameless param:{exp}");
        Assert.Equal("CommandLine", symbol.Name);
        Assert.Equal(SymbolKind.Struct, symbol.Kind);
        Assert.Equal(new Range(new(0, 0), new(0, 25)), symbol.Range);
        Assert.Single(symbol.Children);
        Assert.Equal("Command", symbol.Children[0].Name);
        Assert.Equal(SymbolKind.Function, symbol.Children[0].Kind);
        Assert.Equal(new Range(new(0, 1), new(0, 25)), symbol.Children[0].Range);
        Assert.Equal(3, symbol.Children[0].Children!.Length);
        Assert.Equal("CommandIdentifier", symbol.Children[0].Children[0].Name);
        Assert.Equal(SymbolKind.Key, symbol.Children[0].Children[0].Kind);
        Assert.Equal(new Range(new(0, 1), new(0, 4)), symbol.Children[0].Children[0].Range);
        Assert.Equal("Parameter", symbol.Children[0].Children[1].Name);
        Assert.Equal(SymbolKind.Field, symbol.Children[0].Children[1].Kind);
        Assert.Equal(new Range(new(0, 5), new(0, 13)), symbol.Children[0].Children[1].Range);
        Assert.Equal("Parameter", symbol.Children[0].Children[2].Name);
        Assert.Equal(SymbolKind.Field, symbol.Children[0].Children[2].Kind);
        Assert.Equal(new Range(new(0, 14), new(0, 25)), symbol.Children[0].Children[2].Range);
        Assert.Single(symbol.Children[0].Children[1].Children);
        Assert.Equal("ParameterValue", symbol.Children[0].Children[1].Children[0].Name);
        Assert.Equal(SymbolKind.String, symbol.Children[0].Children[1].Children[0].Kind);
        Assert.Equal(new Range(new(0, 5), new(0, 13)), symbol.Children[0].Children[1].Children[0].Range);
        Assert.Equal(2, symbol.Children[0].Children[2].Children!.Length);
        Assert.Equal("ParameterIdentifier", symbol.Children[0].Children[2].Children[0].Name);
        Assert.Equal(SymbolKind.Key, symbol.Children[0].Children[2].Children[0].Kind);
        Assert.Equal(new Range(new(0, 14), new(0, 19)), symbol.Children[0].Children[2].Children[0].Range);
        Assert.Equal("ParameterValue", symbol.Children[0].Children[2].Children[1].Name);
        Assert.Equal(SymbolKind.String, symbol.Children[0].Children[2].Children[1].Kind);
        Assert.Equal(new Range(new(0, 20), new(0, 25)), symbol.Children[0].Children[2].Children[1].Range);
        Assert.Single(symbol.Children[0].Children[2].Children[1].Children);
        Assert.Equal("Expression", symbol.Children[0].Children[2].Children[1].Children[0].Name);
        Assert.Equal(SymbolKind.Property, symbol.Children[0].Children[2].Children[1].Children[0].Kind);
        Assert.Equal(new Range(new(0, 20), new(0, 25)), symbol.Children[0].Children[2].Children[1].Children[0].Range);
    }

    [Fact]
    public void GenericLineSymbolsAraValid ()
    {
        var symbol = GetSymbol("author.appearance: [cmd] text {exp}");
        Assert.Equal("GenericTextLine", symbol.Name);
        Assert.Equal(SymbolKind.String, symbol.Kind);
        Assert.Equal(new Range(new(0, 0), new(0, 35)), symbol.Range);
        Assert.Equal(3, symbol.Children!.Length);
        Assert.Equal("GenericTextPrefix", symbol.Children[0].Name);
        Assert.Equal(SymbolKind.Constant, symbol.Children[0].Kind);
        Assert.Equal(new Range(new(0, 0), new(0, 18)), symbol.Children[0].Range);
        Assert.Equal("InlinedCommand", symbol.Children[1].Name);
        Assert.Equal(SymbolKind.Struct, symbol.Children[1].Kind);
        Assert.Equal(new Range(new(0, 19), new(0, 24)), symbol.Children[1].Range);
        Assert.Equal("GenericText", symbol.Children[2].Name);
        Assert.Equal(SymbolKind.String, symbol.Children[2].Kind);
        Assert.Equal(new Range(new(0, 24), new(0, 35)), symbol.Children[2].Range);
        Assert.Single(symbol.Children[2].Children);
        Assert.Equal("Expression", symbol.Children[2].Children[0].Name);
        Assert.Equal(SymbolKind.Property, symbol.Children[2].Children[0].Kind);
        Assert.Equal(new Range(new(0, 30), new(0, 35)), symbol.Children[2].Children[0].Range);
        Assert.Equal(2, symbol.Children[0].Children!.Length);
        Assert.Equal("GenericTextAuthor", symbol.Children[0].Children[0].Name);
        Assert.Equal(SymbolKind.Key, symbol.Children[0].Children[0].Kind);
        Assert.Equal(new Range(new(0, 0), new(0, 6)), symbol.Children[0].Children[0].Range);
        Assert.Equal("GenericTextAuthorAppearance", symbol.Children[0].Children[1].Name);
        Assert.Equal(SymbolKind.Enum, symbol.Children[0].Children[1].Kind);
        Assert.Equal(new Range(new(0, 7), new(0, 17)), symbol.Children[0].Children[1].Range);
        Assert.Single(symbol.Children[1].Children);
        Assert.Equal("Command", symbol.Children[1].Children[0].Name);
        Assert.Equal(SymbolKind.Function, symbol.Children[1].Children[0].Kind);
        Assert.Equal(new Range(new(0, 20), new(0, 23)), symbol.Children[1].Children[0].Range);
        Assert.Single(symbol.Children[1].Children[0].Children);
        Assert.Equal("CommandIdentifier", symbol.Children[1].Children[0].Children[0].Name);
        Assert.Equal(SymbolKind.Key, symbol.Children[1].Children[0].Children[0].Kind);
        Assert.Equal(new Range(new(0, 20), new(0, 23)), symbol.Children[1].Children[0].Children[0].Range);
    }

    [Fact]
    public void InlinedCommandSymbolsAraValid ()
    {
        var symbol = GetSymbol("[i]");

        Assert.Equal("GenericTextLine", symbol.Name);
        Assert.Equal(new Range(new(0, 0), new(0, 3)), symbol.Range);
        Assert.Single(symbol.Children);

        Assert.Equal("InlinedCommand", symbol.Children[0].Name);
        Assert.Equal(new Range(new(0, 0), new(0, 3)), symbol.Children[0].Range);
        Assert.Single(symbol.Children[0].Children);

        Assert.Equal("Command", symbol.Children[0].Children[0].Name);
        Assert.Equal(new Range(new(0, 1), new(0, 2)), symbol.Children[0].Children[0].Range);
        Assert.Single(symbol.Children[0].Children[0].Children);

        Assert.Equal("CommandIdentifier", symbol.Children[0].Children[0].Children[0].Name);
        Assert.Equal(new Range(new(0, 1), new(0, 2)), symbol.Children[0].Children[0].Children[0].Range);
    }

    [Fact]
    public void SymbolKindOfParameterValueCorrespondToValueType ()
    {
        var parameters = new[] {
            new Parameter { Id = "str", ValueType = ValueType.String },
            new Parameter { Id = "int", ValueType = ValueType.Integer },
            new Parameter { Id = "dec", ValueType = ValueType.Decimal },
            new Parameter { Id = "bool", ValueType = ValueType.Boolean },
            new Parameter { Id = "list", ValueContainerType = ValueContainerType.List },
            new Parameter { Id = "named", ValueContainerType = ValueContainerType.NamedList }
        };
        meta.Commands = new[] { new Command { Id = "c", Parameters = parameters } };
        Assert.Equal(SymbolKind.String, GetSymbol("@c str:x").Children![0].Children![1].Children![1].Kind);
        Assert.Equal(SymbolKind.Number, GetSymbol("@c int:x").Children![0].Children![1].Children![1].Kind);
        Assert.Equal(SymbolKind.Number, GetSymbol("@c dec:x").Children![0].Children![1].Children![1].Kind);
        Assert.Equal(SymbolKind.Boolean, GetSymbol("@c bool:true").Children![0].Children![1].Children![1].Kind);
        Assert.Equal(SymbolKind.Array, GetSymbol("@c list:,").Children![0].Children![1].Children![1].Kind);
        Assert.Equal(SymbolKind.Array, GetSymbol("@c named:.").Children![0].Children![1].Children![1].Kind);
        Assert.Equal(SymbolKind.String, GetSymbol("@c int:{}").Children![0].Children![1].Children![1].Kind);
    }

    [Fact]
    public void WhenParameterValueContainsExpressionSymbolKindIsString ()
    {
        var parameters = new[] {
            new Parameter { Id = "str", ValueType = ValueType.String },
            new Parameter { Id = "int", ValueType = ValueType.Integer },
            new Parameter { Id = "dec", ValueType = ValueType.Decimal },
            new Parameter { Id = "bool", ValueType = ValueType.Boolean },
            new Parameter { Id = "list", ValueContainerType = ValueContainerType.List },
            new Parameter { Id = "named", ValueContainerType = ValueContainerType.NamedList }
        };
        meta.Commands = new[] { new Command { Id = "c", Parameters = parameters } };
        Assert.Equal(SymbolKind.String, GetSymbol("@c str:{}").Children![0].Children![1].Children![1].Kind);
        Assert.Equal(SymbolKind.String, GetSymbol("@c int:{}").Children![0].Children![1].Children![1].Kind);
        Assert.Equal(SymbolKind.String, GetSymbol("@c dec:{}").Children![0].Children![1].Children![1].Kind);
        Assert.Equal(SymbolKind.String, GetSymbol("@c bool:{}").Children![0].Children![1].Children![1].Kind);
        Assert.Equal(SymbolKind.String, GetSymbol("@c list:{}").Children![0].Children![1].Children![1].Kind);
        Assert.Equal(SymbolKind.String, GetSymbol("@c named:{}").Children![0].Children![1].Children![1].Kind);
    }

    private Symbol[] GetSymbols (string documentText)
    {
        var registry = new DocumentRegistry();
        new DocumentHandler(registry, new MockDiagnoser()).Open("@", documentText);
        return new SymbolHandler(new MetadataProvider(meta), registry).GetSymbols("@");
    }

    private Symbol GetSymbol (string lineText)
    {
        var symbols = GetSymbols(lineText);
        Assert.Single(symbols);
        return symbols[0];
    }
}
