using ClosedXML.Excel;
using Evaluation_App.Services;
using static Constants;

public static class ExcelExportService
{
    public static bool ExportTeamMembers()
    {
        var employees = EmployeeService.LoadEmployees();

        string fileName = BuildDesktopExportPath(TEAM_MEMBERS_REPORT_FILE_NAME);
        using var workbook = new XLWorkbook();

        foreach (var emp in employees)
            ExportEvaluation(workbook, emp.Code, $"{WORKSHEET_EMPLOYEE_TITLE_PREFIX}{emp.Name}");

        if (workbook.Worksheets.Count != employees.Count - 1)
        {
            MessageBox.Show("لم يتم تقييم جميع الموظفين .", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        workbook.SaveAs(fileName);
        return true;
    }

    public static bool ExportTeamMember(Employee emp)
    {
        string fileName = BuildDesktopExportPath($"{emp.Name} Report.xlsx");
        using var workbook = new XLWorkbook();

        ExportEvaluation(workbook, emp.Code, $"{WORKSHEET_EMPLOYEE_TITLE_PREFIX}{emp.Name}");

        if (workbook.Worksheets.Count != 1)
        {
            MessageBox.Show("لم يتم تقييم الموظف.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        workbook.SaveAs(fileName);
        return true;
    }

    public static bool ExportSystemEvaluation()
    {
        string fileName = BuildDesktopExportPath(SYSTEM_EVALUATION_FILE_NAME);
        using var workbook = new XLWorkbook();

        ExportEvaluation(workbook, SYSTEM_EVALUATION_CODE, WORKSHEET_SYSTEM_TITLE);

        if (workbook.Worksheets.Count != 1)
        {
            MessageBox.Show("لم يتم تقييم النظام.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        workbook.SaveAs(fileName);
        return true;
    }

    public static bool TryExportFullReport()
    {
        string fileName = BuildDesktopExportPath(FULL_SURVEY_FILE_NAME);
        using var workbook = new XLWorkbook();

        ExportEvaluation(workbook, SYSTEM_EVALUATION_CODE, WORKSHEET_SYSTEM_TITLE);

        var employees = EmployeeService.LoadEmployees();

        foreach (var emp in employees)
            ExportEvaluation(workbook, emp.Code, $"{WORKSHEET_EMPLOYEE_TITLE_PREFIX}{emp.Name}");

        if (workbook.Worksheets.Count != employees.Count)
        {
            MessageBox.Show("لم يتم تقييم النظام والموظفين.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        workbook.SaveAs(fileName);
        return true;
    }

    public static bool TryExportCombinedFullSurvey(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return false;

        var files = Directory.GetFiles(folderPath, "*.xlsx")
            .Where(path => !Path.GetFileName(path).StartsWith("~$"))
            .OrderBy(path => path)
            .ToList();

        if (files.Count == 0)
            return false;

        using var firstWorkbook = new XLWorkbook(files[0]);
        int sheetCount = firstWorkbook.Worksheets.Count;

        if (sheetCount == 0)
            return false;

        for (int fileIndex = 1; fileIndex < files.Count; fileIndex++)
        {
            using var workbook = new XLWorkbook(files[fileIndex]);
            if (workbook.Worksheets.Count != sheetCount)
                return false;

            for (int sheetIndex = 1; sheetIndex <= sheetCount; sheetIndex++)
            {
                if (!HasSameFirstColumn(firstWorkbook.Worksheet(sheetIndex), workbook.Worksheet(sheetIndex)))
                    return false;
            }
        }

        using var resultWorkbook = new XLWorkbook();

        for (int sheetIndex = 1; sheetIndex <= sheetCount; sheetIndex++)
        {
            var baseSheet = firstWorkbook.Worksheet(sheetIndex);
            var resultSheet = resultWorkbook.Worksheets.Add(baseSheet.Name);
            bool isSystemSheet = baseSheet.Name.Contains("System", StringComparison.OrdinalIgnoreCase) || baseSheet.Name.Contains(SYSTEM_EVALUATION_CODE, StringComparison.OrdinalIgnoreCase);
            var templateSections = isSystemSheet ? ConfigLoader.LoadSystemSections() : ConfigLoader.LoadEmployeeSections();
            var sectionByName = templateSections.ToDictionary(s => s.Name, s => s, StringComparer.OrdinalIgnoreCase);
            var questionToSection = templateSections
                .SelectMany(s => s.Questions.Where(q => q.Include).Select(q => new { q.Text, SectionName = s.Name }))
                .ToDictionary(x => x.Text, x => x.SectionName, StringComparer.OrdinalIgnoreCase);

            var baseRows = GetSheetRows(baseSheet);
            for (int row = 0; row < baseRows.Count; row++)
                resultSheet.Cell(row + 1, COLUMN_LABEL).Value = baseRows[row].Key;

            for (int fileIndex = 0; fileIndex < files.Count; fileIndex++)
            {
                using var sourceWorkbook = new XLWorkbook(files[fileIndex]);
                var sourceSheet = sourceWorkbook.Worksheet(sheetIndex);
                var sourceRows = GetSheetRows(sourceSheet)
                    .ToDictionary(o => o.Key, o => o.Value, StringComparer.OrdinalIgnoreCase);

                int targetColumn = fileIndex + 2;
                resultSheet.Cell(1, targetColumn).Value = Path.GetFileNameWithoutExtension(files[fileIndex]);

                for (int row = 1; row < baseRows.Count; row++)
                {
                    var key = baseRows[row].Key;
                    if (sourceRows.TryGetValue(key, out string? value))
                        resultSheet.Cell(row + 1, targetColumn).Value = value;
                }
            }

            int firstDataColumn = 2;
            int lastDataColumn = files.Count + 1;
            int finalRateColumn = lastDataColumn + 1;
            resultSheet.Cell(1, finalRateColumn).Value = "Final Rate";

            var finalRateByLabel = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int row = 2; row <= baseRows.Count; row++)
            {
                string label = resultSheet.Cell(row, COLUMN_LABEL).GetString().Trim();
                if (string.IsNullOrWhiteSpace(label))
                    continue;

                string startAddress = resultSheet.Cell(row, firstDataColumn).Address.ToStringRelative();
                string endAddress = resultSheet.Cell(row, lastDataColumn).Address.ToStringRelative();

                if (questionToSection.ContainsKey(label))
                {
                    resultSheet.Cell(row, finalRateColumn).FormulaA1 = $"IFERROR(AVERAGE({startAddress}:{endAddress}),0)";
                }
                else if (string.Equals(label, LABEL_SECTION_TOTAL, StringComparison.OrdinalIgnoreCase))
                {
                    string sectionName = FindCurrentSectionName(resultSheet, row);
                    if (sectionByName.TryGetValue(sectionName, out var section))
                    {
                        var includedQuestions = section.Questions.Where(q => q.Include).ToList();
                        string numerator = string.Join("+", includedQuestions
                            .Where(q => finalRateByLabel.ContainsKey(q.Text))
                            .Select(q => $"{resultSheet.Cell(finalRateByLabel[q.Text], finalRateColumn).Address.ToStringRelative()}*{q.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
                        double denominator = includedQuestions.Sum(q => q.Weight);
                        if (!string.IsNullOrEmpty(numerator) && denominator > 0)
                            resultSheet.Cell(row, finalRateColumn).FormulaA1 = $"IFERROR(({numerator})/{denominator.ToString(System.Globalization.CultureInfo.InvariantCulture)},0)";
                    }
                }
                else if (string.Equals(label, LABEL_TOTAL, StringComparison.OrdinalIgnoreCase))
                {
                    string numerator = string.Join("+", templateSections
                        .Where(s => s.Include)
                        .Select(s => (Section: s, Row: FindSectionTotalRow(resultSheet, s.Name, finalRateByLabel)))
                        .Where(x => x.Row > 0)
                        .Select(x => $"{resultSheet.Cell(x.Row, finalRateColumn).Address.ToStringRelative()}*{x.Section.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
                    double denominator = templateSections.Where(s => s.Include).Sum(s => s.Weight);
                    if (!string.IsNullOrEmpty(numerator) && denominator > 0)
                        resultSheet.Cell(row, finalRateColumn).FormulaA1 = $"IFERROR(({numerator})/{denominator.ToString(System.Globalization.CultureInfo.InvariantCulture)},0)";
                }

                finalRateByLabel[label] = row;
            }

            resultSheet.Columns().AdjustToContents();
        }

        resultWorkbook.SaveAs(BuildDesktopExportPath(SPRINT_FULL_SURVEY_FILE_NAME));
        return true;
    }

    public static bool TryLoadEvaluationFromExcel(string excelPath, string evalCode, SystemEvaluationrResult destination)
    {
        if (!File.Exists(excelPath))
            return false;

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets
            .FirstOrDefault(ws => ws.Name.Contains(evalCode, StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.FirstOrDefault(ws => ws.Name.Contains("System", StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.FirstOrDefault();

        if (sheet == null)
            return false;

        var textToQuestion = destination.Sections
            .SelectMany(s => s.Questions)
            .Where(q => q.Include)
            .ToDictionary(q => q.Text, q => q, StringComparer.OrdinalIgnoreCase);

        destination.RecommendAsTeamLead = false;
        destination.FinalNote = string.Empty;

        LoadRows(sheet, textToQuestion, note => destination.FinalNote = note, assistant => destination.RecommendAsTeamLead = assistant);

        destination.SetTotalScore();
        return true;
    }

    public static bool TryLoadEvaluationFromExcel(string excelPath, string evalCode, EvaluationResult destination)
    {
        if (!File.Exists(excelPath))
            return false;

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets
            .FirstOrDefault(ws => ws.Name.Contains(evalCode, StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.FirstOrDefault(ws => ws.Name.Contains("Employee", StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.FirstOrDefault();

        if (sheet == null)
            return false;

        var textToQuestion = destination.Sections
            .SelectMany(s => s.Questions)
            .Where(q => q.Include)
            .ToDictionary(q => q.Text, q => q, StringComparer.OrdinalIgnoreCase);

        destination.RecommendAsTeamLead = false;
        destination.FinalNote = string.Empty;

        LoadRows(sheet, textToQuestion, note => destination.FinalNote = note, assistant => destination.RecommendAsTeamLead = assistant);

        destination.SetTotalScore();
        return true;
    }

    private static string BuildDesktopExportPath(string reportFileName)
    {
        string prefix = $"{AuthService.CurrentUser.Name} [{AuthService.CurrentUser.Code}] - ";
        return Path.Combine(DesktopPath, $"{prefix}{reportFileName}");
    }

    private static bool HasSameFirstColumn(IXLWorksheet firstSheet, IXLWorksheet secondSheet)
    {
        var firstRows = GetSheetRows(firstSheet);
        var secondRows = GetSheetRows(secondSheet);

        if (firstRows.Count != secondRows.Count)
            return false;

        for (int i = 0; i < firstRows.Count; i++)
        {
            if (!string.Equals(firstRows[i].Key, secondRows[i].Key, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static List<KeyValuePair<string, string>> GetSheetRows(IXLWorksheet sheet)
    {
        var rows = new List<KeyValuePair<string, string>>();
        int row = 1;

        while (!sheet.Cell(row, COLUMN_LABEL).IsEmpty())
        {
            string key = sheet.Cell(row, COLUMN_LABEL).GetString().Trim();
            string value = sheet.Cell(row, COLUMN_VALUE).GetString();
            rows.Add(new KeyValuePair<string, string>(key, value));
            row++;
        }

        return rows;
    }

    private static void LoadRows(IXLWorksheet sheet, Dictionary<string, Question> textToQuestion, Action<string> setNote, Action<bool> setAssistant)
    {
        int row = 1;
        while (!sheet.Cell(row, COLUMN_LABEL).IsEmpty())
        {
            string label = sheet.Cell(row, COLUMN_LABEL).GetString().Trim();

            if (textToQuestion.TryGetValue(label, out var question))
            {
                var cell = sheet.Cell(row, COLUMN_VALUE);
                if (cell.TryGetValue<double>(out var value))
                    question.Score = Math.Clamp(value, question.Min, question.Max);
            }
            else if (label.Contains(LABEL_NOTES, StringComparison.OrdinalIgnoreCase) || label.Contains("مقترحات") || label.Contains("كلمة"))
            {
                setNote(sheet.Cell(row, COLUMN_VALUE).GetString());
            }
            else if (label.Contains("assistant", StringComparison.OrdinalIgnoreCase) || label.Contains("مساعد"))
            {
                string value = sheet.Cell(row, COLUMN_VALUE).GetString();
                setAssistant(value.Contains("yes", StringComparison.OrdinalIgnoreCase) || value.Contains("نعم"));
            }

            row++;
        }
    }

    private static void ExportEvaluation(XLWorkbook workbook, string evalCode, string workSheetTitle)
    {
        if (string.Equals(evalCode, SYSTEM_EVALUATION_CODE, StringComparison.OrdinalIgnoreCase))
        {
            ExportSystemEvaluation(workbook, evalCode, workSheetTitle);
            return;
        }

        var eval = EvaluationService.LoadEvaluation(evalCode);
        if (eval == null) return;

        var ws = workbook.Worksheets.Add($"{workSheetTitle} - {evalCode}");

        int row = 1;
        ws.Cell(row, COLUMN_LABEL).Value = workSheetTitle;
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        foreach (var section in eval.Sections)
        {
            int sectionQuestionStart = row + 1;
            ws.Cell(row, COLUMN_LABEL).Value = section.Name;
            ws.Cell(row, COLUMN_VALUE).Value = section.NumberMeaning;
            ws.Row(row).Style.Font.Bold = true;
            row++;

            foreach (var question in section.Questions.Where(q => q.Include))
            {
                ws.Cell(row, COLUMN_LABEL).Value = question.Text;
                ws.Cell(row, COLUMN_VALUE).Value = question.Score;
                row++;
            }

            ws.Cell(row, COLUMN_LABEL).Value = LABEL_SECTION_TOTAL;
            int sectionQuestionEnd = row - 1;
            if (sectionQuestionEnd >= sectionQuestionStart)
                ws.Cell(row, COLUMN_VALUE).FormulaA1 = $"IFERROR(AVERAGE(B{sectionQuestionStart}:B{sectionQuestionEnd}),0)";
            else
                ws.Cell(row, COLUMN_VALUE).Value = 0;

            ws.Row(row).Style.Font.Bold = true;
            row += 2;
        }

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_TOTAL;
        var sectionTotalRows = ws.Column(COLUMN_LABEL).CellsUsed()
            .Where(c => string.Equals(c.GetString().Trim(), LABEL_SECTION_TOTAL, StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Address.RowNumber)
            .ToList();

        if (sectionTotalRows.Count > 0)
            ws.Cell(row, COLUMN_VALUE).FormulaA1 = $"IFERROR(AVERAGE({string.Join(",", sectionTotalRows.Select(r => $"B{r}"))}),0)";
        else
            ws.Cell(row, COLUMN_VALUE).Value = 0;

        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_NOTES;
        ws.Cell(row, COLUMN_VALUE).Value = eval.FinalNote;
        ws.Row(row).Style.Font.Bold = true;

        if (ShouldExportAssistantField(eval.RecommendAsTeamLead))
        {
            row++;
            ws.Cell(row, COLUMN_LABEL).Value = LABEL_TEAM_LEADER_ASSISTANT;
            ws.Cell(row, COLUMN_VALUE).Value = LABEL_TEAM_LEADER_ASSISTANT_YES;
            ws.Row(row).Style.Font.Bold = true;
        }

        ws.Columns().AdjustToContents();
    }

    private static bool ShouldExportAssistantField(bool recommendAsTeamLead)
    {
        if (!recommendAsTeamLead || AuthService.CurrentUser.IsTeamLead)
            return false;

        var options = ConfigLoader.LoadEmployeeOptions();
        return options.AskPreferTeamLeaderAssistant;
    }

    private static void ExportSystemEvaluation(XLWorkbook workbook, string evalCode, string workSheetTitle)
    {
        var eval = EvaluationService.LoadSystemEvaluation();
        if (eval == null) return;

        var ws = workbook.Worksheets.Add($"{workSheetTitle} - {evalCode}");

        int row = 1;
        ws.Cell(row, COLUMN_LABEL).Value = workSheetTitle;
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        foreach (var section in eval.Sections)
        {
            int sectionQuestionStart = row + 1;
            ws.Cell(row, COLUMN_LABEL).Value = section.Name;
            ws.Cell(row, COLUMN_VALUE).Value = section.NumberMeaning;
            ws.Row(row).Style.Font.Bold = true;
            row++;

            foreach (var question in section.Questions.Where(q => q.Include))
            {
                ws.Cell(row, COLUMN_LABEL).Value = question.Text;
                ws.Cell(row, COLUMN_VALUE).Value = question.Score;
                row++;
            }

            ws.Cell(row, COLUMN_LABEL).Value = LABEL_SECTION_TOTAL;
            int sectionQuestionEnd = row - 1;
            if (sectionQuestionEnd >= sectionQuestionStart)
                ws.Cell(row, COLUMN_VALUE).FormulaA1 = $"IFERROR(AVERAGE(B{sectionQuestionStart}:B{sectionQuestionEnd}),0)";
            else
                ws.Cell(row, COLUMN_VALUE).Value = 0;

            ws.Row(row).Style.Font.Bold = true;
            row += 2;
        }

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_TOTAL;
        var sectionTotalRows = ws.Column(COLUMN_LABEL).CellsUsed()
            .Where(c => string.Equals(c.GetString().Trim(), LABEL_SECTION_TOTAL, StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Address.RowNumber)
            .ToList();

        if (sectionTotalRows.Count > 0)
            ws.Cell(row, COLUMN_VALUE).FormulaA1 = $"IFERROR(AVERAGE({string.Join(",", sectionTotalRows.Select(r => $"B{r}"))}),0)";
        else
            ws.Cell(row, COLUMN_VALUE).Value = 0;

        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_NOTES;
        ws.Cell(row, COLUMN_VALUE).Value = eval.FinalNote;
        ws.Row(row).Style.Font.Bold = true;

        if (ShouldExportAssistantField(eval.RecommendAsTeamLead))
        {
            row++;
            ws.Cell(row, COLUMN_LABEL).Value = LABEL_TEAM_LEADER_ASSISTANT;
            ws.Cell(row, COLUMN_VALUE).Value = LABEL_TEAM_LEADER_ASSISTANT_YES;
            ws.Row(row).Style.Font.Bold = true;
        }

        ws.Columns().AdjustToContents();
    }

    private static string FindCurrentSectionName(IXLWorksheet sheet, int sectionTotalRow)
    {
        for (int row = sectionTotalRow - 1; row >= 1; row--)
        {
            string label = sheet.Cell(row, COLUMN_LABEL).GetString().Trim();
            if (string.IsNullOrWhiteSpace(label) || string.Equals(label, LABEL_SECTION_TOTAL, StringComparison.OrdinalIgnoreCase))
                continue;

            string value = sheet.Cell(row, COLUMN_VALUE).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(value))
                return label;
        }

        return string.Empty;
    }

    private static int FindSectionTotalRow(IXLWorksheet sheet, string sectionName, Dictionary<string, int> knownRows)
    {
        if (!knownRows.TryGetValue(sectionName, out int sectionNameRow))
            return -1;

        for (int row = sectionNameRow + 1; row <= sheet.LastRowUsed().RowNumber(); row++)
        {
            string label = sheet.Cell(row, COLUMN_LABEL).GetString().Trim();
            if (string.Equals(label, LABEL_SECTION_TOTAL, StringComparison.OrdinalIgnoreCase))
                return row;

        }

        return -1;
    }
}
