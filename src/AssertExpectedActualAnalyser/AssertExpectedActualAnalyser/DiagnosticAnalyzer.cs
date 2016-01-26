using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AssertExpectedActualAnalyser
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertExpectedActualAnalyserAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AssertExpectedActualAnalyser";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Testing";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            var expr = invocation.Expression as MemberAccessExpressionSyntax;

            if (!IsExpectedActualAssertion(expr))
            {
                return;
            }

            // it's an assertion with (expected, actual) arguments

            var assertArgs = invocation.ArgumentList.Arguments;

            if (assertArgs == null || assertArgs.Count < 2 || assertArgs[1] == null)
            {
                return;
            }

            var actualSyntax = assertArgs[1].Expression;

            if (actualSyntax == null)
            {
                return;
            }

            var actualSyntaxKind = actualSyntax.Kind();

            if (actualSyntaxKind == SyntaxKind.StringLiteralExpression
                || actualSyntaxKind == SyntaxKind.NumericLiteralExpression
                || actualSyntaxKind == SyntaxKind.TrueLiteralExpression
                || actualSyntaxKind == SyntaxKind.FalseLiteralExpression)
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), expr.ToString(), actualSyntax.ToString());

                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsExpectedActualAssertion(ExpressionSyntax expression)
        {
            var expr = expression as MemberAccessExpressionSyntax;
            if (expr == null)
            {
                return false;
            }

            var identifier = expr.Expression as IdentifierNameSyntax;
            if (identifier == null)
            {
                return false;
            }

            if (identifier.Identifier.Text != "Assert")
            {
                return false;
            }

            var methodName = expr.Name.Identifier.Text;

            if (methodName == "Equal"  // Xunit
                || methodName == "AreEqual"  // NUnit, MSTest
                || methodName == "NotEqual"  // Xunit
                || methodName == "AreNotEqual"  // NUnit, MSTest
                )
            {
                return true;
            }

            return false;
        }
    }
}
