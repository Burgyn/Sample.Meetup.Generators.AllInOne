using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace ToString
{
    [Generator]
    public class ToStringGenerator : IIncrementalGenerator
    {
        private const string AttributeSourceText = @"
using System;
namespace ToString
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ToStringAttribute: Attribute
    {
    }
}
";
        private const string AttributeName = "ToString";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 👇 Generate ToStringAttribute.
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "Attribute.cs", SourceText.From(AttributeSourceText, Encoding.UTF8)));

            // 👇 Declare pipeline filters.
            IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsClassDeclarationWithAttributes(s),
                    transform: static (ctx, _) => GetSemanticTarget((ClassDeclarationSyntax)ctx.Node))
                .Where(static m => m is not null)!;

            // 👇 Combine with Compilation.
            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
                = context.CompilationProvider.Combine(classDeclarations.Collect());

            // 👇 Register source output generator
            context.RegisterSourceOutput(
                compilationAndClasses,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private static bool IsClassDeclarationWithAttributes(SyntaxNode node)
            => node is ClassDeclarationSyntax m && m.AttributeLists.Count > 0;

        private static ClassDeclarationSyntax? GetSemanticTarget(ClassDeclarationSyntax node)
            => node.HaveAttribute(AttributeName) ? node : null;

        private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            foreach (ClassDeclarationSyntax candidate in classes)
            {
                // 👇 Stop if we're asked to.
                context.CancellationToken.ThrowIfCancellationRequested();

                // 👇 Generate model.
                ClassModel model = GenerateModel(candidate, compilation);

                // 👇 Format code.
                string code = Format(Generate(model));

                // 👇 Add source code.
                context.AddSource($"{model.Name}.cs", SourceText.From(code, Encoding.UTF8));
            }
        }

        private static ClassModel GenerateModel(
            ClassDeclarationSyntax syntax,
            Compilation compilation)
        {
            CompilationUnitSyntax root = syntax.GetCompilationUnit();
            SemanticModel classSemanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            var classSymbol = classSemanticModel.GetDeclaredSymbol(syntax);

            return new ClassModel()
            {
                Namespace = root.GetNamespace(),
                Name = syntax.GetClassName(),
                Modifier = syntax.GetClassModifier(),
                Properties = classSymbol is null ? Array.Empty<string>() : classSymbol.GetProperties()
            };
        }

        private static string Generate(ClassModel model)
        {
            var sb = new StringBuilder();

            sb.Append($@"
namespace {model.Namespace}
{{
{model.Modifier} class {model.Name}
{{
public override string ToString()
=> $""");

            for (int i = 0; i < model.Properties.Length; i++)
            {
                string prop = model.Properties[i];
                sb.Append($"{prop} = {{{prop}}}");
                if (i < model.Properties.Length - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append("\";}}");

            return sb.ToString();
        }

        private static string Format(string output)
        {
            var tree = CSharpSyntaxTree.ParseText(output);
            var root = (CSharpSyntaxNode)tree.GetRoot();
            output = root.NormalizeWhitespace().ToFullString();

            return output;
        }
    }
}
