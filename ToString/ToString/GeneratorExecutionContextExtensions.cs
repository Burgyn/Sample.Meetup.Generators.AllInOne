﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ToString
{
    public static class GeneratorExecutionContextExtensions
    {
        private static readonly DiagnosticDescriptor _missingPartialModifier = new DiagnosticDescriptor(
            id: "SMP001",
            title: "Missing partial modifier",
            messageFormat: "A partial modifier is required, a ToString override will not be generated",
            category: "Sample.Meetup",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static void ReportMissingPartialModifier(
            this GeneratorExecutionContext context,
            ClassDeclarationSyntax classDeclaration)
            => context.ReportDiagnostic(
                Diagnostic.Create(
                    _missingPartialModifier,
                    classDeclaration.GetLocation()));
    }
}
