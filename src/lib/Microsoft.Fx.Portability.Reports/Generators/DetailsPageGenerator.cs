// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Microsoft.Fx.Portability.Reports.Resources;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Reports.Generators
{
    public class DetailsPageGenerator : IPageGenerator
    {
        private readonly ITargetMapper _mapper;

        public DetailsPageGenerator(ITargetMapper mapper)
        {
            _mapper = mapper;
        }

        public Page GeneratePage(AnalyzeResponse response)
        {
            return new Page
            {
                Title = LocalizedStrings.DetailsPageTitle,
                Content = new[]
                {
                    GenerateTable(response.ReportingResult)
                },
            };
        }

        private Table GenerateTable(ReportingResult analysisResult)
        {
            var showAssemblyColumn = analysisResult.GetAssemblyUsageInfo().Any();

            var detailsPageHeader = new List<string>() { LocalizedStrings.TargetTypeHeader, LocalizedStrings.TargetMemberHeader };

            if (showAssemblyColumn)
            {
                detailsPageHeader.Add(LocalizedStrings.AssemblyHeader);
            }

            detailsPageHeader.AddRange(_mapper.GetTargetNames(analysisResult.Targets, alwaysIncludeVersion: true));
            detailsPageHeader.Add(LocalizedStrings.RecommendedChanges);

            return new Table
            {
                Headers = detailsPageHeader,
                Rows = BuildDetailsRows(analysisResult, showAssemblyColumn).ToList(),
                ColumnWidth = GetColumnWidths(analysisResult)
            };
        }

        private static class ColumnWidth
        {
            public const double TargetType = 40;
            public const double TargetMember = 40;
            public const double AssemblyName = 30;
            public const double RecommendedChanges = 50;
            public const double Targets = 15;
        }

        private IReadOnlyCollection<double> GetColumnWidths(ReportingResult analysisResult)
        {
            var columnWidths = new List<double>
            {
                ColumnWidth.TargetType,
                ColumnWidth.TargetMember,
                ColumnWidth.AssemblyName
            };

            columnWidths.AddRange(Enumerable.Repeat(ColumnWidth.Targets, analysisResult.Targets.Count));
            columnWidths.Add(ColumnWidth.RecommendedChanges);

            return columnWidths;
        }

        private IEnumerable<Row> BuildDetailsRows(ReportingResult analysisResult, bool showAssemblyColumn)
        {
            // Dump out all the types that were identified as missing from the target
            foreach (var item in analysisResult.GetMissingTypes().OrderByDescending(n => n.IsMissing))
            {
                if (item.IsMissing)
                {
                    if (!showAssemblyColumn)
                    {
                        // for a missing type we are going to dump the type name for both the target type and target member columns
                        var rowContent = new List<object> { AddLink(item.TypeName), AddLink(item.TypeName) };

                        rowContent.AddRange(item.TargetStatus);
                        rowContent.Add(item.RecommendedChanges);

                        yield return new Row(rowContent);
                    }
                    else
                    {
                        foreach (var assemblies in item.UsedIn)
                        {
                            string assemblyName = analysisResult.GetNameForAssemblyInfo(assemblies);

                            // for a missing type we are going to dump the type name for both the target type and target member columns
                            var rowContent = new List<object> { AddLink(item.TypeName), AddLink(item.TypeName), assemblyName };
                            rowContent.AddRange(item.TargetStatus);
                            rowContent.Add(item.RecommendedChanges);

                            yield return new Row(rowContent);
                        }
                    }
                }

                foreach (var member in item.MissingMembers.OrderBy(type => type.MemberName))
                {
                    if (showAssemblyColumn)
                    {
                        foreach (var assem in member.UsedIn.OrderBy(asm => asm.AssemblyIdentity))
                        {
                            string assemblyName = analysisResult.GetNameForAssemblyInfo(assem);
                            var rowContent = new List<object> { AddLink(item.TypeName), AddLink(member.MemberName), assemblyName };

                            rowContent.AddRange(member.TargetStatus);
                            rowContent.Add(member.RecommendedChanges);

                            yield return new Row(rowContent);
                        }
                    }
                    else
                    {
                        var rowContent = new List<object> { AddLink(item.TypeName), AddLink(member.MemberName) };

                        rowContent.AddRange(member.TargetStatus);
                        rowContent.Add(member.RecommendedChanges);

                        yield return new Row(rowContent);
                    }
                }
            }
        }

        private static string AddLink(string docId) => docId;
    }
}
