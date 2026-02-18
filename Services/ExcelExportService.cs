using ClosedXML.Excel;
using Evaluation_App.Services;
using static Constants;

public static class ExcelExportService
{
    public static bool TryExportEmployeeEvaluation(EmployeeEvaluation eval)
    {
        string fileName = BuildDesktopExportPath(eval.Filename);

        if (eval == null)
            return false;

        using var workbook = new XLWorkbook();
        WriteAndFillEvaluationSheet(workbook, eval.Evaluated.Title,
            WORKSHEET_EMPLOYEE_TITLE_PREFIX + eval.Evaluated.Title, eval, true, true);

        if (workbook.Worksheets.Count != 1)
        {
            MessageBox.Show("لم يتم تقييم الموظف.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        workbook.SaveAs(fileName);
        return true;
    }
    public static bool TryExportTeamMembers()
    {
        string fileName = BuildDesktopExportPath(BuildTeamMembersEvaluationFilename());

        var evulateds = EmployeeService.OtherEmployees;

        using var workbook = new XLWorkbook();
        foreach (var evaluated in evulateds)
        {
            var eval = EvaluationService.
                LoadEvaluation<EmployeeEvaluation>(EvaluationBase.BuildFilename(AuthService.CurrentUser, evaluated));
            WriteAndFillEvaluationSheet(workbook, evaluated.Title,
                WORKSHEET_EMPLOYEE_TITLE_PREFIX + evaluated, eval, true, true);
        }

        if (workbook.Worksheets.Count != evulateds.Count())
        {
            MessageBox.Show("لم يتم تقييم جميع الموظفين .", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        workbook.SaveAs(fileName);
        return true;
    }
    public static bool TryExportSystemEvaluation(SystemEvaluation eval)
    {
        string fileName = BuildDesktopExportPath(eval.Filename);

        if (eval == null)
            return false;

        using var workbook = new XLWorkbook();
        WriteAndFillEvaluationSheet(workbook, SYSTEM_EVALUATION_CODE, WORKSHEET_SYSTEM_TITLE, eval, true, true);

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
        string fileName = BuildDesktopExportPath(BuildFullReportFilename());

        var systemEval = EvaluationService.LoadEvaluation<SystemEvaluation>
            (EvaluationBase.BuildFilename(AuthService.CurrentUser));

        if (systemEval == null)
            return false;

        using var workbook = new XLWorkbook();
        WriteAndFillEvaluationSheet(workbook, SYSTEM_EVALUATION_CODE, WORKSHEET_SYSTEM_TITLE, 
            systemEval, true, true);

        var evaluteds = EmployeeService.OtherEmployees;

        foreach (var evaluated in evaluteds)
        {
            var eval = EvaluationService.LoadEvaluation<EmployeeEvaluation>
                (EvaluationBase.BuildFilename(AuthService.CurrentUser, evaluated));

            if (eval == null)
                return false;

            WriteAndFillEvaluationSheet(workbook, evaluated.Title,
                WORKSHEET_EMPLOYEE_TITLE_PREFIX + evaluated.Title, eval, true, true);
        }

        if (workbook.Worksheets.Count != evaluteds.Count() + 1)
        {
            MessageBox.Show("لم يتم تقييم النظام والموظفين.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        workbook.SaveAs(fileName);
        return true;
    }
    public static bool TryExportExistEvals()
    {
        string fileName = BuildDesktopExportPath(BuildFullReportFilename());

        var systemEval = EvaluationService.LoadEvaluation<SystemEvaluation>
            (EvaluationBase.BuildFilename(AuthService.CurrentUser));

        using var workbook = new XLWorkbook();

        if (systemEval != null)
            WriteAndFillEvaluationSheet(workbook, SYSTEM_EVALUATION_CODE, WORKSHEET_SYSTEM_TITLE,
                systemEval, true, true);

        var evaluteds = EmployeeService.OtherEmployees;

        foreach (var evaluated in evaluteds)
        {
            var eval = EvaluationService.LoadEvaluation<EmployeeEvaluation>
                (EvaluationBase.BuildFilename(AuthService.CurrentUser, evaluated));

            if (eval != null)
                WriteAndFillEvaluationSheet(workbook, evaluated.Title,
                WORKSHEET_EMPLOYEE_TITLE_PREFIX + evaluated.Title, eval, true, true);
        }

        if (workbook.Worksheets.Count == 0)
        {
            MessageBox.Show("ليست هناك تقييمات على الإطلاق.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        workbook.SaveAs(fileName);
        return true;
    }


    public static bool TryLoadSystemEvaluationFromExcel(string excelPath, SystemEvaluation destination)
    {
        if (!File.Exists(excelPath))
            return false;

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(SYSTEM_EVALUATION_CODE));

        if (sheet == null)
            return false;

        var textToQuestion = destination.Sections
            .SelectMany(s => s.Questions)
            .ToDictionary(q => q.Text, q => q, StringComparer.OrdinalIgnoreCase);

        destination.ReadyToBeAssistantTeamLeader = false;
        destination.FinalNote = string.Empty;

        LoadRows(sheet, textToQuestion, note => destination.FinalNote = note,
            assistant => destination.ReadyToBeAssistantTeamLeader = assistant);

        destination.CalculateScore();
        return true;
    }
    public static bool TryLoadEmployeeEvaluationFromExcel(string excelPath, EmployeeEvaluation destination)
    {
        if (!File.Exists(excelPath))
            return false;

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets.
            FirstOrDefault(ws => ws.Name.Equals(destination.Evaluated.Title));

        if (sheet == null)
            return false;

        var textToQuestion = destination.Sections
            .SelectMany(s => s.Questions)
            .ToDictionary(q => q.Text, q => q, StringComparer.OrdinalIgnoreCase);

        destination.RecommendAsTeamLead = false;
        destination.FinalNote = string.Empty;

        LoadRows(sheet, textToQuestion, note => destination.FinalNote = note, assistant => destination.RecommendAsTeamLead = assistant);

        destination.CalculateScore();
        return true;
    }


    public static bool TryExportCombinedMemberFullSurvey(string systemEvaluationExcelPath, IReadOnlyList<string> employeeEvaluationExcelPaths)
    {
        if (!File.Exists(systemEvaluationExcelPath) || employeeEvaluationExcelPaths.Count == 0)
            return false;

        if (!TryGetEvalutorFromTitle(systemEvaluationExcelPath, out Employee evaluator))
            return false;

        string fileName = BuildDesktopExportPath(BuildFullReportFilename(evaluator));

        var systemModel = new SystemEvaluation(evaluator, 
            ConfigLoader.SystemEvaluationConfig.FilterSectionsForEmployee(evaluator));
        if (!TryLoadSystemEvaluationFromExcel(systemEvaluationExcelPath, systemModel))
            return false;

        var section = ConfigLoader.EmployeeEvaluationConfig.FilterSectionsForEmployee(evaluator);
        var employeeEvals = new List<EmployeeEvaluation>();
        foreach (var employeeFile in employeeEvaluationExcelPaths.Where(File.Exists))
        {
            if (!TryGetEvalutorFromTitle(employeeFile, out var temp) || temp.Code != evaluator.Code)
                return false;

            if(!TryGetEvalutedFromTitle(employeeFile, out Employee evaluated))
                return false;

            var eval = new EmployeeEvaluation(evaluator, section, evaluated);
            if (!TryLoadEmployeeEvaluationFromExcel(employeeFile, eval))
                return false;

            employeeEvals.Add(eval);
        }

        if (employeeEvals.Count == 0)
            return false;

        using var workbook = new XLWorkbook();
        WriteAndFillEvaluationSheet(workbook, SYSTEM_EVALUATION_CODE, WORKSHEET_SYSTEM_TITLE,
            systemModel, true, true);
        foreach (var eval in employeeEvals)
        {
            WriteAndFillEvaluationSheet(workbook, eval.Evaluated.Title, 
                WORKSHEET_EMPLOYEE_TITLE_PREFIX + eval.Evaluated.Title, eval, true, true);
        }

        workbook.SaveAs(fileName);
        return true;
    }
    public static bool TryExportTeamLeadCombinedAllReports(string folderPath)
    {
        return TryExportCombinedFullSurveyInternal(folderPath, includeAssistantReadinessSheet: true, includeNotesSheet: true);
    }
    private static bool TryExportCombinedFullSurveyInternal(string folderPath, bool includeAssistantReadinessSheet, bool includeNotesSheet)
    {
        string fileName = BuildDesktopExportPath(SPRINT_FULL_SURVEY_FILE_NAME);
       
        if (!Directory.Exists(folderPath))
            return false;

        var files = Directory.GetFiles(folderPath, "*.xlsx")
            .Where(path => !Path.GetFileName(path).StartsWith("~$"))
            .OrderBy(path => path)
            .ToList();

        if (files.Count == 0)
            return false;

        var employees = EmployeeService.AllEmployees;


        var reports = new List<SourceSurveyFile>();
        foreach (var file in files)
        {
            if (!file.Contains(FULL_SURVEY_FILE_NAME))
                return false;

            if (!TryGetEvalutorFromTitle(file, out Employee evaluator))
                return false;

            var systemEval = new SystemEvaluation(evaluator,
                ConfigLoader.SystemEvaluationConfig.FilterSectionsForEmployee(evaluator));
            if (!TryLoadSystemEvaluationFromExcel(file, systemEval))
                return false;

            var section = ConfigLoader.EmployeeEvaluationConfig.FilterSectionsForEmployee(evaluator);
            var empEvals = new List<EmployeeEvaluation>();
            foreach (var evaluated in employees)
            {
                if (evaluated.Code.Equals(evaluator.Code))
                    continue;

                var eval = new EmployeeEvaluation(evaluator, section, evaluated);
                if (!TryLoadEmployeeEvaluationFromExcel(file, eval))
                    continue;

                empEvals.Add(eval);
            }

            reports.Add(new SourceSurveyFile
            {
                SystemEvaluation = systemEval,
                EmployeeEvaluations = empEvals
            });
        }

        if (reports.Count < 2)
            return false;


        using var resultWorkbook = new XLWorkbook();
        var defaultSystemEval = new SystemEvaluation(null, ConfigLoader.SystemEvaluationConfig.Sections);
        var systemWS = WriteEvaluationSheet(resultWorkbook, SYSTEM_EVALUATION_CODE, WORKSHEET_SYSTEM_TITLE,
            defaultSystemEval, false, false);

        var defaultEmpEval = new EmployeeEvaluation(null, ConfigLoader.EmployeeEvaluationConfig.Sections, null);
        var empWSs = new Dictionary<Employee, IXLWorksheet>();
        foreach (var report in reports)
        {
            foreach (var empEval in report.EmployeeEvaluations)
            {
                if (empWSs.ContainsKey(empEval.Evaluated))
                    continue;

                empWSs[empEval.Evaluated] = 
                    WriteEvaluationSheet(resultWorkbook, empEval.Evaluated.Title, 
                    WORKSHEET_EMPLOYEE_TITLE_PREFIX + empEval.Evaluated.Title, defaultEmpEval, true, false);
            }
        }


        List<AssistantRecord> assistants = new List<AssistantRecord>();
        List<NoteRecord> notes = new List<NoteRecord>();


        int columnIndex = 3;
        foreach (var report in reports)
        {
            FillEvaluationSheet(systemWS, columnIndex, 
                report.SystemEvaluation.Evaluator.Title, report.SystemEvaluation, false, false);

            notes.Add(new()
            {
                Employee = report.SystemEvaluation.Evaluator,
                Note = report.SystemEvaluation.FinalNote,
            });

            foreach (var empEval in report.EmployeeEvaluations)
            {
                if (!empWSs.TryGetValue(empEval.Evaluated, out IXLWorksheet empWS))
                    continue;

                FillEvaluationSheet(empWS, columnIndex, empEval.Evaluator.Title, empEval, true, false);
            }

            columnIndex++;
        }


        AppendSummaryColumn(systemWS, reports.Select(r => (EvaluationBase)r.SystemEvaluation).ToList(), 
            defaultSystemEval.Sections);
        foreach (var empWS in empWSs)
            AppendSummaryColumn(empWS.Value, reports
                .SelectMany(r => r.EmployeeEvaluations.Where(e => e.Evaluated.Code.Equals(empWS.Key.Code))
                .Cast<EvaluationBase>()).ToList(), defaultEmpEval.Sections);


        if (includeAssistantReadinessSheet)
            AppendAssistantReadinessSheet(resultWorkbook, assistants);
        if (includeNotesSheet)
            AppendNotesAndSuggestionsSheet(resultWorkbook, notes);


        resultWorkbook.SaveAs(fileName);
        return true;
    }


    private static void WriteAndFillEvaluationSheet(XLWorkbook workbook, string workSheetTitle,
        string firstColumnTitle, EvaluationBase eval, bool writeNotes, bool writeTeamLeadAssist)
    {
        var ws = WriteEvaluationSheet(workbook, workSheetTitle, firstColumnTitle, 
            eval, writeNotes, writeTeamLeadAssist);
        FillEvaluationSheet(ws, 2, "", eval, writeNotes, writeTeamLeadAssist);
    }
    private static IXLWorksheet WriteEvaluationSheet(XLWorkbook workbook, string workSheetTitle,
        string firstColumnTitle, EvaluationBase eval, bool writeNotes, bool writeTeamLeadAssist)
    {
        var ws = workbook.Worksheets.Add(CampWorksheetName(workSheetTitle));

        int row = 1;
        ws.Cell(row, COLUMN_LABEL).Value = firstColumnTitle;
        ws.Row(row).Style.Font.Bold = true;
        row++;

        foreach (var section in eval.Sections)
        {
            ws.Cell(row, COLUMN_LABEL).Value = section.Name;
            ws.Cell(row, COLUMN_VALUE).Value = section.ScoreMeaning;
            ws.Row(row).Style.Font.Bold = true;
            row++;

            foreach (var question in section.Questions.Where(q => q.Include))
            {
                ws.Cell(row, COLUMN_LABEL).Value = question.Text;
                row++;
            }

            ws.Cell(row, COLUMN_LABEL).Value = LABEL_SECTION_TOTAL;
            ws.Row(row).Style.Font.Bold = true;
            row += 2;
        }

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_TOTAL;
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        if (writeNotes)
        {
            ws.Cell(row, COLUMN_LABEL).Value = LABEL_NOTES;
            ws.Row(row).Style.Font.Bold = true;
        }

        if (writeTeamLeadAssist && ShouldExportAssistantField())
        {
            if (eval is SystemEvaluation syseval)
            {
                row++;
                ws.Cell(row, COLUMN_LABEL).Value = LABEL_TEAM_LEADER_ASSISTANT_READY;
                ws.Row(row).Style.Font.Bold = true;
            }
            else if (eval is EmployeeEvaluation empEval)
            {
                row++;
                ws.Cell(row, COLUMN_LABEL).Value = LABEL_TEAM_LEADER_ASSISTANT_RECOMMENDATION;
                ws.Row(row).Style.Font.Bold = true;
            }
        }

        ws.Columns().AdjustToContents();

        return ws;
    }
    private static void FillEvaluationSheet(IXLWorksheet ws, int columnIndex, string columnTitle,
        EvaluationBase eval, bool writeNotes, bool writeTeamLeadAssist)
    {
        int row = 1;
        ws.Cell(row, columnIndex).Value = columnTitle;
        ws.Row(row).Style.Font.Bold = true;

        foreach (var section in eval.Sections)
        {
            foreach (var question in section.Questions.Where(q => q.Include))
            {
                row = TryGetRowIndexOfData(ws, question.Text);
                if (row == -1)
                    continue;

                ws.Cell(row, columnIndex).Value = question.Value;
            }

            if (row == -1)
                continue;

            row++;
            ws.Cell(row, columnIndex).Value = section.Score;
            ws.Row(row).Style.Font.Bold = true;
        }

        row = TryGetRowIndexOfData(ws, LABEL_TOTAL);
        ws.Cell(row, columnIndex).Value = eval.Score;
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        if (eval is SystemEvaluation syseval)
        {
            if (writeNotes)
            {
                ws.Cell(row, columnIndex).Value = syseval.FinalNote;
                ws.Row(row).Style.Font.Bold = true;
            }

            if (writeTeamLeadAssist && ShouldExportAssistantField())
            {
                row++;
                ws.Cell(row, columnIndex).Value = syseval.ReadyToBeAssistantTeamLeader ? 
                    LABEL_TEAM_LEADER_ASSISTANT_YES : LABEL_TEAM_LEADER_ASSISTANT_NO;
                ws.Row(row).Style.Font.Bold = true;
            }
        }
        else if (eval is EmployeeEvaluation empEval)
        {
            if (writeNotes)
            {
                ws.Cell(row, columnIndex).Value = empEval.FinalNote;
                ws.Row(row).Style.Font.Bold = true;
            }

            if (writeTeamLeadAssist && ShouldExportAssistantField())
            {
                row++;
                ws.Cell(row, columnIndex).Value = empEval.RecommendAsTeamLead ?
                    LABEL_TEAM_LEADER_ASSISTANT_YES : LABEL_TEAM_LEADER_ASSISTANT_NO;
                ws.Row(row).Style.Font.Bold = true;
            }
        }

        ws.Columns().AdjustToContents();
    }


    private static string BuildTeamMembersEvaluationFilename()
    {
        return $"{AuthService.CurrentUser.Title} {EvaluationBase.EvaluationSperator} {TEAM_MEMBERS_REPORT_FILE_NAME}";
    }
    private static string BuildFullReportFilename()
    {
        return $"{AuthService.CurrentUser.Title} {FULL_SURVEY_FILE_NAME}";
    }
    private static string BuildFullReportFilename(Employee employee)
    {
        return $"{employee.Title} {FULL_SURVEY_FILE_NAME}";
    }
    private static string BuildDesktopExportPath(string fileName)
    {
        return Path.Combine(DesktopPath, fileName);
    }
    private static int TryGetRowIndexOfData(IXLWorksheet ws, string data)
    {
        var used = ws.RowsUsed(o =>
        {
            var exist = o.Cell(1).Value.ToString();
            return exist == data;
        });

        if (used == null || used.Count() == 0) 
            return -1;

        return used.First().RowNumber();
    }
    private static bool ShouldAggregateInNotesSheet(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return false;

        return label.Equals(LABEL_NOTES, StringComparison.OrdinalIgnoreCase);
    }
    private static bool ShouldAggregateInAssistant(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return false;

        return label.Equals(LABEL_TEAM_LEADER_ASSISTANT_READY, StringComparison.OrdinalIgnoreCase) ||
                label.Equals(LABEL_TEAM_LEADER_ASSISTANT_RECOMMENDATION, StringComparison.OrdinalIgnoreCase);
    }
    private static void LoadRows(IXLWorksheet sheet, Dictionary<string, Question> textToQuestion,
        Action<string> setNote, Action<bool> setAssistant)
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
                    question.Value = Math.Clamp(value, question.MinValue, question.MaxValue);
            }
            else if (ShouldAggregateInNotesSheet(label))
            {
                setNote(sheet.Cell(row, COLUMN_VALUE).GetString());
            }
            else if (ShouldAggregateInAssistant(label))
            {
                string value = sheet.Cell(row, COLUMN_VALUE).GetString();
                setAssistant(value.Contains("yes", StringComparison.OrdinalIgnoreCase));
            }
        }
    }
    private static string CampWorksheetName(string title)
    {
        return title.Length <= 31 ? title : title[..31];
    }

    private static void AppendSummaryColumn(IXLWorksheet ws, List<EvaluationBase> evaluations, List<Section> 
        commonSections)
    {
        int lastColumn = ws.LastColumnUsed()?.ColumnNumber() ?? COLUMN_VALUE;
        if (lastColumn < COLUMN_VALUE)
            return;

        foreach (var eval in evaluations)
            eval.CalculateScore();


        EvaluationBase combinedEval = new SystemEvaluation(null, commonSections);
        foreach(var section in combinedEval.Sections)
        {
            foreach(var question in section.Questions)
            {
                List<double> scores = evaluations
                    .SelectMany(e => e.Sections.Where(s => s.Name.Equals(section.Name))
                    .SelectMany(s => s.Questions.Where(q => q.Id.Equals(question.Id)))
                    .Select(q => q.Score))
                    .ToList();

                question.Score = Question.CalculateCombinedScore(question, scores);
            }

            section.Score = section.Questions.Select(q => q.Score).Average();
        }

        combinedEval.Score = combinedEval.Sections.Select(s => s.Score).Average();


        int summaryColumn = lastColumn + 1;
        FillEvaluationSheet(ws, summaryColumn, "Summary", combinedEval, false, false);


        ws.Columns().AdjustToContents();
    }
    private static void AppendAssistantReadinessSheet(XLWorkbook workbook, IReadOnlyList<AssistantRecord> assistants)
    {
        var sheet = workbook.Worksheets.Add("Assistant Readiness");
        sheet.Cell(1, 1).Value = "Employee";
        sheet.Cell(1, 2).Value = "Is Ready";
        sheet.Cell(1, 3).Value = "Who Recommended";
        sheet.Cell(1, 4).Value = "Recommendations Count";
        sheet.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var assistant in assistants)
        {
            sheet.Cell(row, 1).Value = assistant.Target.Title;
            sheet.Cell(row, 2).Value = assistant.Ready;

            string text = "";
            foreach (var str in assistant.WhoRecommended.Select(o => o.Name))
            {
                if (!string.IsNullOrEmpty(text))
                    text += "\n";

                text += str;
            }
            sheet.Cell(row, 3).Value = text;
            sheet.Cell(row, 4).Value = assistant.WhoRecommended.Count;
            row++;
        }

        sheet.Columns().AdjustToContents();
        sheet.Rows().AdjustToContents();
    }
    private static void AppendNotesAndSuggestionsSheet(XLWorkbook workbook, IReadOnlyList<NoteRecord> notesRecords)
    {
        var sheet = workbook.Worksheets.Add("Suggestions & Notes");
        sheet.Cell(1, 1).Value = "Employee";
        sheet.Cell(1, 2).Value = "Notes";
        sheet.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var record in notesRecords)
        {
            sheet.Cell(row, 1).Value = record.Employee.Title;
            sheet.Cell(row, 2).Value = record.Note;
            row++;
        }

        sheet.Columns().AdjustToContents();
    }


    private static bool TryGetEvalutorFromTitle(string title, out Employee evaluator)
    {
        var employees = EmployeeService.AllEmployees;

        string empTile = title;
        if (title.Contains(EvaluationBase.EvaluationSperator))
        {
            empTile = title.Split(EvaluationBase.EvaluationSperator)[0];
        }

        evaluator = employees.FirstOrDefault(e => empTile.Contains(e.Code));
        return evaluator != null;
    }
    private static bool TryGetEvalutedFromTitle(string title, out Employee evaluated)
    {
        string empTile = title;
        if (title.Contains(EvaluationBase.EvaluationSperator))
        {
            empTile = title.Split(EvaluationBase.EvaluationSperator)[1];
            return TryGetEvalutorFromTitle(empTile, out evaluated);
        }

        evaluated = null;
        return false;
    }


    private static bool ShouldExportAssistantField()
    {
        if (AuthService.CurrentUser.IsTeamLead)
            return false;

        return ConfigLoader.EmployeeEvaluationConfig.Options.AskPreferTeamLeaderAssistant;
    }

    private sealed class SourceSurveyFile
    {
        public required SystemEvaluation SystemEvaluation { get; init; }
        public required List<EmployeeEvaluation> EmployeeEvaluations { get; init; }
    }
    private sealed class AssistantRecord
    {
        public required Employee Target { get; init; }
        public required bool Ready { get; init; }
        public List<Employee> WhoRecommended { get; init; } = new List<Employee>();
    }
    private sealed class NoteRecord
    {
        public required Employee Employee { get; init; }
        public required string Note { get; init; }
    }
}
