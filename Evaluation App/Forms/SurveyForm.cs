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
            Text = $"التقييمات - {AuthService.CurrentUser.Name} ({AuthService.CurrentUser.Code})";
        }

        private void SurveyForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_isNavigating && e.CloseReason == CloseReason.UserClosing)
                Application.Exit();
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
            ExcelExportService.ExportFullReport();
            MessageBox.Show("تم إنشاء التقرير الكامل على سطح المكتب.");
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
