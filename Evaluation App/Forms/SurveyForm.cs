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
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "اختر مجلد يحتوي ملفات Full Servey المراد دمجها"
            };

            if (folderDialog.ShowDialog() != DialogResult.OK)
                return;

            if (!ExcelExportService.TryExportCombinedFullSurvey(folderDialog.SelectedPath))
            {
                MessageBox.Show("تعذر دمج الملفات. تأكد أن كل ملفات Excel لها نفس الصفحات ونفس العمود الأول.");
                return;
            }

            MessageBox.Show("تم إنشاء ملف Sprint Full Survey على سطح المكتب.");
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
