using System.Diagnostics;
using Evaluation_App.Services;

namespace Evaluation_App.Forms
{
    public class MainMenuForm : Form
    {
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

            List<Button> controllers = new();

            int top = 90;
            if (AuthService.CurrentUser.IsTeamLead) 
            {
                var btnModifyConfig = CreateButton("Modify Config Files", top);
                btnModifyConfig.Click += BtnModifyConfig_Click;
                controllers.Add(btnModifyConfig);
                top += 50;
            }

            var btnSurvey = CreateButton("Survey", top);
            btnSurvey.Click += BtnSurvey_Click;
            controllers.Add(btnSurvey);
            top += 50;

            var btnCreateReport = CreateButton("Create A Report", top);
            btnCreateReport.Click += BtnCreateReport_Click;
            controllers.Add(btnCreateReport);
            top += 50;

            var btnExit = CreateButton("Exit", top);
            btnExit.Click += (_, _) => Application.Exit();
            controllers.Add(btnExit);

            Controls.Add(title);

            foreach (var control in controllers)
                Controls.Add(control);
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
