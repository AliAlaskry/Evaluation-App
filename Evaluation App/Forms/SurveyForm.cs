using Evaluation_App.Services;

namespace Evaluation_App.Forms
{
    public partial class SurveyForm : Form
    {
        public SurveyForm()
        {
            InitializeComponent();
            Text = $"التقييمات - {AuthService.CurrentUser.Name} ({AuthService.CurrentUser.Code})";
            lblTitle.Text = Text;
        }

        private void BtnRateSystem_Click(object sender, EventArgs e)
        {
            var form = new SystemEvaluationForm();
            form.FormClosed += (_, _) => Show();
            form.Show();
            Hide();
        }

        private void BtnRateTeammates_Click(object sender, EventArgs e)
        {
            var form = new EmployeeListForm();
            form.FormClosed += (_, _) => Show();
            form.Show();
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
            Close();
        }
    }
}
