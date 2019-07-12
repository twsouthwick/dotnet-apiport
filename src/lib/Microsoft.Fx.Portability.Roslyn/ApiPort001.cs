// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;

namespace Microsoft.Fx.Portability.Roslyn
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic, LanguageNames.FSharp)]
    public class ApiPort001 : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ApiPort001";

        private const string Category = "Porting";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        private readonly ICatalogCache _cache;

        public ApiPort001()
        {
        }

        public ApiPort001(ICatalogCache cache)
        {
            _cache = cache;
        }

        public ICatalogCache GetCache() => _cache ?? ServiceLocator.Cache;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterOperationAction(action =>
            {
                var cache = GetCache();

                if (cache is null)
                {
                    return;
                }

                var symbol = GetSymbol(action.Operation);

                if (cache.GetApiStatus(symbol, out var names) == ApiStatus.Unavailable)
                {
                    action.ReportDiagnostic(Diagnostic.Create(Rule, action.Operation.Syntax.GetLocation(), symbol.GetDocumentationCommentId(), string.Join(", ", names)));
                }
            }, OperationKind.MethodReference, OperationKind.Invocation, OperationKind.FieldReference, OperationKind.EventReference);
        }

        private static ISymbol GetSymbol(IOperation operation)
        {
            if (operation is IMemberReferenceOperation member)
            {
                return member.Member;
            }
            else if (operation is IInvocationOperation invocation)
            {
                return invocation.TargetMethod;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
