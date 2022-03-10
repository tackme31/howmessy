namespace Howmessy.Cli;
using CommandLine;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Options
{
    [Value(0, MetaName = "path", Required = true, HelpText = "File path to analyze.")]
    public string? Path { get; set; }

    [Option('m', "metrics", Required = true, Separator = ',', HelpText = "Metrics type to analyze (cognitive/cyclomatic/mi/loc).")]
    public IEnumerable<Metrics>? Metrics { get; set; }

    [Option('t', "target", HelpText = "Fully qualified ethod name to analyze.")]
    public string? Target { get; set; }

    [Option('f', "format", HelpText = "Output format.")]
    public Format Format { get; set; }
}

public enum Metrics
{
    [Display(Name = "CognitiveComplexity")]
    cognitive,

    [Display(Name = "CyclomaticComplexity")]
    cyclomatic,

    [Display(Name = "MaintainabilityIndex")]
    mi,

    [Display(Name = "LinesOfCode")]
    loc
}

public enum Format
{
    json,
    xml,
}

