using System.Diagnostics;
using Evaluation_App.Services;

namespace Evaluation_App.Forms
{
    public class MainMenuForm : Form
    {
        private readonly Button btnModifyConfig;
        private readonly Button btnSurvey;
        private readonly Button btnCreateReport;
        private readonly Button btnExit;

        public MainMenuForm()
        {
            Text = "Main Menu";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(420, 340);
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            var title = new Label
            {
                Text = "Main Menu",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 70
            };

            btnModifyConfig = CreateButton("Modify Config Files", 90);
            btnSurvey = CreateButton("Survey", 140);
            btnCreateReport = CreateButton("Create A Report", 190);
            btnExit = CreateButton("Exist", 240);

            btnModifyConfig.Enabled = AuthService.CurrentUser.IsTeamLead;

            btnModifyConfig.Click += BtnModifyConfig_Click;
            btnSurvey.Click += BtnSurvey_Click;
            btnCreateReport.Click += BtnCreateReport_Click;
            btnExit.Click += (_, _) => Application.Exit();

            Controls.Add(title);
            Controls.AddRange([btnModifyConfig, btnSurvey, btnCreateReport, btnExit]);
        }

        private Button CreateButton(string text, int top)
        {
            return new Button
            {
                Text = text,
                Width = 220,
                Height = 36,
                Left = (ClientSize.Width - 220) / 2,
                Top = top,
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
    }
}
