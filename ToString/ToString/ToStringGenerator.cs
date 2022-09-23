﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace ToString
{
    [Generator]
    public class ToStringGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ToStringReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // 👇 Generate ToStringAttribute.
            string attribute = @"
using System;
namespace ToString
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ToStringAttribute: Attribute
    {
    }
}
";
            context.AddSource("Attribute.cs", SourceText.From(attribute, Encoding.UTF8));

            if (context.SyntaxReceiver is ToStringReceiver actorSyntaxReciver)
            {
                foreach (ClassDeclarationSyntax candidate in actorSyntaxReciver.Candidates)
                {
                    // 👇 Generate model.
                    ClassModel model = GenerateModel(candidate, context.Compilation);

                    // 👇 Format code.
                    string code = Format(Generate(model));

                    // 👇 Add source code.
                    context.AddSource($"{model.Name}.cs", SourceText.From(code, Encoding.UTF8));
                }
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
                Properties = classSymbol.GetProperties()
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
