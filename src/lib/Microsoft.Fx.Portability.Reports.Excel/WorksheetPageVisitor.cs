// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Fx.OpenXmlExtensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Reports
{
    internal class WorksheetPageVisitor : PageVisitor
    {
        private readonly Wrapper _ws;

        public WorksheetPageVisitor(Worksheet ws)
        {
            _ws = new Wrapper(ws);
        }

        public override void Visit(Table table)
        {
            using (var t = _ws.CreateTable(table.Headers))
            {
                foreach (var row in table.Rows)
                {
                    _ws.AddRow(row.Data);
                }
            }

            if (table.ColumnWidth.Any() && !_columnWidthSet)
            {
                _ws.SetWidth(table.ColumnWidth);
                _columnWidthSet = true;
            }
        }

        private bool _columnWidthSet = false;

        public override void Visit(Divider divider)
        {
            _ws.AddRow();
        }

        private class Wrapper
        {
            private readonly Worksheet _ws;
            private int _count;

            public Wrapper(Worksheet ws)
            {
                _ws = ws;
                _count = 1;
            }

            public void AddRow(IReadOnlyCollection<object> items)
            {
                _count++;
                _ws.AddRow(items);
            }

            public void AddRow()
            {
                _count++;
                _ws.AddRow();
            }

            public void SetWidth(IReadOnlyCollection<double> widths)
            {
                _ws.AddColumnWidth(widths);
            }

            public IDisposable CreateTable(IReadOnlyCollection<string> headers)
                => new TableCreator(this, headers);

            private class TableCreator : IDisposable
            {
                private readonly Wrapper _wrapper;
                private readonly int _start;
                private readonly IReadOnlyCollection<string> _headers;

                public TableCreator(Wrapper wrapper, IReadOnlyCollection<string> headers)
                {
                    _wrapper = wrapper;
                    _start = _wrapper._count;
                    _headers = headers;

                    if (headers.Any())
                    {
                        _wrapper.AddRow(headers);
                    }
                }

                public void Dispose()
                {
                    if (_headers.Any())
                    {
                        var tableRowCount = _wrapper._count - _start;
                        _wrapper._ws.AddTable(_start, tableRowCount, 1, _headers);
                    }
                }
            }
        }
    }
}
