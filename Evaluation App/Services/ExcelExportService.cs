using ClosedXML.Excel;
using Evaluation_App.Services;
using static Constants;

public static class ExcelExportService
{
    public static void ExportTeamMembers()
    {
        var employees = EmployeeService.LoadEmployees()
            .Where(e => e.Code != AuthService.CurrentUser.Code && e.Include)
            .ToList();

        if (!employees.Any())
            return;

        string fileName = Path.Combine(DesktopPath, "Team Members Report.xlsx");
        using var workbook = new XLWorkbook();

        foreach (var emp in employees)
            ExportEvaluation(workbook, emp.Code, $"Employee Evaluation - {emp.Name}");

        workbook.SaveAs(fileName);
    }

    public static void ExportTeamMember(Employee emp)
    {
        string fileName = Path.Combine(DesktopPath, $"{emp.Name} Report.xlsx");
        using var workbook = new XLWorkbook();

        ExportEvaluation(workbook, emp.Code, $"Employee Evaluation - {emp.Name}");
        workbook.SaveAs(fileName);
    }

    public static void ExportSystemEvaluation()
    {
        string fileName = Path.Combine(DesktopPath, "System Evaluation.xlsx");
        using var workbook = new XLWorkbook();

        ExportEvaluation(workbook, SYSTEM_EVALUATION_CODE, "System Evaluation");
        workbook.SaveAs(fileName);
    }

    public static void ExportFullReport()
    {
        string fileName = Path.Combine(DesktopPath, "Full Report.xlsx");
        using var workbook = new XLWorkbook();

        ExportEvaluation(workbook, SYSTEM_EVALUATION_CODE, "System Evaluation");

        var employees = EmployeeService.LoadEmployees()
            .Where(e => e.Code != AuthService.CurrentUser.Code && e.Include)
            .ToList();

        foreach (var emp in employees)
            ExportEvaluation(workbook, emp.Code, $"Employee Evaluation - {emp.Name}");

        workbook.SaveAs(fileName);
    }

    public static bool TryLoadEvaluationFromExcel(string excelPath, string evalCode, EvaluationResult destination)
    {
        if (!File.Exists(excelPath))
            return false;

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets
            .FirstOrDefault(ws => ws.Name.Contains(evalCode, StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.FirstOrDefault(ws => ws.Name.Contains("System", StringComparison.OrdinalIgnoreCase) && evalCode == SYSTEM_EVALUATION_CODE)
            ?? workbook.Worksheets.FirstOrDefault(ws => ws.Name.Contains("Employee", StringComparison.OrdinalIgnoreCase) && evalCode != SYSTEM_EVALUATION_CODE)
            ?? workbook.Worksheets.FirstOrDefault();

        if (sheet == null)
            return false;

        var textToQuestion = destination.Sections
            .SelectMany(s => s.Questions)
            .ToDictionary(q => q.Text, q => q, StringComparer.OrdinalIgnoreCase);

        int row = 1;
        while (!sheet.Cell(row, 1).IsEmpty())
        {
            string label = sheet.Cell(row, 1).GetString().Trim();

            if (textToQuestion.TryGetValue(label, out var question))
            {
                var cell = sheet.Cell(row, 2);
                if (cell.TryGetValue<double>(out var value))
                    question.Score = Math.Clamp(value, question.Min, question.Max);
            }
            else if (label.Contains("Notes", StringComparison.OrdinalIgnoreCase) || label.Contains("مقترحات") || label.Contains("كلمة"))
            {
                destination.FinalNote = sheet.Cell(row, 2).GetString();
            }
            else if (label.Contains("assistant", StringComparison.OrdinalIgnoreCase) || label.Contains("مساعد"))
            {
                string value = sheet.Cell(row, 2).GetString();
                destination.RecommendAsTeamLead = value.Contains("yes", StringComparison.OrdinalIgnoreCase) || value.Contains("نعم");
            }

            row++;
        }

        destination.SetTotalScore();
        return true;
    }

    private static void ExportEvaluation(XLWorkbook workbook, string evalCode, string workSheetTitle)
    {
        var eval = EvaluationService.LoadEvaluation(evalCode);
        if (eval == null) return;

        var ws = workbook.Worksheets.Add($"{workSheetTitle} - {evalCode}");

        int row = 1;
        ws.Cell(row, 1).Value = workSheetTitle;
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        foreach (var section in eval.Sections)
        {
            ws.Cell(row, 1).Value = section.Name;
            ws.Cell(row, 2).Value = section.NumberMeaning;
            ws.Row(row).Style.Font.Bold = true;
            row++;

            foreach (var question in section.Questions)
            {
                ws.Cell(row, 1).Value = question.Text;
                ws.Cell(row, 2).Value = question.Score;
                row++;
            }

            ws.Cell(row, 1).Value = "Section Total";
            ws.Cell(row, 2).Value = section.TotalScore;
            ws.Row(row).Style.Font.Bold = true;
            row += 2;
        }

        ws.Cell(row, 1).Value = "Total";
        ws.Cell(row, 2).Value = eval.TotalScore;
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        ws.Cell(row, 1).Value = "Notes";
        ws.Cell(row, 2).Value = eval.FinalNote;
        ws.Row(row).Style.Font.Bold = true;

        row++;
        ws.Cell(row, 1).Value = "Team leader assistant";
        ws.Cell(row, 2).Value = eval.RecommendAsTeamLead ? "Yes" : "No";
        ws.Row(row).Style.Font.Bold = true;

        ws.Columns().AdjustToContents();
    }
}
