using ClosedXML.Excel;

internal static class GoatSheetStyling
{
    public static void CreateGoatSheetWithSummary(XLWorkbook wb, Employee employee, IEvaluation evaluation)
    {
        // Replace existing THE GOAT sheet (so reruns are clean)
        if (wb.Worksheets.Contains("THE GOAT"))
            wb.Worksheet("THE GOAT").Delete();

        var ws = wb.Worksheets.Add("THE GOAT");

        // ===== Sheet-level polish =====
        ws.TabColor = XLColor.FromColor(Color.FromArgb(0xC6, 0x93, 0x00)); // gold tab
        ws.SheetView.FreezeRows(4);
        ws.SheetView.ZoomScale = 160; 

        ws.PageSetup.CenterHorizontally = true;
        ws.PageSetup.CenterVertically = true;
        ws.PageSetup.PagesWide = 1;
        ws.PageSetup.PagesTall = 0;

        // Make a nice, centered "card" area (A:B)
        ws.Column(1).Width = 36; 
        ws.Column(2).Width = 18; 


        // ===== Header banner =====
        ws.Row(1).Height = 30;
        var header = ws.Range("A1:B1");
        header.Merge().Value = "🏆 THE GOAT";
        header.Style
            .Font.SetBold()
            .Font.SetFontSize(18)
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            .Fill.SetBackgroundColor(XLColor.FromColor(Color.FromArgb(0x1B, 0x5E, 0x20)));

        // ===== Employee Name (big centered) =====
        ws.Row(2).Height = 48;
        var nameRange = ws.Range("A2:B2");
        nameRange.Merge().Value = employee.Title;
        nameRange.Style
            .Font.SetBold()
            .Font.SetFontSize(32)
            .Font.SetFontColor(XLColor.FromColor(Color.FromArgb(0x1B, 0x5E, 0x20)))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);


        // ===== Subtitle =====
        ws.Row(3).Height = 22;
        var sub = ws.Range("A3:B3");
        sub.Merge().Value = "Performance Summary";
        sub.Style
            .Font.SetItalic()
            .Font.SetFontSize(13)
            .Font.SetFontColor(XLColor.Gray)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // Small spacing row
        ws.Row(4).Height = 10;


        // ===== Summary table =====
        int startRow = 5;

        ws.Cell(startRow, 1).Value = "Section";
        ws.Cell(startRow, 2).Value = "Total Score";

        var tableHeader = ws.Range(startRow, 1, startRow, 2);
        tableHeader.Style
            .Font.SetBold()
            .Font.SetFontColor(XLColor.White);
        tableHeader.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        tableHeader.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        tableHeader.Style.Fill.SetBackgroundColor(XLColor.FromColor(Color.FromArgb(0x2E, 0x7D, 0x32)));
        tableHeader.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableHeader.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Data rows
        int r = startRow + 1;
        var entities = evaluation.ReadonlyEntities;
        for (int i = 0; i < entities.Count; i++)
        {
            var s = entities[i];

            ws.Cell(r, 1).Value = s.RootConfig.Value.Title;
            ws.Cell(r, 2).Value = s.Score;

            ws.Cell(r, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            ws.Cell(r, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // zebra banding
            if (i % 2 == 0)
                ws.Range(r, 1, r, 2).Style.Fill.SetBackgroundColor(XLColor.FromColor(Color.FromArgb(0xF6, 0xF8, 0xFA)));

            ws.Range(r, 1, r, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(r, 1, r, 2).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            r++;
        }

        // Overall total row (big + gold)
        int totalRowIndex = r + 1;

        ws.Cell(totalRowIndex, 1).Value = "OVERALL TOTAL";
        ws.Cell(totalRowIndex, 1).Style
            .Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

        ws.Cell(totalRowIndex, 2).Value = evaluation.TotalScore;
        ws.Cell(totalRowIndex, 2).Style
            .Font.SetBold()
            .Font.SetFontSize(20)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        var totalRow = ws.Range(totalRowIndex, 1, totalRowIndex, 2);
        totalRow.Style.Fill.SetBackgroundColor(XLColor.FromColor(Color.FromArgb(0xFF, 0xF3, 0xB0))); // light gold
        totalRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
        totalRow.Style.Border.OutsideBorderColor = XLColor.FromColor(Color.FromArgb(0xC6, 0x93, 0x00));
        totalRow.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // Make whole "card" area centered nicely
        ws.Column(2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // Apply a consistent font to the used range
        var used = ws.RangeUsed();
        if (used != null)
        {
            used.Style.Font.SetFontName("Calibri").Font.SetFontSize(11);
            used.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        }

        // Optional: give some extra whitespace below
        ws.Row(totalRowIndex + 2).Height = 10;
    }
    public static void DecorateGoatSheet(IXLWorksheet ws, int? totalScoreRow = null, int? totalScoreCol = null)
    {
        ws.TabColor = XLColor.FromColor(Color.FromArgb(0x2E, 0x7D, 0x32));
        ws.Row(1).InsertRowsAbove(1);
        int lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 12;

        var banner = ws.Range(1, 1, 1, lastCol);
        banner.Merge();
        banner.Value = "🏆 TOP PERFORMER (GOAT)";

        ws.Row(1).Height = 26;
        banner.Style
            .Font.SetBold()
            .Font.SetFontSize(14)
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            .Fill.SetBackgroundColor(XLColor.FromColor(Color.FromArgb(0x1B, 0x5E, 0x20)));

        ws.SheetView.FreezeRows(1);

        ws.PageSetup.PagesWide = 1;
        ws.PageSetup.PagesTall = 0;

        if (totalScoreRow.HasValue && totalScoreCol.HasValue)
        {
            var c = ws.Cell(totalScoreRow.Value + 1, totalScoreCol.Value);
            // +1 because we inserted a banner row above (shifted original rows down)

            c.Style.Font.SetBold().Font.SetFontSize(14);
            c.Style.NumberFormat.SetFormat("0.00");
            c.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            c.Style.Fill.SetBackgroundColor(XLColor.FromColor(Color.FromArgb(0xFF, 0xF3, 0xB0))); // light gold

            c.Style.Border.TopBorder = XLBorderStyleValues.Thick;
            c.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
            c.Style.Border.LeftBorder = XLBorderStyleValues.Thick;
            c.Style.Border.RightBorder = XLBorderStyleValues.Thick;

            var gold = XLColor.FromColor(Color.FromArgb(0xC6, 0x93, 0x00));
            c.Style.Border.TopBorderColor = gold;
            c.Style.Border.BottomBorderColor = gold;
            c.Style.Border.LeftBorderColor = gold;
            c.Style.Border.RightBorderColor = gold;
        }

        var used = ws.RangeUsed();
        if (used != null)
        {
            used.Style.Font.SetFontName("Calibri").Font.SetFontSize(11);
            used.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        }
    }
}