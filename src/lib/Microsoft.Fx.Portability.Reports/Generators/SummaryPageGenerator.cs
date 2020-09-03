// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reports.Resources;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Fx.Portability.Reports.Generators
{
    public class SummaryPageGenerator : IPageGenerator
    {
        private readonly ITargetMapper _mapper;

        public SummaryPageGenerator(ITargetMapper mapper)
        {
            _mapper = mapper;
        }

        public Page GeneratePage(AnalyzeResponse response)
        {
            var targetNames = _mapper.GetTargetNames(response.Targets, alwaysIncludeVersion: true);

            return new Page
            {
                Title = LocalizedStrings.PortabilitySummaryPageTitle,
                Content = new[]
                {
                    new Table
                    {
                        Rows = new[]
                        {
                           new Row(LocalizedStrings.SubmissionId, response.SubmissionId),
                           new Row(LocalizedStrings.Targets, string.Join(", ", targetNames))
                        },
                    },
                    BuildSummaryTable(response, targetNames),
                    new Table
                    {
                    },
                    new Table
                    {
                        Rows = new[]
                        {
                            new Row(LocalizedStrings.CatalogLastUpdated, response.CatalogLastUpdated.ToString("D", CultureInfo.CurrentCulture)),
                            new Row(LocalizedStrings.HowToReadTheExcelTable)
                        }
                    },
                }
            };
        }

        private Table BuildSummaryTable(AnalyzeResponse response, IEnumerable<string> targetNames)
        {
            var assemblyInfoHeader = new List<string> { LocalizedStrings.AssemblyHeader, "Target Framework" };
            assemblyInfoHeader.AddRange(targetNames);

            return new Table
            {
                Headers = assemblyInfoHeader,
                Rows = BuildRows().ToList(),
            };

            IEnumerable<Row> BuildRows()
            {
                foreach (var item in response.ReportingResult.GetAssemblyUsageInfo().OrderBy(a => a.SourceAssembly.AssemblyIdentity))
                {
                    var summaryData = new List<object> { item.SourceAssembly.AssemblyIdentity, item.SourceAssembly.TargetFrameworkMoniker ?? string.Empty };

                    // TODO: figure out how to add formatting to cells to show percentages.
                    summaryData.AddRange(item.UsageData.Select(pui => (object)Math.Round(pui.PortabilityIndex * 100.0, 2)));

                    yield return new Row(summaryData);
                }
            }
        }
    }
}
