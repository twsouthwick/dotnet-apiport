// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Reports
{
    public class ReportGenerator : IReportGenerator2
    {
        private readonly IEnumerable<IPageGenerator> _generators;

        public ReportGenerator(IEnumerable<IPageGenerator> generators)
        {
            _generators = generators;
        }

        public IEnumerable<Page> GeneratePages(AnalyzeResponse response)
        {
            foreach (var generator in _generators)
            {
                foreach (var page in generator.GeneratePages(response))
                {
                    yield return page;
                }
            }
        }
    }
}
