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
            ExportEvaluation(workbook, emp.Code, BuildEmployeeSheetTitle(emp));

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

        ExportEvaluation(workbook, emp.Code, BuildEmployeeSheetTitle(emp));

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

        ExportEvaluation(workbook, SYSTEM_EVALUATION_CODE, BuildSystemSheetTitle());

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

        ExportEvaluation(workbook, SYSTEM_EVALUATION_CODE, BuildSystemSheetTitle());

        var employees = EmployeeService.LoadEmployees();

        foreach (var emp in employees)
            ExportEvaluation(workbook, emp.Code, BuildEmployeeSheetTitle(emp));

        if (workbook.Worksheets.Count != employees.Count)
        {
            MessageBox.Show("لم يتم تقييم النظام والموظفين.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        workbook.SaveAs(fileName);
        return true;
    }

    public static bool TryExportTeamLeadCombinedAllReports(string folderPath)
    {
        return TryExportCombinedFullSurveyInternal(folderPath, includeAssistantReadinessSheet: true);
    }

    public static bool TryExportCombinedMemberFullSurvey(string systemEvaluationExcelPath, IReadOnlyList<string> employeeEvaluationExcelPaths)
    {
        if (!File.Exists(systemEvaluationExcelPath) || employeeEvaluationExcelPaths.Count == 0)
            return false;

        var systemModel = new SystemEvaluationrResult(SYSTEM_EVALUATION_CODE, ConfigLoader.LoadSystemSections());
        if (!TryLoadEvaluationFromExcel(systemEvaluationExcelPath, SYSTEM_EVALUATION_CODE, systemModel))
            return false;

        var employeeSheets = new List<(Employee Employee, EvaluationResult Eval)>();
        foreach (var employeeFile in employeeEvaluationExcelPaths.Where(File.Exists))
        {
            if (!TryResolveEmployeeFromExcel(employeeFile, out var employee))
                return false;

            var eval = new EvaluationResult(employee.Code, true, ConfigLoader.LoadEmployeeSections(employee));
            if (!TryLoadEvaluationFromExcel(employeeFile, employee.Code, eval))
                return false;

            employeeSheets.Add((employee, eval));
        }

        if (employeeSheets.Count == 0)
            return false;

        using var workbook = new XLWorkbook();
        WriteSystemEvaluationSheet(workbook, BuildSystemSheetTitle(), systemModel);
        foreach (var employee in employeeSheets)
            WriteEmployeeEvaluationSheet(workbook, BuildEmployeeSheetTitle(employee.Employee), employee.Eval);

        workbook.SaveAs(BuildDesktopExportPath(SPRINT_FULL_SURVEY_FILE_NAME));
        return true;
    }

    public static bool TryExportCombinedFullSurvey(string folderPath)
    {
        return TryExportCombinedFullSurveyInternal(folderPath, includeAssistantReadinessSheet: false);
    }

    private static bool TryExportCombinedFullSurveyInternal(string folderPath, bool includeAssistantReadinessSheet)
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
            resultSheet.Cell(1, finalRateColumn).Value = "Summary";

            var options = isSystemSheet ? ConfigLoader.LoadSystemOptions() : ConfigLoader.LoadEmployeeOptions();
            var context = new ScoringFormulaContext(options.Scoring, useCombinedFormulas: true);
            var questionFinalRateByLabel = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var sectionFinalRateByName = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            for (int row = 2; row <= baseRows.Count; row++)
            {
                string label = resultSheet.Cell(row, COLUMN_LABEL).GetString().Trim();
                if (string.IsNullOrWhiteSpace(label))
                    continue;

                if (questionToSection.TryGetValue(label, out var sectionName) && sectionByName.TryGetValue(sectionName, out var sectionForQuestion))
                {
                    var question = sectionForQuestion.Questions.FirstOrDefault(q => q.Include && string.Equals(q.Text, label, StringComparison.OrdinalIgnoreCase));
                    if (question != null)
                    {
                        var scores = new List<double>();
                        for (int col = firstDataColumn; col <= lastDataColumn; col++)
                            if (resultSheet.Cell(row, col).TryGetValue<double>(out var v))
                                scores.Add(v);

                        question.Score = scores.Count == 0 ? question.Default : scores.Average();
                        double questionFinal = EvaluationResult.ScoreQuestion(question, options.Scoring, true, scores);
                        resultSheet.Cell(row, finalRateColumn).Value = questionFinal;
                        questionFinalRateByLabel[label] = questionFinal;
                    }
                }
                else if (string.Equals(label, LABEL_SECTION_TOTAL, StringComparison.OrdinalIgnoreCase))
                {
                    sectionName = FindCurrentSectionName(resultSheet, row);
                    if (sectionByName.TryGetValue(sectionName, out var section))
                    {
                        var includedQuestions = section.Questions.Where(q => q.Include).ToList();
                        var questionScores = includedQuestions.Select(q => questionFinalRateByLabel.TryGetValue(q.Text, out var s) ? s : q.Default).ToList();
                        double sectionScore = ComputeSectionScore(section, questionScores, context);
                        resultSheet.Cell(row, finalRateColumn).Value = sectionScore;
                        sectionFinalRateByName[section.Name] = sectionScore;
                    }
                }
                else if (string.Equals(label, LABEL_TOTAL, StringComparison.OrdinalIgnoreCase))
                {
                    var scoredSections = templateSections
                        .Where(s => s.Include)
                        .Select(s => (Section: s, Score: sectionFinalRateByName.TryGetValue(s.Name, out var score) ? score : 0d))
                        .ToList();

                    resultSheet.Cell(row, finalRateColumn).Value = ComputeTotalScore(scoredSections, options.Scoring, useCombined: true);
                }
            }

            resultSheet.Columns().AdjustToContents();
        }

        if (includeAssistantReadinessSheet)
            AppendAssistantReadinessSheet(resultWorkbook);

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


    private static double ComputeSectionScore(Section section, List<double> questionScores, ScoringFormulaContext context)
    {
        var includedQuestions = section.Questions.Where(q => q.Include).ToList();
        var questionWeights = includedQuestions.Select(q => q.Weight).ToList();

        double fallback = 0;
        double denominator = questionWeights.Sum();
        if (denominator > 0)
            fallback = includedQuestions.Zip(questionScores, (q, s) => q.Weight * s).Sum() / denominator;

        string? formula = context.useCombinedFormulas
            ? (section.CombinedFormula ?? context.scoring.CombinedSectionFormula ?? section.Formula ?? context.scoring.SectionFormula)
            : (section.Formula ?? context.scoring.SectionFormula);

        return FormulaEngine.EvaluateToScalar(formula,
            new Dictionary<string, FormulaEngine.Value>
            {
                ["QuestionScore"] = new FormulaEngine.Value(questionScores),
                ["QuestionWeight"] = new FormulaEngine.Value(questionWeights),
                ["QuestionCount"] = new FormulaEngine.Value(questionScores.Count),
                ["SectionWeight"] = new FormulaEngine.Value(section.Weight)
            },
            fallback);
    }

    private static double ComputeTotalScore(List<(Section Section, double Score)> sectionScores, ScoringOptions scoring, bool useCombined)
    {
        var scores = sectionScores.Select(x => x.Score).ToList();
        var weights = sectionScores.Select(x => (double)x.Section.Weight).ToList();

        double fallback = 0;
        double denominator = weights.Sum();
        if (denominator > 0)
            fallback = sectionScores.Sum(x => x.Score * x.Section.Weight) / denominator;

        string formula = useCombined ? scoring.CombinedTotalFormula : scoring.TotalFormula;

        return FormulaEngine.EvaluateToScalar(formula,
            new Dictionary<string, FormulaEngine.Value>
            {
                ["SectionScore"] = new FormulaEngine.Value(scores),
                ["SectionWeight"] = new FormulaEngine.Value(weights),
                ["SectionCount"] = new FormulaEngine.Value(scores.Count)
            },
            fallback);
    }

    private static string BuildDesktopExportPath(string reportFileName)
    {
        string prefix = $"{AuthService.CurrentUser.Name} [{AuthService.CurrentUser.Code}] - ";
        return Path.Combine(DesktopPath, $"{prefix}{reportFileName}");
    }

    private static void WriteSystemEvaluationSheet(XLWorkbook workbook, string workSheetTitle, SystemEvaluationrResult eval)
    {
        var ws = workbook.Worksheets.Add(GetWorksheetName(workSheetTitle, eval.Code));

        int row = 1;
        ws.Cell(row, COLUMN_LABEL).Value = workSheetTitle;
        ws.Row(row).Style.Font.Bold = true;
        row++;

        foreach (var section in eval.Sections)
        {
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
            ws.Cell(row, COLUMN_VALUE).Value = section.TotalScore;
            ws.Row(row).Style.Font.Bold = true;
            row++;
        }

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_TOTAL;
        ws.Cell(row, COLUMN_VALUE).Value = eval.TotalScore;
        ws.Row(row).Style.Font.Bold = true;
        row++;

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_NOTES;
        ws.Cell(row, COLUMN_VALUE).Value = eval.FinalNote;
        ws.Row(row).Style.Font.Bold = true;
        ws.Columns().AdjustToContents();
    }

    private static void WriteEmployeeEvaluationSheet(XLWorkbook workbook, string workSheetTitle, EvaluationResult eval)
    {
        var ws = workbook.Worksheets.Add(GetWorksheetName(workSheetTitle, eval.Code));

        int row = 1;
        ws.Cell(row, COLUMN_LABEL).Value = workSheetTitle;
        ws.Row(row).Style.Font.Bold = true;
        row++;

        foreach (var section in eval.Sections)
        {
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
            ws.Cell(row, COLUMN_VALUE).Value = section.TotalScore;
            ws.Row(row).Style.Font.Bold = true;
            row++;
        }

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_TOTAL;
        ws.Cell(row, COLUMN_VALUE).Value = eval.TotalScore;
        ws.Row(row).Style.Font.Bold = true;
        row++;

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_NOTES;
        ws.Cell(row, COLUMN_VALUE).Value = eval.FinalNote;
        ws.Row(row).Style.Font.Bold = true;

        if (ShouldExportAssistantField())
        {
            row++;
            ws.Cell(row, COLUMN_LABEL).Value = LABEL_TEAM_LEADER_ASSISTANT;
            ws.Cell(row, COLUMN_VALUE).Value = eval.RecommendAsTeamLead ?
                LABEL_TEAM_LEADER_ASSISTANT_YES : LABEL_TEAM_LEADER_ASSISTANT_NO;
            ws.Row(row).Style.Font.Bold = true;
        }

        ws.Columns().AdjustToContents();
    }

    private static void AppendAssistantReadinessSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add("Assistant Readiness");
        sheet.Cell(1, 1).Value = "Employee";
        sheet.Cell(1, 2).Value = "Recommended Count";
        sheet.Cell(1, 3).Value = "Ready For Team Lead Assistant";
        sheet.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var ws in workbook.Worksheets.Where(w => !w.Name.Contains(SYSTEM_EVALUATION_CODE, StringComparison.OrdinalIgnoreCase) && !string.Equals(w.Name, "Assistant Readiness", StringComparison.OrdinalIgnoreCase)))
        {
            int yesCount = 0;
            int headerCol = ws.FirstRowUsed()?.LastCellUsed()?.Address.ColumnNumber ?? 2;
            for (int col = 2; col <= headerCol; col++)
            {
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
                for (int r = 1; r <= lastRow; r++)
                {
                    var label = ws.Cell(r, 1).GetString();
                    if (!label.Contains("assistant", StringComparison.OrdinalIgnoreCase) && !label.Contains("مساعد"))
                        continue;

                    var value = ws.Cell(r, col).GetString();
                    if (value.Contains("yes", StringComparison.OrdinalIgnoreCase) || value.Contains("نعم"))
                        yesCount++;
                }
            }

            sheet.Cell(row, 1).Value = ws.Name;
            sheet.Cell(row, 2).Value = yesCount;
            sheet.Cell(row, 3).Value = yesCount > 0 ? "Ready" : "Not Ready";
            row++;
        }

        sheet.Columns().AdjustToContents();
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
        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;

        for (int row = 1; row <= lastRow; row++)
        {
            string key = sheet.Cell(row, COLUMN_LABEL).GetString().Trim();
            if (string.IsNullOrWhiteSpace(key))
                continue;

            string value = sheet.Cell(row, COLUMN_VALUE).GetString();
            rows.Add(new KeyValuePair<string, string>(key, value));
        }

        return rows;
    }

    private static void LoadRows(IXLWorksheet sheet, Dictionary<string, Question> textToQuestion, Action<string> setNote, Action<bool> setAssistant)
    {
        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
        for (int row = 1; row <= lastRow; row++)
        {
            string label = sheet.Cell(row, COLUMN_LABEL).GetString().Trim();
            if (string.IsNullOrWhiteSpace(label))
                continue;

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

        var ws = workbook.Worksheets.Add(GetWorksheetName(workSheetTitle, evalCode));

        int row = 1;
        ws.Cell(row, COLUMN_LABEL).Value = workSheetTitle;
        ws.Row(row).Style.Font.Bold = true;
        row++;

        foreach (var section in eval.Sections)
        {
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
            ws.Cell(row, COLUMN_VALUE).Value = section.TotalScore;

            ws.Row(row).Style.Font.Bold = true;
            row++;
        }

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_TOTAL;
        ws.Cell(row, COLUMN_VALUE).Value = eval.TotalScore;

        ws.Row(row).Style.Font.Bold = true;
        row++;

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_NOTES;
        ws.Cell(row, COLUMN_VALUE).Value = eval.FinalNote;
        ws.Row(row).Style.Font.Bold = true;

        if (ShouldExportAssistantField())
        {
            row++;
            ws.Cell(row, COLUMN_LABEL).Value = LABEL_TEAM_LEADER_ASSISTANT;
            ws.Cell(row, COLUMN_VALUE).Value = eval.RecommendAsTeamLead ?
                   LABEL_TEAM_LEADER_ASSISTANT_YES : LABEL_TEAM_LEADER_ASSISTANT_NO;
            ws.Row(row).Style.Font.Bold = true;
        }

        ws.Columns().AdjustToContents();
    }


    private static string BuildSystemSheetTitle()
    {
        return SYSTEM_EVALUATION_CODE;
    }

    private static string BuildEmployeeSheetTitle(Employee employee)
    {
        return $"{employee.Name} - {employee.Code}";
    }

    private static string GetWorksheetName(string title, string code)
    {
        var raw = $"{title} - {code}";
        return raw.Length <= 31 ? raw : raw[..31];
    }

    private static bool TryResolveEmployeeFromExcel(string excelPath, out Employee employee)
    {
        employee = null;
        var employees = EmployeeService.LoadEmployees();

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets
            .FirstOrDefault(ws => !ws.Name.Contains(SYSTEM_EVALUATION_CODE, StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.FirstOrDefault();

        if (sheet == null)
            return false;

        var probeTexts = new[]
        {
            sheet.Name,
            sheet.Cell(1, COLUMN_LABEL).GetString(),
            sheet.Cell(1, COLUMN_VALUE).GetString()
        };

        foreach (var text in probeTexts)
        {
            employee = employees.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Code) && text.Contains(e.Code, StringComparison.OrdinalIgnoreCase));
            if (employee != null)
                return true;
        }

        return false;
    }

    private static bool ShouldExportAssistantField()
    {
        if (AuthService.CurrentUser.IsTeamLead)
            return false;

        var options = ConfigLoader.LoadEmployeeOptions();
        return options.AskPreferTeamLeaderAssistant;
    }

    private static void ExportSystemEvaluation(XLWorkbook workbook, string evalCode, string workSheetTitle)
    {
        var eval = EvaluationService.LoadSystemEvaluation();
        if (eval == null) return;

        var ws = workbook.Worksheets.Add(GetWorksheetName(workSheetTitle, evalCode));

        int row = 1;
        ws.Cell(row, COLUMN_LABEL).Value = workSheetTitle;
        ws.Row(row).Style.Font.Bold = true;
        row++;

        foreach (var section in eval.Sections)
        {
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
            ws.Cell(row, COLUMN_VALUE).Value = section.TotalScore;

            ws.Row(row).Style.Font.Bold = true;
            row++;
        }

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_TOTAL;
        ws.Cell(row, COLUMN_VALUE).Value = eval.TotalScore;

        ws.Row(row).Style.Font.Bold = true;
        row++;

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_NOTES;
        ws.Cell(row, COLUMN_VALUE).Value = eval.FinalNote;
        ws.Row(row).Style.Font.Bold = true;

        if (ShouldExportAssistantField())
        {
            row++;
            ws.Cell(row, COLUMN_LABEL).Value = LABEL_TEAM_LEADER_ASSISTANT;
            ws.Cell(row, COLUMN_VALUE).Value = eval.RecommendAsTeamLead ?
                 LABEL_TEAM_LEADER_ASSISTANT_YES : LABEL_TEAM_LEADER_ASSISTANT_NO;
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


}
