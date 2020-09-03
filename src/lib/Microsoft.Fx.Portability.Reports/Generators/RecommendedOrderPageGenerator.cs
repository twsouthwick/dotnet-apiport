// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reports.Resources;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Reports.Generators
{
    public class RecommendedOrderPageGenerator : IPageGenerator
    {
        private static readonly double[] ColumnWidth = new[] { 100D };

        public IEnumerable<Page> GeneratePages(AnalyzeResponse response)
        {
            if (response.RecommendedOrder.Any())
            {
                yield return new Page
                {
                    Title = LocalizedStrings.RecommendedOrderHeader,
                    Content = new Content[]
                    {
                        new Text(LocalizedStrings.RecommendedOrderDetails),
                        GenerateOrderPage(response)
                    }
                };
            }
        }

        private static Table GenerateOrderPage(AnalyzeResponse response) => new Table
        {
            Headers = new[] { LocalizedStrings.AssemblyHeader },
            Rows = response.RecommendedOrder.Select(o => new Row(o)).ToList(),
            ColumnWidth = ColumnWidth,
        };
    }
}
