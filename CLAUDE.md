# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Howmessy is a Visual Studio extension that displays code complexity metrics in CodeLens. It also provides a standalone CLI tool for calculating metrics. The project calculates four types of metrics:
- Cognitive Complexity
- Cyclomatic Complexity
- Maintainability Index
- Lines of Code (physical and logical)

## Solution Structure

The solution consists of five C# projects:

1. **Howmessy.CodeAnalysis** (netstandard2.0): Core metrics calculation engine
   - `Analyzers/`: Contains analyzer classes for each metric type (CognitiveComplexityAnalyzer, CyclomaticComplexityAnalyzer, LinesOfCodeAnalyzer, MaintainabilityIndexAnalyzer)
   - `Walkers/`: Roslyn syntax walkers that traverse the syntax tree to calculate metrics (CognitiveComplexityWalker, CyclomaticComplexityWalker, HalsteadMetricsWalker)
   - Uses Microsoft.CodeAnalysis (Roslyn) 4.0.1

2. **Howmessy.Shared** (netstandard2.0): Shared types and interfaces
   - CodeLensData: Data model for metrics results
   - MetricsType enum, option interfaces, logging utilities
   - No external dependencies

3. **Howmessy.CodeLensProvider** (.NET Framework 4.8): CodeLens provider for Visual Studio
   - CodeLensDataPoint: Manages individual CodeLens data points
   - CodeLensProvider: Entry point for Visual Studio CodeLens integration
   - VisualStudioConnectionHandler: Handles IPC communication
   - Uses StreamJsonRpc for remote communication

4. **Howmessy.VSExtension** (.NET Framework 4.8): Visual Studio extension package (VSIX)
   - HowmessyPackage: Main VS package class
   - CodeMetricsProvider: Connects to Roslyn workspace and calculates metrics
   - CodeLensConnectionHandler: Manages connections between VS extension and CodeLens provider
   - HowmessySaveCommandHandler: Command handler for save operations
   - Options/: User configuration pages (GeneralOptions, ThresholdOptions)
   - References Howmessy.CodeAnalysis, Howmessy.CodeLensProvider, and Howmessy.Shared

5. **Howmessy.Cli** (.NET 6): Command-line tool
   - Uses CommandLineParser for argument parsing
   - Outputs metrics in JSON format
   - References Howmessy.CodeAnalysis

## Building and Testing

### Build the entire solution:
```bash
dotnet build howmessy.sln
```

### Build specific projects:
```bash
# Build the CLI tool
dotnet build Howmessy.Cli/Howmessy.Cli.csproj

# Build the VS extension (requires Visual Studio SDK)
msbuild Howmessy.VSExtension/Howmessy.VSExtension.csproj
```

### Running the CLI tool:
```bash
# From the CLI project directory
dotnet run --project Howmessy.Cli/Howmessy.Cli.csproj -- <path-to-cs-file> --metrics cognitive,cyclomatic,mi,loc

# Example
dotnet run --project Howmessy.Cli/Howmessy.Cli.csproj -- ./Sample.cs --metrics cognitive,cyclomatic,mi,loc -t "FullyQualifiedMethodName"
```

### Debugging the VS extension:
The VSExtension project is configured to launch Visual Studio experimental instance on F5. Start action points to devenv.exe with /rootsuffix Exp.

## Architecture Details

### Metrics Calculation Flow

1. **Entry Point**: CodeMetricsProvider.LoadCodeMetrics() in VSExtension receives a document location
2. **Document Retrieval**: Gets the Roslyn Document from VisualStudioWorkspace
3. **Method Extraction**: GetMethod() finds the BaseMethodDeclarationSyntax at the specified text span
4. **Analysis**: Each analyzer is invoked with the syntax node:
   - Analyzers are static classes with an Analyze() method
   - Analyzers instantiate corresponding Walker classes
   - Walkers inherit from CSharpSyntaxWalker and override Visit* methods
   - Walkers traverse the syntax tree and accumulate complexity scores
5. **Result Packaging**: Metrics are packaged into CodeLensData and returned

### Communication Architecture

The VS extension uses a split architecture:
- **VSExtension**: Runs in the main Visual Studio process, has access to VisualStudioWorkspace
- **CodeLensProvider**: Runs as a separate service, communicates via StreamJsonRpc
- **CodeLensConnectionHandler**: Manages the named pipe connection between the two processes

### Key Patterns

- **Analyzers**: Facade pattern - simple static Analyze() methods that delegate to Walkers
- **Walkers**: Visitor pattern - override specific Visit*() methods from CSharpSyntaxWalker to calculate complexity during tree traversal
- **Options**: VS uses DialogPageProvider pattern with BaseOptionModel for settings persistence

## Important Implementation Details

### Cognitive Complexity
- Follows SonarSource specification
- Tracks nesting depth and increments for control flow structures
- Located in CognitiveComplexityWalker.cs

### Cyclomatic Complexity
- Counts decision points + 1
- Simpler than cognitive complexity
- Located in CyclomaticComplexityWalker.cs

### Maintainability Index
- Uses Halstead metrics and LOC
- Formula includes volume, cyclomatic complexity, and lines of code
- HalsteadMetricsWalker counts operators and operands
- Located in MaintainabilityIndexAnalyzer.cs

### Lines of Code
- Counts both physical and logical lines
- Can ignore block brackets based on context
- Expression-bodied members handled separately
- Located in LinesOfCodeAnalyzer.cs

## Configuration

Users can configure:
- Which metrics to display (GeneralOptions)
- Threshold values for color-coding (ThresholdOptions)
- Display format

Options are persisted per Visual Studio profile and trigger CodeLens refresh on save.
