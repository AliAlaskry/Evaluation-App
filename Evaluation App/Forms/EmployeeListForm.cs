using Evaluation_App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Evaluation_App.Forms
{
    public partial class EmployeeListForm : Form
    {
        private List<Employee> allEmployees;

        public EmployeeListForm()
        {
            InitializeComponent();
            LoadEmployees();
            UpdateExportButtonState();
        }

        private void LoadEmployees()
        {
            allEmployees = EmployeeService.LoadEmployees()
                .Where(e => e.Code != AuthService.CurrentUser.Code && e.Include)
                .ToList();

            lstEmployees.DataSource = allEmployees;
            lstEmployees.DisplayMember = "Name";
            lstEmployees.ValueMember = "Code";
        }

        private void btnEvaluate_Click(object sender, EventArgs e)
        {
            if (lstEmployees.SelectedItem is Employee emp)
            {
                var evalForm = new EmployeeEvaluationForm(emp);
                evalForm.FormClosed += (s, args) => UpdateExportButtonState(); // تحديث حالة الأزرار بعد إغلاق النموذج
                evalForm.Show();

                this.Hide();
            }
            else
            {
                MessageBox.Show("اختر موظف لتقييمه");
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            var menuForm = new MainMenuForm();
            menuForm.Show();
            this.Close();
        }

        private void btnExportMembers_Click(object sender, EventArgs e)
        {
            ExcelExportService.ExportTeamMembers();
            MessageBox.Show("تم تصدير تقييم الموظفين فقط إلى ملف Excel.");
        }

        private void btnExpertReport_Click(object sender, EventArgs e)
        {
            ExcelExportService.ExportFullReport();
            MessageBox.Show("تم تصدير جميع التقييمات إلى ملف Excel.");
        }

        private void UpdateExportButtonState()
        {
            bool allRated = allEmployees.All(emp => EvaluationService.HasSavedEvaluation(emp.Code));

            btnExportMembers.Enabled = allRated;  // تصدير الموظفين فقط إذا قيم كل الموظفين
            btnExpertReport.Enabled = allRated;   // التقرير الشامل أيضًا
        }
    }
}
