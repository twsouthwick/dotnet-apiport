// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reports.Resources;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Reports.Generators
{
    public class UnresolvedAssemblyPageGenerator : IPageGenerator
    {
        private static readonly double[] ColumnWidth = new[] { 40D, 40D, 30D };

        public IEnumerable<Page> GeneratePages(AnalyzeResponse response)
        {
            if (response.ReportingResult.GetUnresolvedAssemblies().Any())
            {
                yield return new Page
                {
                    Title = LocalizedStrings.UnresolvedUsedAssembly,
                    Content = new[]
                    {
                        GenerateTable(response)
                    },
                };
            }
        }

        private static Table GenerateTable(AnalyzeResponse response)
        {
            var missingAssembliesPageHeader = new[] { LocalizedStrings.AssemblyHeader, LocalizedStrings.UsedBy, LocalizedStrings.UnresolvedAssemblyStatus };

            return new Table
            {
                Headers = missingAssembliesPageHeader,
                Rows = GenerateUnreferencedAssembliesPage(response).ToList(),
                ColumnWidth = ColumnWidth,
            };
        }

        private static IEnumerable<Row> GenerateUnreferencedAssembliesPage(AnalyzeResponse response)
        {
            var unresolvedAssembliesMap = response.ReportingResult.GetUnresolvedAssemblies();

            foreach (var unresolvedAssemblyPair in unresolvedAssembliesMap.OrderBy(asm => asm.Key))
            {
                if (unresolvedAssemblyPair.Value.Any())
                {
                    foreach (var usedIn in unresolvedAssemblyPair.Value)
                    {
                        yield return new Row(unresolvedAssemblyPair.Key, usedIn, LocalizedStrings.UnresolvedUsedAssembly);
                    }
                }
                else
                {
                    yield return new Row(unresolvedAssemblyPair.Key, string.Empty, LocalizedStrings.UnresolvedUsedAssembly);
                }
            }

            foreach (var unresolvedAssemblyPair in response.Request.NonUserAssemblies.OrderBy(asm => asm.AssemblyIdentity))
            {
                yield return new Row(unresolvedAssemblyPair.AssemblyIdentity, string.Empty, LocalizedStrings.SkippedAssembly);
            }
        }
    }
}
