using Evaluation_App.Services;

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
                Application.Exit();
        }

        private void LoadEmployees()
        {
            allEmployees = EmployeeService.LoadEmployees()
                .Where(e => e.Code != AuthService.CurrentUser.Code && e.Include)
                .ToList();

            lstEmployees.DataSource = allEmployees;
            lstEmployees.DisplayMember = "Name";
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
    }
}
