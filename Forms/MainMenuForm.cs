using DocumentFormat.OpenXml.Wordprocessing;
using Evaluation_App.Services;
using System.Diagnostics;

namespace Evaluation_App.Forms
{
    public partial class MainMenuForm : Form
    {
        private bool _isNavigating;

        public MainMenuForm()
        {
            InitializeComponent();
            FormClosing += MainMenuForm_FormClosing;
            Text = $"القائمة الرئيسية";
            lblTitle.Text = $"مرحباً {AuthService.CurrentUser.Title}";
            ConfigureMenu();
        }

        private void MainMenuForm_FormClosing(object? sender, FormClosingEventArgs e)
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

        private void ConfigureMenu()
        {
            btnModifyConfig.Visible = btnModifyConfig.Enabled = AuthService.CurrentUser.IsTeamLead;

            const int startTop = 90;
            const int spacing = 50;

            int currentTop = startTop;
            foreach (var button in new[] { btnModifyConfig, btnSurvey, btnLogout, btnExit })
            {
                if (!button.Enabled)
                    continue;

                button.Top = currentTop;
                currentTop += spacing;
            }
        }

        private void BtnModifyConfig_Click(object? sender, EventArgs e)
        {
            var form = new ConfigFilesMenuForm();
            form.Show();
            _isNavigating = true;
            Hide();
        }

        private void BtnSurvey_Click(object? sender, EventArgs e)
        {
            var surveyForm = new SurveyForm();
            surveyForm.Show();
            _isNavigating = true;
            Hide();
        }

        private void BtnCreateReport_Click(object? sender, EventArgs e)
        {
            
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            _isNavigating = true;
            Hide();
            AuthService.Logout();
            var loginForm = new LoginForm();
            loginForm.Show();
        }

        private void BtnExit_Click(object? sender, EventArgs e)
        {
            if (!ExitConfirmationService.ConfirmExit())
                return;

            Application.Exit();
        }
    }
}
