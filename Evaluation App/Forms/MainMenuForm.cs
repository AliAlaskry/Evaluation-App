using System.Diagnostics;
using Evaluation_App.Services;

namespace Evaluation_App.Forms
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
            Text = $"القائمة الرئيسية";
            lblTitle.Text = $"مرحباً {AuthService.CurrentUser.Name} [{AuthService.CurrentUser.Code}]";
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
                MessageBox.Show("هذا الخيار متاح لقادة الفريق فقط.");
                return;
            }

            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataFolder))
            {
                MessageBox.Show("لم يتم العثور على مجلد الإعدادات.");
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
            var surveyForm = new SurveyForm();
            surveyForm.FormClosed += (_, _) => Show();
            surveyForm.Show();
            Hide();
        }

        private void BtnCreateReport_Click(object? sender, EventArgs e)
        {
            
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
