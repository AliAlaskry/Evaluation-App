using ClosedXML.Excel;
using Evaluation_App.Services;
using static Constants;

public static class ExcelExportService
{
    public static void ExportTeamMembers()
    {
        var employees = EmployeeService.LoadEmployees()
            .Where(e => e.Code != AuthService.CurrentUser.Code)
            .ToList();

        if (!employees.Any())
            return;

        string fileName = Path.Combine(DesktopPath, $"Team Members Report.xlsx");
        using var workbook = new XLWorkbook();

        foreach (var emp in employees)
        {
            ExportEvaluation(workbook, emp.Code, $"تقييم الموظف - {emp.Name}");
        }

        workbook.SaveAs(fileName);
    }
    public static void ExportTeamMember(Employee emp)
    {
        string fileName = Path.Combine(DesktopPath, $"{emp.Name} Report.xlsx");
        using var workbook = new XLWorkbook();

        ExportEvaluation(workbook, emp.Code, $"تقييم الموظف - {emp.Name}");

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
          .Where(e => e.Code != AuthService.CurrentUser.Code)
          .ToList();

        if (employees.Any())
        {
            foreach (var emp in employees)
            {
                ExportEvaluation(workbook, emp.Code, $"تقييم الموظف - {emp.Name}");
            }
        }

        workbook.SaveAs(fileName);
    }

    private static void ExportEvaluation(XLWorkbook workbook, string evalCode, string workSheetTitle)
    {
        var eval = EvaluationService.LoadEvaluation(evalCode);
        if (eval == null) return;

        // إنشاء ورقة عمل باسم الموظف
        var ws = workbook.Worksheets.Add(workSheetTitle);

        int row = 1;
        ws.Cell(row, 1).Value = workSheetTitle;
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        foreach (var section in eval.Sections)
        {
            ws.Cell(row, 1).Value = section.Name;
            ws.Row(row).Style.Font.Bold = true;
            row++;

            foreach (var question in section.Questions)
            {
                ws.Cell(row, 1).Value = question.Text;
                ws.Cell(row, 2).Value = question.Score;
                row++;
            }

            ws.Cell(row, 1).Value = "تقييم القسم الكلي";
            ws.Cell(row, 2).Value = section.TotalScore;
            ws.Row(row).Style.Font.Bold = true;
            row += 2;
        }

        ws.Cell(row, 1).Value = "التقييم الكلي";
        ws.Cell(row, 2).Value = eval.TotalScore;
        ws.Row(row).Style.Font.Bold = true;

        row += 2;

        ws.Cell(row, 1).Value = eval.IsEmployeeEvaluation ? "كلمة ليه" : "مقترحات";
        ws.Cell(row, 2).Value = eval.FinalNote;
        ws.Row(row).Style.Font.Bold = true;

        row++;
        ws.Cell(row, 1).Value = eval.IsEmployeeEvaluation ? "ترشيحه كمساعد لمدير الفريق" : "مستعد يكون مساعد مدير الفريق";
        ws.Cell(row, 2).Value = eval.RecommendAsTeamLead ? "نعم" : "لا";
        ws.Row(row).Style.Font.Bold = true;

        ws.Columns().AdjustToContents();
    }
}
