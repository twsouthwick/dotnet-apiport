// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Fx.OpenXmlExtensions;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Reports
{
    public class ExcelReportWriter : IReportWriter
    {
        public ExcelReportWriter(IReportGenerator2 generator)
        {
            _generator = generator;
            Format = new ResultFormatInformation
            {
                DisplayName = "Excel",
                MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileExtension = ".xlsx"
            };
        }

        private readonly IReportGenerator2 _generator;

        public ResultFormatInformation Format { get; }

        public Task WriteStreamAsync(Stream stream, AnalyzeResponse response)
        {
            var pages = _generator.GeneratePages(response);

            // Writing directly to the stream can cause problems if it is a BufferedStream (as seen when writing a multipart response)
            // This will write the spreadsheet to a temporary stream, and then copy it to the expected stream afterward
            using (var ms = new MemoryStream())
            {
                using (var spreadsheet = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
                {
                    spreadsheet.AddWorkbookPart();
                    spreadsheet.WorkbookPart.Workbook = new Workbook();

                    AddStylesheet(spreadsheet.WorkbookPart);

                    foreach (var page in pages)
                    {
                        var ws = new WorksheetPageVisitor(spreadsheet.AddWorksheet(page.Title));

                        ws.Visit(page);
                    }
                }

                ms.Position = 0;
                return ms.CopyToAsync(stream);
            }
        }

        private void AddStylesheet(WorkbookPart wb)
        {
            var cellstyle = new CellStyle { Name = "Normal", FormatId = 0U, BuiltinId = 0U };
            var border = new Border(new LeftBorder(), new RightBorder(), new TopBorder(), new BottomBorder(), new DiagonalBorder());

            var fill1 = new Fill(new PatternFill { PatternType = PatternValues.None });
            var fill2 = new Fill(new PatternFill { PatternType = PatternValues.Gray125 });

            var format1 = new CellFormat { FontId = 0U };
            var format2 = new CellFormat { FontId = 1U, ApplyFont = true };

            var textFont = new Font(
                new FontSize { Val = 11D },
                new Color { Theme = 1U },
                new FontName { Val = "Calibri" },
                new FontFamilyNumbering { Val = 2 },
                new FontScheme { Val = FontSchemeValues.Minor });

            var hyperlinkFont = new Font(
                new Underline(),
                new FontSize { Val = 11D },
                new Color { Theme = 10U },
                new FontName { Val = "Calibri" },
                new FontFamilyNumbering { Val = 2 },
                new FontScheme { Val = FontSchemeValues.Minor });

            var stylesheet = new Stylesheet
            {
                Fonts = new Fonts(textFont, hyperlinkFont),
                CellFormats = new CellFormats(format1, format2),
                Fills = new Fills(fill1, fill2),
                CellStyles = new CellStyles(cellstyle),
                Borders = new Borders(border),
            };

            wb.AddNewPart<WorkbookStylesPart>();
            wb.WorkbookStylesPart.Stylesheet = stylesheet;
        }
    }
}
