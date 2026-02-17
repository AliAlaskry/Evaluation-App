using ClosedXML.Excel;
using Evaluation_App.Services;
using System.Net.Quic;
using System.Text;
using static Constants;

public static class ExcelExportService
{
    public static bool ExportTeamMembers()
    {
        var employees = EmployeeService.LoadEmployees();

        string fileName = BuildDesktopExportPath(TEAM_MEMBERS_REPORT_FILE_NAME);
        using var workbook = new XLWorkbook();

        foreach (var emp in employees)
        {
            var eval = EvaluationService.LoadEvaluation(emp.Code);
            var title = BuildEmployeeSheetTitle(emp);
            WriteAndFillEmployeeEvaluationSheet(workbook, title, WORKSHEET_EMPLOYEE_TITLE_PREFIX + title, eval, true);
        }

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
        string fileName = BuildDesktopExportPath($"{BuildEmployeeSheetTitle(emp)} Report.xlsx");
        using var workbook = new XLWorkbook();

        var eval = EvaluationService.LoadEvaluation(emp.Code);
        var title = BuildEmployeeSheetTitle(emp);
        WriteAndFillEmployeeEvaluationSheet(workbook, title, WORKSHEET_EMPLOYEE_TITLE_PREFIX + title, eval, true);

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

        WriteAndFillSystemEvaluationSheet(workbook, SYSTEM_EVALUATION_CODE, WORKSHEET_SYSTEM_TITLE, EvaluationService.LoadSystemEvaluation(), true, true);

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
        string fileName = BuildDesktopExportPath($"{BuildEmployeeSheetTitle(AuthService.CurrentUser)} - {FULL_SURVEY_FILE_NAME}");
        using var workbook = new XLWorkbook();

        WriteAndFillSystemEvaluationSheet(workbook, SYSTEM_EVALUATION_CODE, WORKSHEET_SYSTEM_TITLE, EvaluationService.LoadSystemEvaluation(), true, true);

        var employees = EmployeeService.LoadEmployees();

        foreach (var emp in employees)
        {
            var eval = EvaluationService.LoadEvaluation(emp.Code);
            var title = BuildEmployeeSheetTitle(emp);
            WriteAndFillEmployeeEvaluationSheet(workbook, title, WORKSHEET_EMPLOYEE_TITLE_PREFIX + title, eval, true);
        }

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
        return TryExportCombinedFullSurveyInternal(folderPath, includeAssistantReadinessSheet: true, includeNotesSheet: true);
    }

    public static bool TryExportCombinedMemberFullSurvey(string systemEvaluationExcelPath, IReadOnlyList<string> employeeEvaluationExcelPaths)
    {
        if (!File.Exists(systemEvaluationExcelPath) || employeeEvaluationExcelPaths.Count == 0)
            return false;

        var systemModel = new SystemEvaluationrResult(ConfigLoader.LoadSystemSections());
        if (!TryLoadSystemEvaluationFromExcel(systemEvaluationExcelPath, systemModel))
            return false;

        var employeeSheets = new List<(Employee Employee, EmployeeEvaluationResult Eval)>();
        foreach (var employeeFile in employeeEvaluationExcelPaths.Where(File.Exists))
        {
            if (!TryResolveEmployeeFromTitle(employeeFile, out var employee))
                return false;

            var eval = new EmployeeEvaluationResult(employee, ConfigLoader.LoadEmployeeSections(employee));
            if (!TryLoadEmployeeEvaluationFromExcel(employeeFile, eval))
                return false;

            employeeSheets.Add((employee, eval));
        }

        if (employeeSheets.Count == 0)
            return false;

        using var workbook = new XLWorkbook();
        WriteAndFillSystemEvaluationSheet(workbook, SYSTEM_EVALUATION_CODE, WORKSHEET_SYSTEM_TITLE, systemModel, true, true);
        foreach (var emp in employeeSheets)
        {
            var title = BuildEmployeeSheetTitle(emp.Employee);
            WriteAndFillEmployeeEvaluationSheet(workbook, title, WORKSHEET_EMPLOYEE_TITLE_PREFIX + title, emp.Eval, true);
        }

        string fileName = BuildDesktopExportPath($"{BuildEmployeeSheetTitle(AuthService.CurrentUser)} - {FULL_SURVEY_FILE_NAME}");
        workbook.SaveAs(BuildDesktopExportPath(fileName));
        return true;
    }

