using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using static Constants;

internal static class ExcelExportService
{
    public static bool TryExportEmployeeEvaluation(EvaluationInstance eval)
    {
        if (eval == null)
            return false;

        string fileName = eval.FileNameWithExension.BuildDesktopExportPath();

        eval.CalculateScore();

        using var workbook = new XLWorkbook();
        WriteAndFillEvaluationSheet(workbook, eval, WriteConfig.FullWrite());

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
        string fileName = AuthService.CurrentUser
            .BuildTeamMembersEvaluationFilename().AppendExcelExtension().BuildDesktopExportPath();

        var evulateds = EmployeeService.OtherEmployees;

        using var workbook = new XLWorkbook();
        foreach (var evaluated in evulateds)
        {
            var eval = EvaluationService.LoadEvaluation(GetFileName(AuthService.CurrentUser, evaluated));

            if (eval == null)
                continue;

            eval.CalculateScore();
            WriteAndFillEvaluationSheet(workbook, eval, WriteConfig.FullWrite());
        }

        if (workbook.Worksheets.Count != evulateds.Count)
        {
            MessageBox.Show("لم يتم تقييم جميع الموظفين .", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        workbook.SaveAs(fileName);
        return true;
    }
    public static bool TryExportSystemEvaluation(EvaluationInstance eval)
    {
        string fileName = eval.FileNameWithExension.BuildDesktopExportPath();

        if (eval == null)
            return false;

        eval.CalculateScore();

        using var workbook = new XLWorkbook();
        WriteAndFillEvaluationSheet(workbook, eval, WriteConfig.FullWrite());

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
        string fileName = AuthService.CurrentUser.BuildFullReportFilename().AppendExcelExtension()
            .BuildDesktopExportPath();

        var systemEval = EvaluationService.LoadEvaluation(GetFileName(AuthService.CurrentUser));

        if (systemEval == null)
            return false;

        systemEval.CalculateScore();

        using var workbook = new XLWorkbook();
        WriteAndFillEvaluationSheet(workbook, systemEval, WriteConfig.FullWrite());

        var evaluteds = EmployeeService.OtherEmployees;

        foreach (var evaluated in evaluteds)
        {
            var eval = EvaluationService.LoadEvaluation(GetFileName(AuthService.CurrentUser, evaluated));

            if (eval == null)
                return false;

            eval.CalculateScore();

            WriteAndFillEvaluationSheet(workbook, eval, WriteConfig.FullWrite());
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
        string fileName = AuthService.CurrentUser.BuildFullReportFilename().AppendExcelExtension()
            .BuildDesktopExportPath();

        var systemEval = EvaluationService.LoadEvaluation(GetFileName(AuthService.CurrentUser));

        using var workbook = new XLWorkbook();

        if (systemEval != null)
        {
            systemEval.CalculateScore();
            WriteAndFillEvaluationSheet(workbook, systemEval, WriteConfig.FullWrite());
        }

        var evaluteds = EmployeeService.OtherEmployees;

        foreach (var evaluated in evaluteds)
        {
            var eval = EvaluationService.LoadEvaluation(GetFileName(AuthService.CurrentUser, evaluated));

            if (eval != null)
            {
                eval.CalculateScore();
                WriteAndFillEvaluationSheet(workbook, eval, WriteConfig.FullWrite());
            }
        }

        if (workbook.Worksheets.Count == 0)
        {
            MessageBox.Show("ليست هناك تقييمات على الإطلاق.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        workbook.SaveAs(fileName);
        return true;
    }


    public static bool TryLoadEvaluationInstanceFromExcel(string excelPath, EvaluationInstance eval)
    {
        if (!File.Exists(excelPath))
            return false;

        var code = eval.BeingEvaluated?.Title.CampWorksheetName() ?? SYSTEM_EVALUATION_CODE;
        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(code));

        if (sheet == null)
            return false;

        LoadRows(sheet, eval);
        eval.CalculateScore();

        return true;
    }
    // used to load from one evaluation instance file only. like system or single employee evaluation file.
    public static bool TryLoadEvaluationInstanceFromExcel(string excelPath, out EvaluationInstance eval)
    {
        eval = null;
        if (!File.Exists(excelPath))
            return false;

        if (!TryGetEvalutorFromTitle(excelPath, out Employee evaluator))
            return false;

        TryGetEvalutedFromTitle(excelPath, out Employee? beingEvaluated);

        string code;
        if (beingEvaluated == null)
        {
            eval = new(evaluator);
            if (!excelPath.Contains(SYSTEM_EVALUATION_CODE))
                return false;

            code = SYSTEM_EVALUATION_CODE;
        }
        else
        {
            eval = new(evaluator, beingEvaluated);
            if (!excelPath.Contains(beingEvaluated.Title))
                return false;

            code = beingEvaluated.Title.CampWorksheetName();
        }

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(code));

        if (sheet == null)
            return false;

        LoadRows(sheet, eval);
        eval.CalculateScore();

        return true;
    }
    // used to load employee evaluation from combined or full report.
    public static bool TryLoadEvaluationInstanceFromExcel(string excelPath, Employee? beingEvaluated,
        out EvaluationInstance eval)
    {
        eval = null;
        if (!File.Exists(excelPath))
            return false;

        if (!TryGetEvalutorFromTitle(excelPath, out Employee evaluator))
            return false;

        string code;
        if (beingEvaluated == null)
        {
            eval = new(evaluator);
            code = SYSTEM_EVALUATION_CODE;
        }
        else
        {
            eval = new(evaluator, beingEvaluated);
            code = beingEvaluated.Title.CampWorksheetName();
        }

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(code));

        if (sheet == null)
            return false;

        LoadRows(sheet, eval);
        eval.CalculateScore();

        return true;
    }


    public static bool TryExportCombinedMemberFullSurvey(string systemEvaluationExcelPath, IReadOnlyList<string> employeeEvaluationExcelPaths)
    {
        if (!File.Exists(systemEvaluationExcelPath) || employeeEvaluationExcelPaths.Count == 0)
            return false;

        if (!TryLoadEvaluationInstanceFromExcel(systemEvaluationExcelPath, out var systemEval))
            return false;
        Employee evaluator = systemEval.FirstEvaluator;

        systemEval.CalculateScore();

        var employees = EmployeeService.AllEmployees;
        var employeeEvals = new Dictionary<Employee, EvaluationInstance>();
        foreach (var employeeFile in employeeEvaluationExcelPaths.Where(File.Exists))
        {
            if (!TryLoadEvaluationInstanceFromExcel(employeeFile, out var eval))
            {
                foreach (var employee in employees)
                    TryLoadEvaluationInstanceFromExcel(employeeFile, employee, out eval);

                if (eval == null)
                    return false;
            }

            if (!eval.FirstEvaluator.Code.Equals(evaluator.Code) || eval.BeingEvaluated == null)
                return false;

            if (employeeEvals.TryAdd(eval.BeingEvaluated, eval))
            {
                eval.CalculateScore();
            }
        }

        if (employeeEvals.Count == 0)
            return false;

        using var workbook = new XLWorkbook();
        WriteAndFillEvaluationSheet(workbook, systemEval, WriteConfig.FullWrite());
        foreach (var eval in employeeEvals.Values)
            WriteAndFillEvaluationSheet(workbook, eval, WriteConfig.FullWrite());

        string fileName = evaluator.BuildFullReportFilename().AppendExcelExtension()
            .BuildDesktopExportPath();
        workbook.SaveAs(fileName);
        return true;
    }
    public static bool TryExportTeamLeadCombinedAllReports(string folderPath)
    {
        string fileName = SPRINT_FULL_SURVEY_FILE_NAME.AppendExcelExtension().BuildDesktopExportPath();

        if (!Directory.Exists(folderPath))
            return false;

        var files = Directory.GetFiles(folderPath, "*.xlsx")
            .Where(path => !Path.GetFileName(path).StartsWith("~$"))
            .OrderBy(path => path)
            .ToList();

        if (files.Count == 0)
            return false;

        var employees = EmployeeService.AllEmployees;
        var sysEvals = new List<EvaluationInstance>();
        var beingEvaluedEvals = new Dictionary<Employee, List<EvaluationInstance>>();
        foreach (var file in files)
        {
            if (!file.Contains(FULL_SURVEY_FILE_NAME))
                return false;

            if (!TryLoadEvaluationInstanceFromExcel(file, null, out var systemEval))
                return false;

            foreach (var employee in employees)
            {
                if (!TryLoadEvaluationInstanceFromExcel(file, employee, out var eval))
                    continue;

                eval.CalculateScore();

                if (!beingEvaluedEvals.ContainsKey(employee))
                    beingEvaluedEvals.Add(employee, []);
                beingEvaluedEvals[employee].Add(eval);
            }

            sysEvals.Add(systemEval);
        }
        sysEvals = sysEvals.OrderBy(o => o.FirstEvaluator.Code).ToList();
        beingEvaluedEvals = beingEvaluedEvals.OrderBy(o => o.Key.Code).ToDictionary();

        if (sysEvals.Count < 2 || beingEvaluedEvals.Count < 2)
            return false;

        var fullSysEval = new EvaluationInstance(sysEvals, "");
        var fullBeingEvaluatedEval = beingEvaluedEvals
            .ToDictionary(o => o.Key, o => new EvaluationInstance(o.Value, ""));

        using var resultWorkbook = new XLWorkbook();
        var systemWS = WriteEvaluationSheet(resultWorkbook, fullSysEval, WriteConfig.AggregateWrite());

        var empWSs = new Dictionary<Employee, IXLWorksheet>();
        foreach (var beingEvaluatedEval in fullBeingEvaluatedEval.Values)
        {
            var ws = WriteEvaluationSheet(resultWorkbook, beingEvaluatedEval, WriteConfig.ParialWrite());
            empWSs.Add(beingEvaluatedEval.BeingEvaluated, ws);
        }

        Dictionary<Employee, AssistantRecord> assistants = new();
        List<NoteRecord> notes = new List<NoteRecord>();

        int columnIndex = COLUMN_START_VALUE;
        foreach (var sysEval in sysEvals)
        {
            if (!string.IsNullOrEmpty(sysEval.FinalNote))
                notes.Add(new NoteRecord
                {
                    Employee = sysEval.FirstEvaluator,
                    Note = sysEval.FinalNote
                });

            if (sysEval.ReadyToBeAssistantTeamLeader)
                assistants.Add(sysEval.FirstEvaluator, new AssistantRecord
                {
                    Ready = true,
                    Target = sysEval.FirstEvaluator,
                    WhoRecommended = new()
                });

            FillEvaluationSheet(systemWS, columnIndex, sysEval, WriteConfig.AggregateWrite());
            columnIndex++;
        }
        FillEvaluationSheet(systemWS, columnIndex, fullSysEval, WriteConfig.AggregateWrite());

        foreach (var empEval in beingEvaluedEvals)
        {
            columnIndex = COLUMN_START_VALUE;
            var evals = empEval.Value.OrderBy(o => o.FirstEvaluator.Code);
            foreach (var eval in evals)
            {
                if (eval.RecommendAsTeamLead)
                {
                    if (assistants.TryGetValue(eval.BeingEvaluated, out var assistant))
                        assistant.WhoRecommended.Add(eval.FirstEvaluator);
                    else
                        assistants.Add(eval.BeingEvaluated, new AssistantRecord
                        {
                            Ready = false,
                            Target = eval.BeingEvaluated,
                            WhoRecommended = [eval.FirstEvaluator]
                        });
                }

                FillEvaluationSheet(empWSs[empEval.Key], columnIndex, eval,
                    WriteConfig.AggregateWrite());
                columnIndex++;
            }

            FillEvaluationSheet(empWSs[empEval.Key], columnIndex, fullBeingEvaluatedEval[empEval.Key],
                WriteConfig.AggregateWrite());
        }

        AppendAssistantReadinessSheet(resultWorkbook, assistants.Values.ToList());
        AppendNotesAndSuggestionsSheet(resultWorkbook, notes);

        resultWorkbook.SaveAs(fileName);
        return true;
    }


    private static void WriteAndFillEvaluationSheet(XLWorkbook workbook, EvaluationInstance eval,
        WriteConfig writeConfig)
    {
        var ws = WriteEvaluationSheet(workbook, eval, writeConfig);
        FillEvaluationSheet(ws, COLUMN_START_VALUE, eval, writeConfig);
    }
    private static IXLWorksheet WriteEvaluationSheet(XLWorkbook workbook, EvaluationInstance eval,
        WriteConfig writeConfig)
    {
        var wsTitle = eval.BeingEvaluated == null ? SYSTEM_EVALUATION_CODE : eval.BeingEvaluated.Title;
        var ws = workbook.Worksheets.Add(wsTitle.CampWorksheetName());

        ws.Column(COLUMN_ID).Hide();

        int row = 1;

        if (writeConfig.WriteTitleInFirstColumn)
        {
            var stColumnTitle = eval.BeingEvaluated == null
                ? WORKSHEET_SYSTEM_TITLE
                : WORKSHEET_EMPLOYEE_TITLE_PREFIX + eval.BeingEvaluated.Title;

            ws.Cell(row, COLUMN_LABEL).Value = stColumnTitle;
        }
        ws.Row(row).Style.Font.Bold = true;

        ws.Row(ROW_EVALUATOR_META).Hide();
        row = 3;

        WriteEntities(ws, ref row, eval.Entities);

        if (!eval.IsSystemEvaluationInstance)
        {
            ws.Cell(row, COLUMN_LABEL).Value = LABEL_TOTAL;
            ws.Row(row).Style.Font.Bold = true;
            row += 2;
        }

        if (writeConfig.WriteNotes)
        {
            ws.Cell(row, COLUMN_LABEL).Value = LABEL_NOTES;
            ws.Row(row).Style.Font.Bold = true;
        }

        if (writeConfig.WriteAssistant && eval.AssistantSectionEnabled())
        {
            row++;

            if (eval.BeingEvaluated == null)
                ws.Cell(row, COLUMN_LABEL).Value = LABEL_TEAM_LEADER_ASSISTANT_READY;
            else
                ws.Cell(row, COLUMN_LABEL).Value = LABEL_TEAM_LEADER_ASSISTANT_RECOMMENDATION;

            ws.Row(row).Style.Font.Bold = true;
        }

        ws.Columns().AdjustToContents();
        ws.Rows().AdjustToContents();

        return ws;
    }
    private static void WriteEntities(IXLWorksheet ws, ref int row, List<EntityBase> entities)
    {
        foreach (var entity in entities)
        {
            ws.Cell(row, COLUMN_ID).Value = entity.BaseConfig.ID;

            if (entity.ValueConfig.HasValue)
            {
                ws.Cell(row, COLUMN_LABEL).Value = entity.ValueConfig.Value.Body;
                row++;
            }

            if (entity.RootConfig.HasValue)
            {
                ws.Cell(row, COLUMN_LABEL).Value = entity.RootConfig.Value.Title;
                ws.Cell(row, COLUMN_MEANING).Value = entity.RootConfig.Value.ScoreMeaning;
                ws.Row(row).Style.Font.Bold = true;
                row++;

                WriteEntities(ws, ref row, entity.RootConfig.Value.Childs);

                ws.Cell(row, COLUMN_LABEL).Value = entity.BaseConfig.ID + POSTFIX_ENTITY_TOTAL;
                ws.Row(row).Style.Font.Bold = true;
                row += 2;
            }
        }
    }
    private static void FillEvaluationSheet(IXLWorksheet ws, int columnIndex, EvaluationInstance eval,
        WriteConfig writeConfig)
    {
        eval.CalculateScore();

        ws.Row(ROW_EVALUATOR_META).Hide();
        ws.Cell(ROW_EVALUATOR_META, columnIndex).Value =
            EvalCodeMarker(eval.Evaluators.Count > 1 ? "AGG" : eval.FirstEvaluator.Code);

        if (writeConfig.WriteEvaluatorTitle)
            ws.Cell(ROW_HEADER_VISIBLE, columnIndex).Value =
                eval.Evaluators.Count > 1 ? "Summary" : eval.FirstEvaluator.Title;
        ws.Row(ROW_HEADER_VISIBLE).Style.Font.Bold = true;

        var index = SheetIndexBuilder.Build(ws);
        FillEntities(ws, index, columnIndex, eval.Entities, writeConfig);

        if (!eval.IsSystemEvaluationInstance)
        {
            if (index.Columns[COLUMN_ID].TryGetValue(LABEL_TOTAL, out int row)
                || index.Columns[COLUMN_LABEL].TryGetValue(LABEL_TOTAL, out row))
            {
                ws.Cell(row, columnIndex).Value = eval.TotalScore;
                ws.Row(row).Style.Font.Bold = true;
            }
        }

        if (writeConfig.WriteNotes)
        {
            if (index.Columns[COLUMN_ID]
                .TryGetValue(LABEL_NOTES, out int row)
                || index.Columns[COLUMN_LABEL]
                .TryGetValue(LABEL_NOTES, out row))
            {
                ws.Cell(row, columnIndex).Value = eval.FinalNote;
                ws.Cell(row, columnIndex).Style.Alignment.SetWrapText();
                ws.Row(row).Style.Font.Bold = true;
            }
        }

        if (writeConfig.WriteAssistant && eval.AssistantSectionEnabled())
        {
            if (eval.BeingEvaluated == null)
            {
                if (index.Columns[COLUMN_ID]
                   .TryGetValue(LABEL_TEAM_LEADER_ASSISTANT_READY, out int row)
                   || index.Columns[COLUMN_LABEL]
                   .TryGetValue(LABEL_TEAM_LEADER_ASSISTANT_READY, out row))
                {
                    ws.Cell(row, columnIndex).Value = eval.ReadyToBeAssistantTeamLeader ?
                        LABEL_TEAM_LEADER_ASSISTANT_YES : LABEL_TEAM_LEADER_ASSISTANT_NO;
                }
            }
            else
            {
                if (index.Columns[COLUMN_ID]
                   .TryGetValue(LABEL_TEAM_LEADER_ASSISTANT_RECOMMENDATION, out int row)
                   || index.Columns[COLUMN_LABEL]
                   .TryGetValue(LABEL_TEAM_LEADER_ASSISTANT_RECOMMENDATION, out row))
                {
                    ws.Cell(row, columnIndex).Value = eval.RecommendAsTeamLead ?
                        LABEL_TEAM_LEADER_ASSISTANT_YES : LABEL_TEAM_LEADER_ASSISTANT_NO;
                }

                ws.Row(row).Style.Font.Bold = true;
            }
        }

        ws.Columns().AdjustToContents();
        ws.Rows().AdjustToContents();
    }
    private static void FillEntities(IXLWorksheet ws, SheetIndex index, int columnIndex, 
        List<EntityBase> entities, WriteConfig writeConfig)
    {
        foreach (var entity in entities)
        {
            if (!index.Columns[COLUMN_ID].TryGetValue(entity.BaseConfig.ID, out int row))
            {
                if (entity.RootConfig.HasValue)
                    if (!index.Columns[COLUMN_ID].TryGetValue(entity.RootConfig.Value.Title, out row))
                        if (!index.Columns[COLUMN_LABEL].TryGetValue(entity.RootConfig.Value.Title, out row))
                            continue;

                if (entity.ValueConfig.HasValue)
                    if (!index.Columns[COLUMN_ID].TryGetValue(entity.ValueConfig.Value.Body, out row))
                        if (!index.Columns[COLUMN_LABEL].TryGetValue(entity.ValueConfig.Value.Body, out row))
                            continue;
            }

            if (entity.ValueConfig.HasValue)
                ws.Cell(row, columnIndex).Value = entity.Value;

            if (entity.RootConfig.HasValue)
            {
                FillEntities(ws, index, columnIndex, entity.RootConfig.Value.Childs, writeConfig);

                row = index.Columns[COLUMN_LABEL][entity.BaseConfig.ID + POSTFIX_ENTITY_TOTAL];
                ws.Cell(row, columnIndex).Value = entity.Score;
                ws.Row(row).Style.Font.Bold = true;
            }
        }
    }
    private static bool TryGetStartRowIndexOfEntity(IXLWorksheet ws, EntityBase entity, out int row)
    {
        row = TryGetRowIndexOfData(ws, COLUMN_ID, entity.BaseConfig.ID);
        if (row != -1)
            return true;

        if (entity.RootConfig.HasValue)
        {
            row = TryGetRowIndexOfData(ws, COLUMN_LABEL, entity.RootConfig.Value.Title);
            if (row != -1)
                return true;
        }

        if (entity.ValueConfig.HasValue)
        {
            row = TryGetRowIndexOfData(ws, COLUMN_LABEL, entity.ValueConfig.Value.Body);

            if (row != -1)
                return true;

            row = TryGetRowIndexOfData(ws, COLUMN_ID, entity.ValueConfig.Value.Body);
        }

        return row != -1;
    }
    private static bool TryGetStartColumnIndexOfEvaluation(IXLWorksheet ws, EvaluationInstance eval,
        out int column)
    {
        column = -1;

        int lastColumn = ws.LastColumnUsed()?.ColumnNumber() ?? 0;
        if (lastColumn <= 0)
            return false;

        string targetMarker = EvalCodeMarker(eval.FirstEvaluator.Code);

        for (int c = 1; c <= lastColumn; c++)
        {
            string meta = ws.Cell(ROW_EVALUATOR_META, c).GetString();
            if (!string.IsNullOrWhiteSpace(meta) && meta.Equals(targetMarker, StringComparison.OrdinalIgnoreCase))
            {
                column = c;
                return true;
            }
        }

        for (int c = 1; c <= lastColumn; c++)
        {
            string label = ws.Cell(ROW_HEADER_VISIBLE, c).GetString();
            if (!string.IsNullOrEmpty(label) && label.Equals(eval.FirstEvaluator.Title, StringComparison.OrdinalIgnoreCase))
            {
                column = c;
                return true;
            }
        }

        var entity = eval.SearchFor(e => e.ValueConfig.HasValue);
        if (entity == null)
            return false;

        if (TryGetStartRowIndexOfEntity(ws, entity, out int row))
        {
            for (int c = 1; c <= lastColumn; c++)
            {
                var cell = ws.Cell(row, c);
                if (cell.TryGetValue<double>(out _))
                {
                    column = c;
                    return true;
                }
            }
        }

        return false;
    }

    private static int TryGetRowIndexOfData(IXLWorksheet ws, int columnIndex, string data)
    {
        var used = ws.RowsUsed(o =>
        {
            var exist = o.Cell(columnIndex).Value.ToString();
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


    private static void LoadRows(IXLWorksheet ws, EvaluationInstance eval)
    {
        if (!TryGetStartColumnIndexOfEvaluation(ws, eval, out int column))
            return;

        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

        var index = SheetIndexBuilder.Build(ws);
        LoadRows(ws, index, column, eval.Entities);

        index.Columns[COLUMN_ID].TryGetValue(LABEL_TOTAL, out int row);
        if (row == -1)
            index.Columns[COLUMN_LABEL].TryGetValue(LABEL_TOTAL, out row);

        for (; row <= lastRow; row++)
        {
            string value = ws.Cell(row, column).GetString();
            if (string.IsNullOrEmpty(value))
                continue;

            string id = ws.Cell(row, COLUMN_ID).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(id))
            {
                if (ShouldAggregateInNotesSheet(id))
                    eval.FinalNote = value;
                else if (ShouldAggregateInAssistant(id))
                    eval.ReadyToBeAssistantTeamLeader = eval.RecommendAsTeamLead =
                        value.Contains("yes", StringComparison.OrdinalIgnoreCase);
            }

            string label = ws.Cell(row, COLUMN_LABEL).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(label))
            {
                if (ShouldAggregateInNotesSheet(label))
                    eval.FinalNote = value;
                else if (ShouldAggregateInAssistant(label))
                    eval.ReadyToBeAssistantTeamLeader = eval.RecommendAsTeamLead =
                        value.Contains("yes", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
    private static void LoadRows(IXLWorksheet ws, SheetIndex index, int column, List<EntityBase> entities)
    {
        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

        foreach (var entity in entities)
        {
            if (!index.Columns[COLUMN_ID].TryGetValue(entity.BaseConfig.ID, out int row))
            {
                if (entity.RootConfig.HasValue)
                    if (!index.Columns[COLUMN_ID].TryGetValue(entity.RootConfig.Value.Title, out row))
                        if (!index.Columns[COLUMN_LABEL].TryGetValue(entity.RootConfig.Value.Title, out row))
                            continue;

                if (entity.ValueConfig.HasValue)
                    if (!index.Columns[COLUMN_ID].TryGetValue(entity.ValueConfig.Value.Body, out row))
                        if (!index.Columns[COLUMN_LABEL].TryGetValue(entity.ValueConfig.Value.Body, out row))
                            continue;
            }

            if (entity.ValueConfig.HasValue)
            {
                var cell = ws.Cell(row, column);
                if (cell.TryGetValue<double>(out var value))
                    entity.Value = (int)Math.Clamp(value,
                        entity.ValueConfig.Value.MinValue, entity.ValueConfig.Value.MaxValue);
            }

            if (entity.RootConfig.HasValue)
                LoadRows(ws, index, column, entity.RootConfig.Value.Childs);
        }
    }


    private static void AppendAssistantReadinessSheet(XLWorkbook workbook, IReadOnlyList<AssistantRecord> assistants)
    {
        var sheet = workbook.Worksheets.Add("Assistant Readiness");
        sheet.Cell(1, 1).Value = "Assistant";
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
            foreach (var str in assistant.WhoRecommended.Select(o => o.Title))
            {
                if (!string.IsNullOrEmpty(text))
                    text += ", ";

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
        sheet.Rows().AdjustToContents();
    }


    private static bool TryGetEvalutorFromTitle(string title, out Employee evaluator)
    {
        var employees = EmployeeService.AllEmployees;

        string empTile = title;
        if (title.Contains(EvaluationSperator))
        {
            empTile = title.Split(EvaluationSperator)[0];
        }

        evaluator = employees.FirstOrDefault(e => empTile.Contains(e.Code));
        return evaluator != null;
    }
    private static bool TryGetEvalutedFromTitle(string title, out Employee? evaluated)
    {
        string empTile = title;
        if (title.Contains(EvaluationSperator))
        {
            empTile = title.Split(EvaluationSperator)[1];
            return TryGetEvalutorFromTitle(empTile, out evaluated);
        }

        evaluated = null;
        return false;
    }

    #region Extensions
    private static string EvalCodeMarker(string code) => $"__EvalCode:{code}";
    private static bool IsEvalCodeMarker(string s) => !string.IsNullOrWhiteSpace(s) && s.StartsWith("__EvalCode:", StringComparison.OrdinalIgnoreCase);
    private static string ExtractEvalCode(string marker) => marker.Substring("__EvalCode:".Length).Trim();

    private static string CampWorksheetName(this string title)
    {
        return title.Length <= 31 ? title : title[..31];
    }

    public static string GetFileName(this EvaluationInstance instance)
    {
        return GetFileName(instance.FirstEvaluator, instance.BeingEvaluated);
    }
    public static string GetFileName(Employee evaluator, Employee? beingEvaluated = null)
    {
        return $"{evaluator.Title} {EvaluationSperator} " +
         (beingEvaluated == null ? SYSTEM_EVALUATION_CODE : beingEvaluated.Title);
    }
    public static string AppendExcelExtension(this string filename) =>
        Path.ChangeExtension(filename, ".xlsx");

    private static string BuildTeamMembersEvaluationFilename(this Employee evaluator)
    {
        return $"{evaluator.Title} {EvaluationSperator} {TEAM_MEMBERS_REPORT_FILE_NAME}";
    }
    private static string BuildFullReportFilename(this Employee evaluator)
    {
        return $"{evaluator.Title} {FULL_SURVEY_FILE_NAME}";
    }
    private static string BuildDesktopExportPath(this string fileName)
    {
        return Path.Combine(DesktopPath, fileName);
    }
    #endregion

    private struct WriteConfig
    {
        public required bool WriteTitleInFirstColumn;
        public required bool WriteEvaluatorTitle;
        public required bool WriteAssistant;
        public required bool WriteNotes;

        public static WriteConfig FullWrite()
        {
            return new WriteConfig
            {
                WriteTitleInFirstColumn = true,
                WriteEvaluatorTitle = true,
                WriteAssistant = true,
                WriteNotes = true
            };
        }
        public static WriteConfig ParialWrite()
        {
            return new WriteConfig
            {
                WriteTitleInFirstColumn = true,
                WriteEvaluatorTitle = true,
                WriteAssistant = false,
                WriteNotes = true
            };
        }
        public static WriteConfig AggregateWrite()
        {
            return new WriteConfig
            {
                WriteTitleInFirstColumn = true,
                WriteEvaluatorTitle = true,
                WriteAssistant = false,
                WriteNotes = false
            };
        }
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
