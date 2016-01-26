using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace AssertExpectedActualAnalyser
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AssertExpectedActualAnalyserCodeFixProvider)), Shared]
    public class AssertExpectedActualAnalyserCodeFixProvider : CodeFixProvider
    {
        private const string title = "Make uppercase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AssertExpectedActualAnalyserAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ReverseExpectedActual(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> ReverseExpectedActual(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var currentArgsSyntax = invocation.ArgumentList;

            var flippedArgsList = FlipFirstTwo(invocation.ArgumentList.Arguments);

            var fixedInvocation = invocation.WithArgumentList(currentArgsSyntax.WithArguments(flippedArgsList));

            var docSyntax = await document.GetSyntaxRootAsync();

            var fixedDocSyntax = docSyntax.ReplaceNode(invocation, fixedInvocation);

            return document.WithSyntaxRoot(fixedDocSyntax);
        }

        private static SeparatedSyntaxList<T> FlipFirstTwo<T>(SeparatedSyntaxList<T> syntaxList)
            where T : SyntaxNode
        {
            if (syntaxList.Count < 2)
            {
                return syntaxList;
            }

            var first = syntaxList[0];
            var second = syntaxList[1];

            return syntaxList.RemoveAt(0)
                             .RemoveAt(0)
                             .InsertRange(0, new[] { second, first });
        }
    }
}