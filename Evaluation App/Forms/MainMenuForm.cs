using System.Diagnostics;
using Evaluation_App.Services;

namespace Evaluation_App.Forms
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
            ConfigureMenu();
        }

        private void ConfigureMenu()
        {
            btnModifyConfig.Visible = btnModifyConfig.Enabled = AuthService.CurrentUser.IsTeamLead;

            const int startTop = 90;
            const int spacing = 50;

            int currentTop = startTop;
            foreach (var button in new[] { btnModifyConfig, btnSurvey, btnCreateReport, btnLogout, btnExit })
            {
                if (!button.Enabled)
                    continue;

                button.Top = currentTop;
                currentTop += spacing;
            }
        }

        private void BtnModifyConfig_Click(object? sender, EventArgs e)
        {
            if (!AuthService.CurrentUser.IsTeamLead)
            {
                MessageBox.Show("This option is only available for team leaders.");
                return;
            }

            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataFolder))
            {
                MessageBox.Show("Config folder was not found.");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = dataFolder,
                UseShellExecute = true
            });
        }

        private void BtnSurvey_Click(object? sender, EventArgs e)
        {
            var systemForm = new SystemEvaluationForm();
            systemForm.FormClosed += (_, _) => Show();
            systemForm.Show();
            Hide();
        }

        private void BtnCreateReport_Click(object? sender, EventArgs e)
        {
            ExcelExportService.ExportFullReport();
            MessageBox.Show("Full report has been created on Desktop.");
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            Hide();
            AuthService.Logout();
            var loginForm = new LoginForm();
            loginForm.Show();
        }

        private void BtnExit_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
