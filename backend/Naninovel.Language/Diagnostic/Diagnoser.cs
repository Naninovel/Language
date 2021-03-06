using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Naninovel.Metadata;
using Naninovel.Parsing;

namespace Naninovel.Language;

// https://microsoft.github.io/language-server-protocol/specifications/specification-3-16/#textDocument_publishDiagnostics

public class Diagnoser : IDiagnoser
{
    private readonly MetadataProvider meta;
    private readonly PublishDiagnostics publish;
    private readonly List<Diagnostic> diagnostics = new();

    private int lineIndex;

    public Diagnoser (MetadataProvider meta, PublishDiagnostics publish)
    {
        this.meta = meta;
        this.publish = publish;
    }

    public void Diagnose (string documentUri, Document document)
    {
        ResetState();
        for (; lineIndex < document.Lines.Count; lineIndex++)
            DiagnoseLine(document[lineIndex]);
        Publish(documentUri);
    }

    private void ResetState ()
    {
        diagnostics.Clear();
        lineIndex = 0;
    }

    private void Publish (string documentUri)
    {
        if (diagnostics.Count == 0)
            publish(documentUri, Array.Empty<Diagnostic>());
        else publish(documentUri, diagnostics.ToArray());
    }

    private void DiagnoseLine (DocumentLine line)
    {
        foreach (var error in line.Errors)
            AddParseError(error);
        if (line.Script is GenericTextLine genericLine)
            DiagnoseGenericLine(genericLine);
        else if (line.Script is CommandLine commandLine)
            DiagnoseCommand(commandLine.Command);
    }

    private void AddParseError (ParseError error)
    {
        var range = new Range(
            new(lineIndex, error.StartIndex),
            new(lineIndex, error.EndIndex + 1));
        diagnostics.Add(new(range, DiagnosticSeverity.Error, error.Message));
    }

    private void DiagnoseGenericLine (GenericTextLine line)
    {
        foreach (var content in line.Content)
            if (content is InlinedCommand inlined)
                DiagnoseCommand(inlined.Command);
    }

    private void DiagnoseCommand (Parsing.Command command)
    {
        if (string.IsNullOrEmpty(command.Identifier)) return;
        var commandMeta = meta.FindCommand(command.Identifier);
        if (commandMeta is null) AddUnknownCommand(command);
        else DiagnoseCommand(command, commandMeta);
    }

    private void DiagnoseCommand (Parsing.Command command, Metadata.Command commandMeta)
    {
        foreach (var paramMeta in commandMeta.Parameters)
            if (paramMeta.Required && !IsParameterDefined(paramMeta, command))
                AddMissingRequiredParameter(command, paramMeta);
        foreach (var param in command.Parameters)
            DiagnoseParameter(param, commandMeta);
    }

    private void DiagnoseParameter (Parsing.Parameter param, Metadata.Command commandMeta)
    {
        var paramMeta = meta.FindParameter(commandMeta.Id, param.Identifier);
        if (paramMeta is null) AddUnknownParameter(param, commandMeta);
        else if (param.Value.Empty || param.Value.Dynamic) return;
        else if (!IsValueValid(param.Value, paramMeta))
            AddInvalidValue(param.Value, paramMeta);
    }

    private void AddUnknownCommand (Parsing.Command command)
    {
        var range = Range.FromContent(command, lineIndex);
        var message = $"Command '{command.Identifier}' is unknown.";
        diagnostics.Add(new(range, DiagnosticSeverity.Error, message));
    }

    private void AddUnknownParameter (Parsing.Parameter param, Metadata.Command commandMeta)
    {
        var range = Range.FromContent(param, lineIndex);
        var message = param.Nameless
            ? $"Command '{commandMeta.Label}' doesn't have a nameless parameter."
            : $"Command '{commandMeta.Label}' doesn't have '{param.Identifier}' parameter.";
        diagnostics.Add(new(range, DiagnosticSeverity.Error, message));
    }

    private void AddMissingRequiredParameter (Parsing.Command command, Metadata.Parameter missingParam)
    {
        var range = Range.FromContent(command, lineIndex);
        var message = $"Required parameter '{missingParam.Label}' is missing.";
        diagnostics.Add(new(range, DiagnosticSeverity.Error, message));
    }

    private void AddInvalidValue (ParameterValue value, Metadata.Parameter paramMeta)
    {
        var range = Range.FromContent(value, lineIndex);
        var message = $"Invalid value: '{value}' is not a {paramMeta.TypeLabel}.";
        diagnostics.Add(new(range, DiagnosticSeverity.Error, message));
    }

    private static bool IsParameterDefined (Metadata.Parameter paramMeta, Parsing.Command command)
    {
        foreach (var param in command.Parameters)
            if (string.Equals(param.Identifier, paramMeta.Id, StringComparison.OrdinalIgnoreCase)) return true;
            else if (string.Equals(param.Identifier, paramMeta.Alias, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private static bool IsValueValid (ParameterValue value, Metadata.Parameter paramMeta)
    {
        if (paramMeta.ValueContainerType is ValueContainerType.List)
            return value.Text.Split(',', StringSplitOptions.RemoveEmptyEntries).All(IsValid);
        if (paramMeta.ValueContainerType is ValueContainerType.NamedList)
            return value.Text.Split(',', StringSplitOptions.RemoveEmptyEntries).All(v => IsValid(GetNamedValue(v)));
        if (paramMeta.ValueContainerType is ValueContainerType.Named)
            return IsValid(GetNamedValue(value));
        return IsValid(value);

        bool IsValid (string value) => paramMeta.ValueType switch {
            Metadata.ValueType.Integer => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            Metadata.ValueType.Decimal => float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _),
            Metadata.ValueType.Boolean => bool.TryParse(value, out _),
            Metadata.ValueType.String or _ => true
        };
    }

    private static string GetNamedValue (string value)
    {
        var dotIdx = value.LastIndexOf('.');
        if (dotIdx < 0 || dotIdx + 1 >= value.Length) return "";
        return value[(dotIdx + 1)..];
    }
}
