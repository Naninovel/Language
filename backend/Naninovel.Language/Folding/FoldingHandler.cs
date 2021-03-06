using System.Collections.Generic;
using Naninovel.Parsing;

namespace Naninovel.Language;

// https://microsoft.github.io/language-server-protocol/specifications/specification-3-17/#textDocument_foldingRange

public class FoldingHandler
{
    private readonly DocumentRegistry registry;
    private readonly List<FoldingRange> ranges = new();

    private int lineIndex;
    private FoldingRange? range;

    public FoldingHandler (DocumentRegistry registry)
    {
        this.registry = registry;
    }

    public FoldingRange[] GetFoldingRanges (string documentUri)
    {
        ResetState();
        var lines = registry.Get(documentUri).Lines;
        for (; lineIndex < lines.Count; lineIndex++)
            if (ShouldFold(lines[lineIndex].Script))
                FoldLine(lines[lineIndex].Script);
        if (range is not null) AddRange();
        return ranges.ToArray();
    }

    private void ResetState ()
    {
        ranges.Clear();
        lineIndex = 0;
        range = null;
    }

    private bool ShouldFold (IScriptLine line)
    {
        return line is CommentLine or CommandLine;
    }

    private void FoldLine (IScriptLine line)
    {
        if (range is null) range = new(lineIndex, lineIndex);
        else if (lineIndex > range.EndLine + 1) AddRange();
        else range.EndLine = lineIndex;
    }

    private void AddRange ()
    {
        ranges.Add(range!);
        range = new(lineIndex, lineIndex);
    }
}
