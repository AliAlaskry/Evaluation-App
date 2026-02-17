using Evaluation_App.Services;

namespace Evaluation_App.Forms
{
    public partial class SurveyForm : Form
    {
        private bool _isNavigating;

        public SurveyForm()
        {
            InitializeComponent();
            FormClosing += SurveyForm_FormClosing;
            Text = $"الإستبيان - {AuthService.CurrentUser.Name} ({AuthService.CurrentUser.Code})";
            btnMergeAllExcel.Visible = AuthService.CurrentUser.IsTeamLead;
            btnMergeAllExcel.Enabled = AuthService.CurrentUser.IsTeamLead;
        }

        private void SurveyForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_isNavigating && e.CloseReason == CloseReason.UserClosing)
            {
                if (!ExitConfirmationService.ConfirmExit())
                {
                    e.Cancel = true;
                    return;
                }

                Application.Exit();
            }
        }

        private void BtnRateSystem_Click(object sender, EventArgs e)
        {
            var form = new SystemEvaluationForm();
            form.Show();
            _isNavigating = true;
            Hide();
        }

        private void BtnRateTeammates_Click(object sender, EventArgs e)
        {
            var form = new EmployeeListForm();
            form.Show();
            _isNavigating = true;
            Hide();
        }

        private void BtnGenerateExcel_Click(object sender, EventArgs e)
        {
            if (ExcelExportService.TryExportFullReport())
                MessageBox.Show("تم إنشاء التقرير الكامل على سطح المكتب.");
        }

        private void BtnMergeExcel_Click(object sender, EventArgs e)
        {
            using var systemDialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "اختر ملف تقييم النظام"
            };

            if (systemDialog.ShowDialog() != DialogResult.OK)
                return;

            using var employeesDialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "اختر ملفات تقييم الموظفين",
                Multiselect = true
            };

            if (employeesDialog.ShowDialog() != DialogResult.OK)
                return;

            if (employeesDialog.FileNames.Length == 0)
            {
                MessageBox.Show("يجب اختيار ملف موظف واحد على الأقل.");
                return;
            }

            if (!ExcelExportService.TryExportCombinedMemberFullSurvey(systemDialog.FileName, employeesDialog.FileNames.ToList()))
            {
                MessageBox.Show("تعذر إنشاء التقرير. تأكد أن الملفات مطابقة لنفس الهيكل والأسئلة.");
                return;
            }

            MessageBox.Show("تم إنشاء التقرير المجمع على سطح المكتب.");
        }

        private void BtnMergeAllExcel_Click(object sender, EventArgs e)
        {
            if (!AuthService.CurrentUser.IsTeamLead)
                return;

            using var folderDialog = new FolderBrowserDialog
            {
                Description = "اختر مجلد يحتوي ملفات Full Servey المراد دمجها"
            };

            if (folderDialog.ShowDialog() != DialogResult.OK)
                return;

            if (!ExcelExportService.TryExportTeamLeadCombinedAllReports(folderDialog.SelectedPath))
            {
                MessageBox.Show("تعذر دمج الملفات. تأكد أن الملفات لها نفس الهيكل.");
                return;
            }

            MessageBox.Show("تم إنشاء تقرير شامل للكل على سطح المكتب.");
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            var menuForm = new MainMenuForm();
            menuForm.Show();
            _isNavigating = true;
            Close();
        }
    }
}