    private static bool TryExportCombinedFullSurveyInternal(string folderPath, bool includeAssistantReadinessSheet, bool includeNotesSheet)
    {
        if (!Directory.Exists(folderPath))
            return false;

        var files = Directory.GetFiles(folderPath, "*.xlsx")
            .Where(path => !Path.GetFileName(path).StartsWith("~$"))
            .OrderBy(path => path)
            .ToList();

        if (files.Count == 0)
            return false;

        var employees = EmployeeService.LoadEmployees();
       
        var employeeEvaluations = new List<(Employee Employee, SystemEvaluationrResult SysEval, List<EmployeeEvaluationResult> EmpEvals)>();

        foreach (var file in files)
        {
            if(!TryResolveEmployeeFromTitle(file, out Employee currentEmp))
                return false;
            
            var systemEval = new SystemEvaluationrResult(ConfigLoader.LoadSystemSections());
            if (!TryLoadSystemEvaluationFromExcel(file, systemEval))
                return false;
           
            var employeeEvals = new List<EmployeeEvaluationResult>();
            foreach (var emp in employees)
            {
                var eval = new EmployeeEvaluationResult(emp, ConfigLoader.LoadEmployeeSections(emp));
                if (!TryLoadEmployeeEvaluationFromExcel(file, eval))
                    continue;

                employeeEvals.Add(eval);
            }
            employeeEvaluations.Add((currentEmp, systemEval, employeeEvals));
        }

        if (employeeEvaluations.Count < 2)
            return false;

        var notesRecords = AggregateNotes(employeeEvaluations);
        var assistants = AggregateAssistantTeamLeader(employeeEvaluations);

        using var resultWorkbook = new XLWorkbook();
        var defaultSysEval = new SystemEvaluationrResult(ConfigLoader.LoadSystemSections());
        var systemWS = WriteSystemEvaluationSheet(resultWorkbook, SYSTEM_EVALUATION_CODE, WORKSHEET_SYSTEM_TITLE, defaultSysEval, false, false);

        var empWSs = new Dictionary<string, IXLWorksheet>();
        string title;
        foreach (var eval in employeeEvaluations)
        {
            foreach (var empEval in eval.EmpEvals)
            {
                title = BuildEmployeeSheetTitle(empEval.Employee);

                if (empWSs.ContainsKey(title))
                    continue;

                empWSs[title] = WriteEmployeeEvaluationSheet(resultWorkbook, title, WORKSHEET_EMPLOYEE_TITLE_PREFIX + title, new(eval.Employee, ConfigLoader.LoadEmployeeSections(eval.Employee)), false);
            }
        }

        int columnIndex = 2;
        foreach(var eval in employeeEvaluations)
        {
            title = BuildEmployeeSheetTitle(eval.Employee);
            FillSystemEvaluationSheet(systemWS, columnIndex, title, eval.SysEval, false, false);

            foreach (var empEval in eval.EmpEvals)
            {
                var empSheeTitle = BuildEmployeeSheetTitle(empEval.Employee);
                if (empWSs.TryGetValue(empSheeTitle, out IXLWorksheet empWS))
                    FillEmployeeEvaluationSheet(empWS, columnIndex, title, empEval, false);
            }
            
            columnIndex++;
        }

        List<double> scores = new List<double>();
        int index = 2;
        var scoring = ConfigLoader.LoadEmployeeOptions().Scoring;
        foreach (var eval in employeeEvaluations)
        {
            int row = 1;
            systemWS.Cell(row, columnIndex).Value = "Summary";

            foreach(var section in eval.SysEval.Sections)
            {
                foreach(var question in section.Questions)
                {
                    row = GetRowIndexOfData(systemWS, question.Text);

                    scores.Clear();
                    index = 2;
                    while (index < columnIndex)
                    {
                        if (systemWS.Cell(row, index).TryGetValue(out double tempScore))
                            scores.Add(tempScore);
                        else
                            Console.WriteLine(systemWS.Cell(row, index).Value);
                        index++;
                    }

                    double score = Question.ScoreQuestion(question, scores);

                    systemWS.Cell(row, columnIndex).Value = score;
                }

                row++;
                section.SetTotalScore(scoring);
                systemWS.Cell(row, columnIndex).Value = section.TotalScore;
            }

            row = GetRowIndexOfData(systemWS, LABEL_TOTAL);
            eval.SysEval.SetTotalScore();
            systemWS.Cell(row, columnIndex).Value = eval.SysEval.TotalScore;
            

            foreach (var empEval in eval.EmpEvals)
            {
                var empSheeTitle = BuildEmployeeSheetTitle(empEval.Employee);

                if (empWSs.TryGetValue(empSheeTitle, out IXLWorksheet empWS))
                {
                    row = 1;
                    empWS.Cell(row, columnIndex).Value = "Summary";
                    row++;

                    foreach (var section in empEval.Sections)
                    {
                        foreach (var question in section.Questions)
                        {
                            row = GetRowIndexOfData(empWS, question.Text);

                            scores.Clear();
                            index = 2;
                            while (index < columnIndex)
                            {
                                if (empWS.Cell(row, index).TryGetValue(out double tempScore))
                                    scores.Add(tempScore);
                                else
                                    Console.WriteLine(empWS.Cell(row, index).Value);
                                index++;
                            }

                            double score = Question.ScoreQuestion(question, scores);

                            empWS.Cell(row, columnIndex).Value = score;
                        }

                        row++;
                        section.SetTotalScore(scoring);
                        empWS.Cell(row, columnIndex).Value = section.TotalScore;
                    }

                    row = GetRowIndexOfData(empWS, LABEL_TOTAL);
                    empEval.SetTotalScore();
                    systemWS.Cell(row, columnIndex).Value = empEval.TotalScore;
                }
            }
        }

        if (includeAssistantReadinessSheet)
            AppendAssistantReadinessSheet(resultWorkbook, assistants);

        if (includeNotesSheet)
            AppendNotesAndSuggestionsSheet(resultWorkbook, notesRecords);

        string fileName = BuildDesktopExportPath(SPRINT_FULL_SURVEY_FILE_NAME);
        resultWorkbook.SaveAs(fileName);
        return true;
    }

