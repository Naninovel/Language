using Naninovel.Metadata;
using Xunit;

namespace Naninovel.Language.Test;

public class HoverTest
{
    private readonly Project meta = new();

    [Fact]
    public void WhenCommandMetadataNotFoundNullIsReturned ()
    {
        Assert.Null(Hover("@c", 1));
        Assert.Null(Hover("[c]", 1));
    }

    [Fact]
    public void WhenParameterMetadataNotFoundNullIsReturned ()
    {
        meta.Commands = new[] { new Command { Id = "c" } };
        Assert.Null(Hover("@c p", 3));
        Assert.Null(Hover("[c p]", 3));
    }

    [Fact]
    public void WhenOverNothingHoverableNullIsReturned ()
    {
        meta.Commands = new[] { new Command { Id = "c" } };
        Assert.Null(Hover("", 0));
        Assert.Null(Hover("; comment", 5));
        Assert.Null(Hover("# label", 0));
        Assert.Null(Hover("generic text", 5));
        Assert.Null(Hover("@c ", 3));
        Assert.Null(Hover("[c]", 3));
        Assert.Null(Hover("{expression}", 5));
    }

    [Fact]
    public void WhenOverCommandIdentifierHoverContainSummary ()
    {
        meta.Commands = new[] { new Command { Id = "c", Summary = "foo" } };
        Assert.Contains("foo", Hover("@c p:v", 1).Contents.Value);
        Assert.Contains("foo", Hover("[c p:v]", 1).Contents.Value);
    }

    [Fact]
    public void WhenOverParameterHoverContentIsEqualToSummary ()
    {
        var parameters = new[] { new Parameter { Id = "p", Summary = "foo" } };
        meta.Commands = new[] { new Command { Id = "c", Parameters = parameters } };
        Assert.Equal("foo", Hover("@c p:v", 4).Contents.Value);
        Assert.Equal("foo", Hover("[c p:v]", 4).Contents.Value);
    }

    [Fact]
    public void CommandIdentifierHoverRangeIsEqualToContentRange ()
    {
        meta.Commands = new[] { new Command { Id = "c", Summary = "" } };
        Assert.Equal(new Range(new(0, 1), new(0, 2)), Hover("@c p:v", 1).Range);
    }

    [Fact]
    public void ParameterHoverRangeIsEqualToContentRange ()
    {
        var parameters = new[] { new Parameter { Id = "p", Summary = "foo" } };
        meta.Commands = new[] { new Command { Id = "c", Parameters = parameters } };
        Assert.Equal(new Range(new(0, 3), new(0, 6)), Hover("@c p:v", 5).Range);
    }

    [Fact]
    public void CommandSummaryHasCorrectMarkup ()
    {
        meta.Commands = new[] { new Command { Id = "c", Summary = "foo" } };
        Assert.Contains("## Summary\nfoo", Hover("@c", 1).Contents.Value);
    }

    [Fact]
    public void CommandRemarksHaveCorrectMarkup ()
    {
        meta.Commands = new[] { new Command { Id = "c", Remarks = "foo" } };
        Assert.Contains("## Remarks\nfoo", Hover("@c", 1).Contents.Value);
    }

    [Fact]
    public void CommandExamplesHaveCorrectMarkup ()
    {
        meta.Commands = new[] { new Command { Id = "c", Examples = "foo" } };
        Assert.Contains("## Examples\n```nani\nfoo\n```", Hover("@c", 1).Contents.Value);
    }

    [Fact]
    public void CommandParametersHaveCorrectMarkup ()
    {
        var parameters = new[] {
            new Parameter { Id = "Nameless", Nameless = true },
            new Parameter { Id = "Required", Required = true },
            new Parameter { Id = "NamelessAndRequired", Nameless = true, Required = true },
            new Parameter { Id = "WithSummary", Summary = "foo" },
            new Parameter { Id = "WithAlias", Alias = "Alias" }
        };
        meta.Commands = new[] { new Command { Id = "c", Parameters = parameters } };
        var content = Hover("@c", 1).Contents.Value;
        Assert.Contains("## Parameters\nName | Type | Summary\n:--- | :--- | :---\n", content);
        Assert.Contains("~nameless~ | string | \n", content);
        Assert.Contains("**required** | string | \n", content);
        Assert.Contains("~**namelessAndRequired**~ | string | \n", content);
        Assert.Contains("withSummary | string | foo\n", content);
        Assert.Contains("alias | string | \n", content);
    }

    private Hover Hover (string lineText, int charOffset)
    {
        var registry = new DocumentRegistry();
        var handler = new HoverHandler(new MetadataProvider(meta), registry);
        new DocumentHandler(registry, new MockDiagnoser()).Open("@", lineText);
        return handler.Hover("@", new Position(0, charOffset));
    }
}
