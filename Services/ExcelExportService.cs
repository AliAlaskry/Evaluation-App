using ClosedXML.Excel;
using static Constants;

internal static class ExcelExportService
{
    public static bool TryExportSingleEvaluation(EvaluationInstance eval)
    {
        if (eval == null)
            return false;

        using var workbook = new XLWorkbook();
        WriteAndFillEvaluationSheet(workbook, eval, WriteConfig.FullWrite());

        workbook.SaveExcelFile(eval.FileName, DesktopPath);
        return true;
    }
    public static bool TryExportMultiEvaluations(string filename,
        List<EvaluationInstance> evals)
    {
        if (evals == null || evals.Count == 0)
            return false;

        Dictionary<Employee, List<EvaluationInstance>> perEvaluator = evals
            .GroupBy(o => o.Evaluator).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var key in perEvaluator.Keys)
            ExportMultiEvaluations(filename, perEvaluator[key]);

        return true;
    }
    private static void ExportMultiEvaluations(string filename,
        List<EvaluationInstance> evals)
    {
        using var workbook = new XLWorkbook();

        foreach (var eval in evals)
            WriteAndFillEvaluationSheet(workbook, eval, WriteConfig.FullWrite());

        workbook.SaveExcelFile(filename, DesktopPath);
    }


    public static bool TryLoadEvaluationInstanceFromExcel(string excelPath,
        EvaluationInstance eval)
    {
        if (!File.Exists(excelPath))
            return false;

        using var workbook = new XLWorkbook(excelPath);

        var code = eval.GetEvaluationSheetName();
        var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(code));
        if (sheet == null)
            return false;

        LoadRows(sheet, eval);
        eval.CalculateScore();

        return true;
    }
    // used to load from one evaluation instance file only on first valid sheet.
    public static bool TryLoadEvaluationInstanceFromExcel(string excelPath, out EvaluationInstance eval)
    {
        eval = null;
        if (!File.Exists(excelPath))
            return false;

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets
            .FirstOrDefault(ws =>
            {
                if (ws.Name.Equals(SYSTEM_EVALUATION_CODE))
                    return true;

                foreach (var employee in EmployeeService.AllEmployees)
                    if (ws.Name.Contains(employee.Code))
                        return true;

                return false;
            });
        if (sheet == null)
            return false;

        if (!sheet.TryGetEvaluator(out var evaluator))
            return false;

        if (!sheet.TryGetBeingEvaluated(out var beingEvaluated))
            return false;

        eval = new(evaluator, beingEvaluated);
        LoadRows(sheet, eval);
        eval.CalculateScore();

        return true;
    }
    // used to load evaluation from combined or full report for single memeber.
    public static bool TryLoadEvaluationInstanceFromExcel(string excelPath, Employee? beingEvaluated,
        out EvaluationInstance eval)
    {
        eval = null;
        if (!File.Exists(excelPath))
            return false;

        string sheetName = beingEvaluated == null ?
            SYSTEM_EVALUATION_CODE : beingEvaluated.Title.CampWorksheetName();

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(sheetName));
        if (sheet == null)
            return false;

        if (!sheet.TryGetEvaluator(out var evaluator))
            return false;

        eval = new(evaluator, beingEvaluated);
        LoadRows(sheet, eval);
        eval.CalculateScore();

        return true;
    }
    // used to load evaluation from full report for all memebers.
    public static bool TryLoadEvaluationInstanceFromExcel(string excelPath, Employee evaluator,
        Employee? beingEvaluated, out EvaluationInstance eval)
    {
        eval = null;
        if (!File.Exists(excelPath))
            return false;

        string sheetName = beingEvaluated == null ?
            SYSTEM_EVALUATION_CODE : beingEvaluated.Title.CampWorksheetName();

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(sheetName));
        if (sheet == null)
            return false;

        eval = new(evaluator, beingEvaluated);
        LoadRows(sheet, eval);
        eval.CalculateScore();

        return true;
    }


    public static bool TryExportCombinedMemberFullSurvey(IReadOnlyList<string> evaluationExcelPaths)
    {
        if (evaluationExcelPaths.Count == 0)
            return false;

        EvaluationInstance systemEval = null;
        foreach (var path in evaluationExcelPaths)
            if (TryLoadEvaluationInstanceFromExcel(path, null, out systemEval))
                break;

        if (systemEval == null)
            return false;

        Employee evaluator = systemEval.Evaluator;

        var employees = EmployeeService.AllEmployees;
        var employeeEvals = new Dictionary<Employee, EvaluationInstance>();
        foreach (var employeeFile in evaluationExcelPaths.Where(File.Exists))
        {
            if (!TryLoadEvaluationInstanceFromExcel(employeeFile, out var eval))
                foreach (var employee in employees)
                    TryLoadEvaluationInstanceFromExcel(employeeFile, employee, out eval);

            if (eval == null
                || !eval.Evaluator.Code.Equals(evaluator.Code) || eval.BeingEvaluated == null)
                continue;

            employeeEvals.TryAdd(eval.BeingEvaluated, eval);
        }

        if (employeeEvals.Count == 0)
            return false;

        using var workbook = new XLWorkbook();
        WriteAndFillEvaluationSheet(workbook, systemEval, WriteConfig.FullWrite());
        foreach (var eval in employeeEvals.Values)
            WriteAndFillEvaluationSheet(workbook, eval, WriteConfig.FullWrite());

        workbook.SaveExcelFile(evaluator.BuildFullReportFilename(), DesktopPath);
        return true;
    }
    public static bool TryExportTeamLeadCombinedAllReports(IReadOnlyList<string> reportFiles)
    {
        if (reportFiles.Count == 0)
            return false;

        var employees = EmployeeService.AllEmployees;
        var sysEvals = new List<EvaluationInstance>();
        var beingEvaluedEvals = new Dictionary<Employee, List<EvaluationInstance>>();
        foreach (var file in reportFiles)
        {
            if (!TryLoadEvaluationInstanceFromExcel(file, null, out var systemEval))
                return false;

            foreach (var employee in employees)
            {
                if (!TryLoadEvaluationInstanceFromExcel(file, employee, out var eval))
                    continue;

                if (!beingEvaluedEvals.ContainsKey(employee))
                    beingEvaluedEvals.Add(employee, []);
                beingEvaluedEvals[employee].Add(eval);
            }

            sysEvals.Add(systemEval);
        }
        sysEvals = sysEvals.OrderBy(o => o.Evaluator.Code).ToList();
        beingEvaluedEvals = beingEvaluedEvals.OrderBy(o => o.Key.Code).ToDictionary();

        if (sysEvals.Count < 2 || beingEvaluedEvals.Count < 2)
            return false;

        var fullSysEval = CombinedEvaluationInstance.GenerateCombinedEntity(sysEvals);
        var fullBeingEvaluatedEval = beingEvaluedEvals
            .ToDictionary(o => o.Key, o => CombinedEvaluationInstance.GenerateCombinedEntity(o.Value));

        using var resultWorkbook = new XLWorkbook();
        var systemWS = WriteEvaluationSheet(resultWorkbook, fullSysEval, WriteConfig.AggregateWrite());

        var empWSs = new Dictionary<Employee, IXLWorksheet>();
        foreach (var beingEvaluatedEval in fullBeingEvaluatedEval.Values)
        {
            var ws = WriteEvaluationSheet(resultWorkbook, beingEvaluatedEval, WriteConfig.AssessmentOnlyWrite());
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
                    Employee = sysEval.Evaluator,
                    Note = sysEval.FinalNote
                });

            if (sysEval.ReadyToBeAssistantTeamLeader)
                assistants.Add(sysEval.Evaluator, new AssistantRecord
                {
                    Ready = true,
                    Target = sysEval.Evaluator,
                    WhoRecommended = new()
                });

            FillEvaluationSheet(systemWS, columnIndex, sysEval, WriteConfig.AggregateWrite());
            columnIndex++;
        }
        FillEvaluationSheet(systemWS, columnIndex, fullSysEval, WriteConfig.AggregateWrite(true));

        foreach (var empEval in beingEvaluedEvals)
        {
            columnIndex = COLUMN_START_VALUE;
            var evals = empEval.Value.OrderBy(o => o.Evaluator.Code);
            foreach (var eval in evals)
            {
                if (eval.RecommendAsTeamLead)
                {
                    if (assistants.TryGetValue(eval.BeingEvaluated, out var assistant))
                        assistant.WhoRecommended.Add(eval.Evaluator);
                    else
                        assistants.Add(eval.BeingEvaluated, new AssistantRecord
                        {
                            Ready = false,
                            Target = eval.BeingEvaluated,
                            WhoRecommended = [eval.Evaluator]
                        });
                }

                FillEvaluationSheet(empWSs[empEval.Key], columnIndex, eval,
                    WriteConfig.AssessmentOnlyWrite());
                columnIndex++;
            }

            FillEvaluationSheet(empWSs[empEval.Key], columnIndex, fullBeingEvaluatedEval[empEval.Key],
                WriteConfig.AggregateWrite(true));
        }

        var goat = fullBeingEvaluatedEval.MaxBy(e => e.Value.TotalScore);
        GoatSheetStyling.CreateGoatSheetWithSummary(resultWorkbook, goat.Key, goat.Value);

        AppendAssistantReadinessSheet(resultWorkbook, assistants.Values.ToList());
        AppendNotesAndSuggestionsSheet(resultWorkbook, notes);

        resultWorkbook.SaveExcelFile(SPRINT_FULL_SURVEY_FILE_NAME, DesktopPath);
        return true;
    }


    private static void WriteAndFillEvaluationSheet(XLWorkbook workbook, IEvaluation eval,
        WriteConfig writeConfig)
    {
        var ws = WriteEvaluationSheet(workbook, eval, writeConfig);
        FillEvaluationSheet(ws, COLUMN_START_VALUE, eval, writeConfig);
    }
    private static IXLWorksheet WriteEvaluationSheet(XLWorkbook workbook, IEvaluation eval,
        WriteConfig writeConfig)
    {
        var ws = workbook.Worksheets.Add(eval.GetEvaluationSheetName());

        ws.Column(COLUMN_ID).Hide();
        ws.Row(ROW_EVALUATOR_META).Hide();

        int row = 1;
        ws.Row(row).Style.Font.Bold = true;
        if (writeConfig.WriteTitleInFirstColumn)
            ws.Cell(row, COLUMN_LABEL).Value = eval.GetEvaluationColumnTitle();

        row = 3;
        WriteEntities(ws, ref row, eval.ReadonlyEntities);

        if (!eval.IsSystemEvaluation)
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
    private static void WriteEntities(IXLWorksheet ws, ref int row,
        IReadOnlyList<IEntityNode> entities)
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

                WriteEntities(ws, ref row, entity.ReadonlyChilds ?? []);

                ws.Cell(row, COLUMN_LABEL).Value = entity.BaseConfig.ID + POSTFIX_ENTITY_TOTAL;
                ws.Row(row).Style.Font.Bold = true;
                row += 2;
            }
        }
    }


    private static void FillEvaluationSheet(IXLWorksheet ws, int columnIndex, IEvaluation eval,
        WriteConfig writeConfig)
    {
        ws.Cell(ROW_EVALUATOR_META, columnIndex).Value =
            EvalCodeMarker(eval.IsCombined ? "AGG" : eval.Evaluator.Code);

        if (writeConfig.WriteEvaluatorTitle)
            ws.Cell(ROW_HEADER_VISIBLE, columnIndex).Value =
                eval.IsCombined ? "Summary" : eval.Evaluator.Title;
        ws.Row(ROW_HEADER_VISIBLE).Style.Font.Bold = true;

        var index = SheetIndexBuilder.Build(ws);
        FillEntities(ws, index, columnIndex, eval.ReadonlyEntities, writeConfig);

        if (!eval.IsSystemEvaluation)
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
            if (eval.IsSystemEvaluation)
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
        IReadOnlyList<IEntityNode> entities, WriteConfig writeConfig)
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
                ws.Cell(row, columnIndex).Value = writeConfig.WriteScore ? entity.Score.RoundDouble(2) : entity.Value;

            if (entity.RootConfig.HasValue)
            {
                FillEntities(ws, index, columnIndex, entity.ReadonlyChilds ?? [], writeConfig);

                row = index.Columns[COLUMN_LABEL][entity.BaseConfig.ID + POSTFIX_ENTITY_TOTAL];
                ws.Cell(row, columnIndex).Value = entity.Score.RoundDouble(2);
                ws.Row(row).Style.Font.Bold = true;
            }
        }
    }

    private static bool TryGetStartColumnIndexOfEvaluation(IXLWorksheet ws, IEvaluation eval,
        out int column)
    {
        column = -1;

        int lastColumn = ws.LastColumnUsed()?.ColumnNumber() ?? 0;
        if (lastColumn <= 0)
            return false;

        string targetMarker = EvalCodeMarker(eval.Evaluator.Code);

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
            if (!string.IsNullOrEmpty(label) && label.Equals(eval.Evaluator.Title, StringComparison.OrdinalIgnoreCase))
            {
                column = c;
                return true;
            }
        }

        var entity = eval.SearchFor(e => e.ValueConfig.HasValue);
        if (entity == null)
            return false;

        return false;
    }

    private static void LoadRows(IXLWorksheet ws, EvaluationInstance eval)
    {
        if (!TryGetStartColumnIndexOfEvaluation(ws, eval, out int column))
            return;

        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

        var index = SheetIndexBuilder.Build(ws);
        LoadRows(ws, index, column, eval.Entities);

        if (index.Columns[COLUMN_ID].TryGetValue(LABEL_NOTES, out int row) ||
            index.Columns[COLUMN_LABEL].TryGetValue(LABEL_NOTES, out row))
            eval.FinalNote = ws.Cell(row, column).GetString();

        if (eval.IsSystemEvaluation)
        {
            if (index.Columns[COLUMN_ID].TryGetValue(LABEL_TEAM_LEADER_ASSISTANT_READY, out row) ||
                index.Columns[COLUMN_LABEL].TryGetValue(LABEL_TEAM_LEADER_ASSISTANT_READY, out row))
                eval.ReadyToBeAssistantTeamLeader = eval.RecommendAsTeamLead =
                    ws.Cell(row, column).GetString().Contains("yes", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            if (index.Columns[COLUMN_ID].TryGetValue(LABEL_TEAM_LEADER_ASSISTANT_RECOMMENDATION, out row) ||
                 index.Columns[COLUMN_LABEL].TryGetValue(LABEL_TEAM_LEADER_ASSISTANT_RECOMMENDATION, out row))
                eval.ReadyToBeAssistantTeamLeader = eval.RecommendAsTeamLead =
                    ws.Cell(row, column).GetString().Contains("yes", StringComparison.OrdinalIgnoreCase);
        }
    }
    private static void LoadRows(IXLWorksheet ws, SheetIndex index, int column,
        IReadOnlyList<IEntityNode> entities)
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
            {
                var cell = ws.Cell(row, column);
                if (cell.TryGetValue<double>(out var value))
                    entity.Value = (int)Math.Clamp(value,
                        entity.ValueConfig.Value.MinValue, entity.ValueConfig.Value.MaxValue);
            }

            if (entity.RootConfig.HasValue)
                LoadRows(ws, index, column, entity.ReadonlyChilds ?? []);
        }
    }


    private static void AppendAssistantReadinessSheet(XLWorkbook workbook, IReadOnlyList<AssistantRecord> assistants)
    {
        assistants = assistants.OrderBy(o => o.WhoRecommended.Count).ThenBy(o => !o.Ready).ToList();

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

        int row = 1;
        string text = "";
        foreach (var str in ConfigLoader.SystemEvaluationOptions.IssuesToResolve)
        {
            if (!string.IsNullOrEmpty(text))
                text += ", ";

            text += str;
        }
        sheet.Cell(row, 2).Value = text;

        row++;
        sheet.Cell(row, 1).Value = "Employee";
        sheet.Cell(row, 2).Value = "Notes";
        sheet.Row(row).Style.Font.Bold = true;

        row++;
        foreach (var record in notesRecords)
        {
            sheet.Cell(row, 1).Value = record.Employee.Title;
            sheet.Cell(row, 2).Value = record.Note;
            row++;
        }

        sheet.Columns().AdjustToContents();
        sheet.Rows().AdjustToContents();
    }


    #region Extensions
    private static string EvalCodeMarker(string code) => $"__EvalCode:{code}";
    private static bool IsEvalCodeMarker(string s) => !string.IsNullOrWhiteSpace(s) && s.StartsWith("__EvalCode:", StringComparison.OrdinalIgnoreCase);
    private static string ExtractEvalCode(string marker) => marker.Substring("__EvalCode:".Length).Trim();

    private static bool TryGetEvaluator(this IXLWorksheet ws, out Employee evaluator)
    {
        evaluator = null;
        foreach (var column in ws.ColumnsUsed())
        {
            var marker = column.Cell(ROW_EVALUATOR_META).GetString()?.Trim();
            if (!string.IsNullOrWhiteSpace(marker) && IsEvalCodeMarker(marker))
                evaluator = EmployeeService.GetEmployeeByCode(ExtractEvalCode(marker));
        }

        return evaluator != null;
    }
    private static bool TryGetBeingEvaluated(this IXLWorksheet ws, out Employee? beingEvaluated)
    {
        beingEvaluated = null;
        if (IsSystemSheet(ws))
            return true;

        beingEvaluated = EmployeeService.AllEmployees.FirstOrDefault(e => ws.Name.Contains(e.Code));
        return beingEvaluated != null;
    }

    private static string CampWorksheetName(this string title)
    {
        return title.Length <= 31 ? title : title[..31];
    }
    private static string GetEvaluationSheetName(this IEvaluation eval)
        => (eval.BeingEvaluated == null ? SYSTEM_EVALUATION_CODE : eval.BeingEvaluated.Title).CampWorksheetName();
    private static string GetEvaluationColumnTitle(this IEvaluation eval)
        => eval.BeingEvaluated == null ? WORKSHEET_SYSTEM_TITLE : WORKSHEET_EMPLOYEE_TITLE_PREFIX + eval.BeingEvaluated.Title;

    private static bool IsSystemSheet(this IXLWorksheet sheet)
        => sheet.Name.Equals(SYSTEM_EVALUATION_CODE);

    private static float RoundDouble(this double value, int rounds)
        => MathF.Round((float)value, rounds);

    public static string GetFileName(this IEvaluation instance)
    {
        return GetFileName(instance.Evaluator, instance.BeingEvaluated);
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
    public static string BuildFullReportFilename(this Employee evaluator)
    {
        return $"{evaluator.Title} {FULL_SURVEY_FILE_NAME}";
    }


    private static void SaveExcelFile(this XLWorkbook workbook, string filename, string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var fullPath = Path.Combine(path, Path.ChangeExtension(filename, ".xlsx"));
        workbook.SaveAs(fullPath);
    }
    #endregion

    private struct WriteConfig
    {
        public required bool WriteTitleInFirstColumn;
        public required bool WriteEvaluatorTitle;
        public required bool WriteAssistant;
        public required bool WriteNotes;
        public required bool WriteScore;

        public static WriteConfig FullWrite(bool writeScore = false)
        {
            return new WriteConfig
            {
                WriteTitleInFirstColumn = true,
                WriteEvaluatorTitle = true,
                WriteAssistant = true,
                WriteNotes = true,
                WriteScore = writeScore
            };
        }
        public static WriteConfig AssessmentOnlyWrite(bool writeScore = false)
        {
            return new WriteConfig
            {
                WriteTitleInFirstColumn = true,
                WriteEvaluatorTitle = true,
                WriteAssistant = false,
                WriteNotes = true,
                WriteScore = writeScore
            };
        }
        public static WriteConfig AggregateWrite(bool writeScore = false)
        {
            return new WriteConfig
            {
                WriteTitleInFirstColumn = true,
                WriteEvaluatorTitle = true,
                WriteAssistant = false,
                WriteNotes = false,
                WriteScore = writeScore
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