    private static List<(Employee employee, bool ready, List<Employee> recommendation)> AggregateAssistantTeamLeader(List<(Employee Employee, SystemEvaluationrResult SysEval, List<EmployeeEvaluationResult> EmpEvals)> evals)
    {
        List<(Employee employee, bool ready, List<Employee> recommendation)> data = new();
        foreach (var emp in EmployeeService.LoadEmployees())
        {
            var whoRecommended = evals.Where(o =>
            {
                var empEval = o.EmpEvals.FirstOrDefault(o => o.Employee.Code == emp.Code);
                return empEval != null && empEval.RecommendAsTeamLead;
            }).Select(o => o.Employee).ToList();

            bool isReady = false;
            var eval = evals.FirstOrDefault(o => o.Employee.Code.Equals(emp.Code));
            if (eval != default)
                isReady = eval.SysEval.ReadyToBeAssistantTeamLeader;

            data.Add(new(emp, isReady, whoRecommended));
        }
        return data.OrderByDescending(o => o.recommendation.Count).ThenByDescending(o => o.ready).ToList();
    }

    private static List<(Employee employee, string notes)> AggregateNotes(List<(Employee Employee, SystemEvaluationrResult SysEval, List<EmployeeEvaluationResult> EmpEvals)> evals)
    {
        List<(Employee employee, string notes)> data = new();
        foreach (var eval in evals)
        {
            data.Add(new(eval.Employee, eval.SysEval.FinalNote));
        }
        return data;
    }

    public static bool TryLoadSystemEvaluationFromExcel(string excelPath, SystemEvaluationrResult destination)
    {
        if (!File.Exists(excelPath))
            return false;

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(SYSTEM_EVALUATION_CODE));

        if (sheet == null)
            return false;

        var textToQuestion = destination.Sections
            .SelectMany(s => s.Questions)
            .Where(q => q.Include)
            .ToDictionary(q => q.Text, q => q, StringComparer.OrdinalIgnoreCase);

        destination.ReadyToBeAssistantTeamLeader = false;
        destination.FinalNote = string.Empty;

        LoadRows(sheet, textToQuestion, note => destination.FinalNote = note, assistant => destination.ReadyToBeAssistantTeamLeader = assistant);

