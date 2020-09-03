// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Reports
{
    public class Table : Content
    {
        public IReadOnlyCollection<string> Headers { get; set; } = Array.Empty<string>();

        public IReadOnlyCollection<Row> Rows { get; set; } = Array.Empty<Row>();

        public IReadOnlyCollection<double> ColumnWidth { get; set; } = Array.Empty<double>();
    }
}
