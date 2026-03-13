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

            if (!AuthService.CurrentUser.IsTeamLead)
            {
                btnBack.Location = btnMergeAllExcel.Location;
            }
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
            List<EvaluationInstance> evals = new();
            if (EvaluationService.TryLoadEvaluation(AuthService.CurrentUser, out var sysEval))
                evals.Add(sysEval);

            foreach (var beingEvaluted in EmployeeService.OtherEmployees)
                if (EvaluationService.TryLoadEvaluation(AuthService.CurrentUser, out var temp,
                    beingEvaluted))
                    evals.Add(temp);

            if (evals.Count != EmployeeService.OtherEmployees.Count + 1)
            {
                DialogResult result = MessageBox.Show("هل تود انشاء تقرير بما تم الى الآن؟", "تأكيد",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                    return;
            }

            if (ExcelExportService.TryExportMultiEvaluations(
                AuthService.CurrentUser.Title + " " + Constants.FULL_SURVEY_FILE_NAME, evals))
                MessageBox.Show("تم إنشاء التقرير الكامل على سطح المكتب.");
        }

        private void BtnMergeExcel_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "اختر المفات للدمج",
                Multiselect = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            if (dialog.FileNames.Length == 0)
            {
                MessageBox.Show("يجب اختيار ملف موظف واحد على الأقل.");
                return;
            }

            if (!ExcelExportService.TryExportCombinedMemberFullSurvey(dialog.FileNames))
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

            using var dialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "Choose Team Members' Reports",
                Multiselect = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            if (!ExcelExportService.TryExportTeamLeadCombinedAllReports(dialog.FileNames))
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
