namespace Evaluation_App.Forms
{
    public partial class EmployeeListForm : Form
    {
        private List<Employee> allEmployees = new();
        private bool _isNavigating;

        public EmployeeListForm()
        {
            InitializeComponent();
            FormClosing += EmployeeListForm_FormClosing;
            Text = $"قائمة الموظفين - {AuthService.CurrentUser.Name} [{AuthService.CurrentUser.Code}]";
            LoadEmployees();
        }

        private void EmployeeListForm_FormClosing(object? sender, FormClosingEventArgs e)
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

        private void LoadEmployees()
        {
            allEmployees = EmployeeService.OtherEmployees;

            lstEmployees.DataSource = allEmployees;
            lstEmployees.DisplayMember = "Title";
            lstEmployees.ValueMember = "Code";
        }

        private void OpenSelectedEmployeeEvaluation()
        {
            if (lstEmployees.SelectedItem is not Employee emp)
            {
                MessageBox.Show("اختر موظف لتقييمه");
                return;
            }

            var evalForm = new EmployeeEvaluationForm(emp);
            evalForm.Show();
            _isNavigating = true;
            Hide();
        }

        private void lstEmployees_DoubleClick(object sender, EventArgs e)
        {
            OpenSelectedEmployeeEvaluation();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            var surveyForm = new SurveyForm();
            surveyForm.Show();
            _isNavigating = true;
            Close();
        }

        private void exportExcel_Click(object sender, EventArgs e)
        {
            List<EvaluationInstance> evals = new();
            foreach (var beingEvaluted in EmployeeService.OtherEmployees)
                if (EvaluationService.TryLoadEvaluation(AuthService.CurrentUser, out var temp,
                    beingEvaluted))
                    evals.Add(temp);

            if (ExcelExportService.TryExportMultiEvaluations(Constants.TEAM_MEMBERS_REPORT_FILE_NAME,
                evals))
                MessageBox.Show("تم إنشاء تقرير Excel على سطح المكتب.");
        }
    }
}
