// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Reports
{
    public class Row
    {
        public Row()
        {
        }

        public Row(IReadOnlyCollection<object> data)
        {
            Data = data;
        }

        public Row(params object[] data)
        {
            Data = data;
        }

        public IReadOnlyCollection<object> Data { get; set; }
    }
}