        destination.SetTotalScore();
        return true;
    }
     
    public static bool TryLoadEmployeeEvaluationFromExcel(string excelPath, EmployeeEvaluationResult destination)
    {
        if (!File.Exists(excelPath))
            return false;

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(BuildEmployeeSheetTitle(destination.Employee)));

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
        return Path.Combine(DesktopPath, reportFileName);
    }

    private static void WriteAndFillSystemEvaluationSheet(XLWorkbook workbook, string workSheetTitle, string firstColumnTitle, SystemEvaluationrResult eval, bool writeNotes, bool writeTeamLeadAssist)
    {
        var ws = WriteSystemEvaluationSheet(workbook, workSheetTitle, firstColumnTitle, eval, writeNotes, writeTeamLeadAssist);
        FillSystemEvaluationSheet(ws, 2, "", eval, writeNotes, writeTeamLeadAssist);
    }

    private static IXLWorksheet WriteSystemEvaluationSheet(XLWorkbook workbook, string workSheetTitle, string firstColumnTitle, SystemEvaluationrResult eval, bool writeNotes, bool writeTeamLeadAssist)
    {
        var ws = workbook.Worksheets.Add(CampWorksheetName(workSheetTitle));

        int row = 1;
        ws.Cell(row, COLUMN_LABEL).Value = firstColumnTitle;
        ws.Row(row).Style.Font.Bold = true;
        row++;

        foreach (var section in eval.Sections)
        {
            ws.Cell(row, COLUMN_LABEL).Value = section.Name;
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
            row++;
            ws.Cell(row, COLUMN_LABEL).Value = LABEL_TEAM_LEADER_ASSISTANT_READY;
            ws.Row(row).Style.Font.Bold = true;
        }

        ws.Columns().AdjustToContents();

        return ws;
    }

    private static void FillSystemEvaluationSheet(IXLWorksheet ws, int columnIndex, string columnTitle, SystemEvaluationrResult eval, bool writeNotes, bool writeTeamLeadAssist)
    {
        int row = 1;
        ws.Cell(row, columnIndex).Value = columnTitle;
        ws.Row(row).Style.Font.Bold = true;

        foreach (var section in eval.Sections)
        {
            row = GetRowIndexOfData(ws, section.Name);
            ws.Cell(row, columnIndex).Value = section.NumberMeaning;
            ws.Row(row).Style.Font.Bold = true;

            foreach (var question in section.Questions.Where(q => q.Include))
            {
                row = GetRowIndexOfData(ws, question.Text);
                ws.Cell(row, columnIndex).Value = question.Score;
            }

            row++;
            ws.Cell(row, columnIndex).Value = section.TotalScore;
            ws.Row(row).Style.Font.Bold = true;
        }

        row = GetRowIndexOfData(ws, LABEL_TOTAL);
        ws.Cell(row, columnIndex).Value = eval.TotalScore;
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        if (writeNotes)
        {
            ws.Cell(row, columnIndex).Value = eval.FinalNote;
            ws.Row(row).Style.Font.Bold = true;
        }

        if (writeTeamLeadAssist && ShouldExportAssistantField())
        {
            row++;
            ws.Cell(row, columnIndex).Value = eval.ReadyToBeAssistantTeamLeader ? LABEL_TEAM_LEADER_ASSISTANT_YES : LABEL_TEAM_LEADER_ASSISTANT_NO;
            ws.Row(row).Style.Font.Bold = true;
        }

        ws.Columns().AdjustToContents();
    }

    private static void WriteAndFillEmployeeEvaluationSheet(XLWorkbook workbook, string workSheetTitle, string firstColumnTitle, EmployeeEvaluationResult eval, bool writeTeamLeadAssist)
    {
        var ws = WriteEmployeeEvaluationSheet(workbook, workSheetTitle, firstColumnTitle, eval, writeTeamLeadAssist);
        FillEmployeeEvaluationSheet(ws, 2, "", eval, writeTeamLeadAssist);
    }

    private static IXLWorksheet WriteEmployeeEvaluationSheet(XLWorkbook workbook, string workSheetTitle, string firstColumnTitle, EmployeeEvaluationResult eval, bool writeTeamLeadAssist)
    {
        var ws = workbook.Worksheets.Add(CampWorksheetName(workSheetTitle));

        int row = 1;
        ws.Cell(row, COLUMN_LABEL).Value = firstColumnTitle;
        ws.Row(row).Style.Font.Bold = true;
        row++;

        foreach (var section in eval.Sections)
        {
            ws.Cell(row, COLUMN_LABEL).Value = section.Name;
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

        ws.Cell(row, COLUMN_LABEL).Value = LABEL_NOTES;
        ws.Row(row).Style.Font.Bold = true;

        if (writeTeamLeadAssist && ShouldExportAssistantField())
        {
            row++;
            ws.Cell(row, COLUMN_LABEL).Value = LABEL_TEAM_LEADER_ASSISTANT_RECOMMENDATION;
            ws.Row(row).Style.Font.Bold = true;
        }

        ws.Columns().AdjustToContents();

        return ws;
    }

    private static void FillEmployeeEvaluationSheet(IXLWorksheet ws, int columnIndex, string columnTitle, EmployeeEvaluationResult eval, bool writeTeamLeadAssist)
    {
        int row = 1;
        ws.Cell(row, columnIndex).Value = columnTitle;
        ws.Row(row).Style.Font.Bold = true;
        row++;

        foreach (var section in eval.Sections)
        {
            row = GetRowIndexOfData(ws, section.Name);
            ws.Cell(row, columnIndex).Value = section.NumberMeaning;
            ws.Row(row).Style.Font.Bold = true;

            foreach (var question in section.Questions.Where(q => q.Include))
            {
                row = GetRowIndexOfData(ws, question.Text);
                ws.Cell(row, columnIndex).Value = question.Score;
            }

            row++;
            ws.Cell(row, columnIndex).Value = section.TotalScore;
            ws.Row(row).Style.Font.Bold = true;
        }

        row = GetRowIndexOfData(ws, LABEL_TOTAL);
        ws.Cell(row, columnIndex).Value = eval.TotalScore;
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        ws.Cell(row, columnIndex).Value = eval.FinalNote;
        ws.Row(row).Style.Font.Bold = true;

        if (writeTeamLeadAssist && ShouldExportAssistantField())
        {
            row++;
            ws.Cell(row, columnIndex).Value = eval.RecommendAsTeamLead ? LABEL_TEAM_LEADER_ASSISTANT_YES : LABEL_TEAM_LEADER_ASSISTANT_NO;
            ws.Row(row).Style.Font.Bold = true;
        }

        ws.Columns().AdjustToContents();
    }

    private static int GetRowIndexOfData(IXLWorksheet ws, string data)
    {
        var used = ws.RowsUsed(o =>
        {
            var exist = o.Cell(1).Value.ToString();
            return exist == data;
        });
        return used.First().RowNumber();
    }

    private static void AppendAssistantReadinessSheet(XLWorkbook workbook, List<(Employee employee, bool ready, List<Employee> recommendation)> assistants)
    {
        var sheet = workbook.Worksheets.Add("Assistant Readiness");
        sheet.Cell(1, 1).Value = "Employee";
        sheet.Cell(1, 2).Value = "Is Ready";
        sheet.Cell(1, 3).Value = "Who Recommended";
        sheet.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var assistant in assistants)
        {
            sheet.Cell(row, 1).Value = BuildEmployeeSheetTitle(assistant.employee);
            sheet.Cell(row, 2).Value = assistant.ready;

            string text = "";
            foreach(var str in assistant.recommendation.Select(o => o.Name))
            {
                if(!string.IsNullOrEmpty(text))
                    text += "\n";
                
                text += str;
            }
            sheet.Cell(row, 3).Value = text;
            row++;
        }

        sheet.Columns().AdjustToContents();
        sheet.Rows().AdjustToContents();
    }

    private static bool ShouldAggregateInNotesSheet(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return false;

        return label.Contains(LABEL_NOTES, StringComparison.OrdinalIgnoreCase)
            || label.Contains("مقترحات")
            || label.Contains("كلمة");
    }

    private static void AppendNotesAndSuggestionsSheet(XLWorkbook workbook, IReadOnlyList<(Employee Employee, string Note)> notesRecords)
    {
        var sheet = workbook.Worksheets.Add("Notes & Suggestions");
        sheet.Cell(1, 1).Value = "Employee";
        sheet.Cell(1, 2).Value = "Notes";
        sheet.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var record in notesRecords)
        {
            sheet.Cell(row, 1).Value = BuildEmployeeSheetTitle(record.Employee);
            sheet.Cell(row, 2).Value = record.Note;
            row++;
        }

        sheet.Columns().AdjustToContents();
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
            else if (ShouldAggregateInNotesSheet(label))
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

    private static string BuildEmployeeSheetTitle(Employee employee)
    {
        return $"{employee.Name} - {employee.Code}";
    }

    private static string CampWorksheetName(string title)
    {
        return title.Length <= 31 ? title : title[..31];
    }

    private static bool TryResolveEmployeeFromTitle(string title, out Employee employee)
    {
        employee = null;
        var employees = EmployeeService.LoadEmployees();

        employee = employees.FirstOrDefault(e => title.Contains(e.Code));
        if (employee != null)
            return true;

        return false;
    }

    private static bool ShouldExportAssistantField()
    {
        if (AuthService.CurrentUser.IsTeamLead)
            return false;

        var options = ConfigLoader.LoadEmployeeOptions();
        return options.AskPreferTeamLeaderAssistant;
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
