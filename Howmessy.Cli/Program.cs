using CommandLine;

using Howmessy.Cli;
using Howmessy.CodeAnalysis.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

Parser.Default.ParseArguments<Options>(args).WithParsed(Process);

static void Process(Options options)
{
    if (options is null || options.Path is null || options.Metrics is null)
    {
        return;
    }

    var program = GetProgramRoot(options.Path);
    var root = GetProgramRoot(options.Path);
    var results = new Dictionary<string, ExpandoObject>();
    var methods = GetMethods(root).ToList();
    foreach (var method in methods)
    {
        var methodName = GetMemberName(method);
        if (!IsTargetMember(methodName, options.Target))
        {
            continue;
        }

        if (!results.TryGetValue(methodName, out var result))
        {
            result = new ExpandoObject();
            _ = result.TryAdd("Name", methodName);
            results[methodName] = result;
        }

        foreach (var metric in options.Metrics)
        {
            var metricName = GetMetricName(metric);
            var analyzeResult = AnalyzeMethod(method, metric);
            _ = result.TryAdd(metricName!, analyzeResult);
        }
    }


    var output = options.Format switch
    {
        Format.xml => SerializeToXml(results.Values),
        Format.json => SerializeToJson(results.Values),
        _ => SerializeToJson(results.Values)
    };

    Console.WriteLine(output);
}

static string GetMetricName(Metrics metrics)
{
    var attribute = metrics.GetType()
        .GetMember(metrics.ToString())
        .First()
        .GetCustomAttributes(typeof(DisplayAttribute), inherit: false)
        .First() as DisplayAttribute;

    return attribute?.Name ?? throw new ArgumentException(nameof(metrics));
}

static CompilationUnitSyntax GetProgramRoot(string path)
{
    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
    using var reader = new StreamReader(fs);
    var program = reader.ReadToEnd();
    var tree = CSharpSyntaxTree.ParseText(program);
    var root = tree.GetCompilationUnitRoot();

    return root;
}

static object AnalyzeMethod(BaseMethodDeclarationSyntax method, Metrics metrics)
{
    switch (metrics)
    {
        case Metrics.cognitive:
        {
            return CognitiveComplexityAnalyzer.Analyze(method); ;
        }
        case Metrics.cyclomatic:
        {
            return CyclomaticComplexityAnalyzer.Analyze(method);
        }
        case Metrics.mi:
        {
            var cyclomatic = CyclomaticComplexityAnalyzer.Analyze(method);
            var (_, logicalLoc) = method?.ExpressionBody == null
                ? LinesOfCodeAnalyzer.Analyze(method?.Body, ignoreBlockBrackets: true)
                : LinesOfCodeAnalyzer.Analyze(method?.ExpressionBody?.Expression, ignoreBlockBrackets: false);
            return (int)MaintainabilityIndexAnalyzer.Analyze(method, cyclomatic, logicalLoc);
        }
        case Metrics.loc:
        {
            var (physicalLoc, logicalLoc) = method?.ExpressionBody == null
                ? LinesOfCodeAnalyzer.Analyze(method?.Body, ignoreBlockBrackets: true)
                : LinesOfCodeAnalyzer.Analyze(method?.ExpressionBody?.Expression, ignoreBlockBrackets: false);
            return new
            {
                Physical = physicalLoc,
                Logical = logicalLoc,
            };
        }
        default:
            throw new ArgumentOutOfRangeException(nameof(metrics));
    }
}

static string GetMemberName(SyntaxNode? node)
{
    if (node is null)
    {
        return string.Empty;
    }

    switch (node)
    {
        case AccessorListSyntax accessorList:
            return GetMemberName(accessorList.Parent);
        case AccessorDeclarationSyntax accessor when accessor.IsKind(SyntaxKind.SetAccessorDeclaration):
            return GetMemberName(accessor.Parent) + " { set; }";
        case AccessorDeclarationSyntax accessor when accessor.IsKind(SyntaxKind.GetAccessorDeclaration):
            return GetMemberName(accessor.Parent) + " { get; }";
        case PropertyDeclarationSyntax property:
            return GetMemberName(property.Parent) + "." + property.Identifier.ValueText;
        case IndexerDeclarationSyntax indexer:
            return GetMemberName(indexer.Parent) + ".this" + indexer.ParameterList;
        case MethodDeclarationSyntax method:
            return GetMemberName(method.Parent) + "." + method.Identifier.ValueText + method.TypeParameterList + method.ParameterList;
        case ConstructorDeclarationSyntax constructor:
            return GetMemberName(constructor.Parent) + ".ctor" + constructor.ParameterList;
        case DestructorDeclarationSyntax destructor:
            return GetMemberName(destructor.Parent) + ".dtor" + destructor.ParameterList;
        case ConversionOperatorDeclarationSyntax conversion when conversion.ImplicitOrExplicitKeyword.ValueText == "implicit":
            return GetMemberName(conversion.Parent) + ".op_Implicit" + conversion.ParameterList;
        case ConversionOperatorDeclarationSyntax conversion when conversion.ImplicitOrExplicitKeyword.ValueText == "explicit":
            return GetMemberName(conversion.Parent) + ".op_Explicit" + conversion.ParameterList;
        case OperatorDeclarationSyntax @operator:
            return GetMemberName(@operator.Parent) + ".op_" + @operator.OperatorToken.ValueText + @operator.ParameterList;
        case TypeDeclarationSyntax type:
            var typeParent = GetMemberName(type.Parent);
            return string.IsNullOrEmpty(typeParent)
                ? "global::" + type.Identifier.ValueText
                : typeParent + "." + type.Identifier.ValueText;
        case NamespaceDeclarationSyntax @namespace:
            return @namespace.Name.ToString();
        default:
            return string.Empty;
    }
}

static IEnumerable<BaseMethodDeclarationSyntax> GetMethods(SyntaxNode node)
{
    switch (node)
    {
        case CompilationUnitSyntax root:
            foreach (var method in root.Members.SelectMany(GetMethods))
            {
                yield return method;
            }

            break;
        case NamespaceDeclarationSyntax @namespace:
            foreach (var method in @namespace.Members.SelectMany(GetMethods))
            {
                yield return method;
            }

            break;
        case TypeDeclarationSyntax type:
            foreach (var method in type.Members.SelectMany(GetMethods))
            {
                yield return method;
            }

            break;
        case BaseMethodDeclarationSyntax method:
            yield return method;
            break;
        default:
            break;
    }
}

static bool IsTargetMember(string memberName, string? target)
{
    if (string.IsNullOrWhiteSpace(target))
    {
        return true;
    }

    var regex = "^" + Regex.Escape(target).Replace("\\*", ".*?").Replace("\\?", ".");
    return Regex.IsMatch(memberName, regex);
}

static string SerializeToJson(object result) => JsonConvert.SerializeObject(result, Formatting.Indented);

static string SerializeToXml<T>(T result)
{
    var serializer = new XmlSerializer(typeof(T));
    using var ms = new MemoryStream();
    serializer.Serialize(ms, result);
    return Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);
}