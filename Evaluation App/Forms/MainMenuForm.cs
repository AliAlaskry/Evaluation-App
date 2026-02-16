using System.Diagnostics;
using Evaluation_App.Services;

namespace Evaluation_App.Forms
{
    public class MainMenuForm : Form
    {
        private readonly Label _titleLabel;
        private readonly Button _btnModifyConfig;
        private readonly Button _btnSurvey;
        private readonly Button _btnCreateReport;
        private readonly Button _btnLogout;
        private readonly Button _btnExit;

        public MainMenuForm()
        {
            Text = "Main Menu";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(420, 340);
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            _titleLabel = new Label
            {
                Text = "Main Menu",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 70
            };

            _btnModifyConfig = CreateButton("Modify Config Files");
            _btnModifyConfig.Click += BtnModifyConfig_Click;

            _btnSurvey = CreateButton("Survey");
            _btnSurvey.Click += BtnSurvey_Click;

            _btnCreateReport = CreateButton("Create A Report");
            _btnCreateReport.Click += BtnCreateReport_Click;

            _btnLogout = CreateButton("Log Out");
            _btnLogout.Click += BtnLogout_Click;

            _btnExit = CreateButton("Exit");
            _btnExit.Click += (_, _) => Application.Exit();

            Controls.Add(_titleLabel);
            Controls.Add(_btnModifyConfig);
            Controls.Add(_btnSurvey);
            Controls.Add(_btnCreateReport);
            Controls.Add(_btnLogout);
            Controls.Add(_btnExit);

            ConfigureMenu();
        }

        private void ConfigureMenu()
        {
            _btnModifyConfig.Visible = AuthService.CurrentUser.IsTeamLead;

            const int startTop = 90;
            const int spacing = 50;

            int currentTop = startTop;
            foreach (var button in new[] { _btnModifyConfig, _btnSurvey, _btnCreateReport, _btnLogout, _btnExit })
            {
                if (!button.Visible)
                    continue;

                button.Top = currentTop;
                currentTop += spacing;
            }
        }

        private Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 220,
                Height = 36,
                Left = (ClientSize.Width - 220) / 2,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
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
    }
}
