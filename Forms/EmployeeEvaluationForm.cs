using static EvaluationFormHelper;

namespace Evaluation_App.Forms
{
    internal partial class EmployeeEvaluationForm : Form
    {
        private bool _isNavigating;
        private readonly Dictionary<string, TrackBar> _inputControls = new();
        private readonly Dictionary<string, Label> _valueLabels = new();
        private EvaluationInstance _evaluationInstance;

        public EmployeeEvaluationForm(Employee employee)
        {
            InitializeComponent();
            FormClosing += EmployeeEvaluationForm_FormClosing;

            Text = $"تقييم الموظف - {AuthService.CurrentUser.Name} [{AuthService.CurrentUser.Code}]";
            lblTitle.Text = $"تقييم: {employee.Name} [{employee.Code}]";

            if(!EvaluationService.TryLoadEvaluation(AuthService.CurrentUser, out _evaluationInstance, employee))
                _evaluationInstance = new EvaluationInstance(AuthService.CurrentUser, employee);

            Initialize(flowLayoutPanel1, _inputControls, _valueLabels, chkTeamLead, txtFinalNote,
                _evaluationInstance);
            LoadSections();
            LoadValues(_evaluationInstance);

            chkTeamLead.Visible = !AuthService.CurrentUser.IsTeamLead &&
                _evaluationInstance.AssistantSectionEnabled();
            chkTeamLead.Checked = _evaluationInstance.RecommendAsTeamLead;

            lblFinalNote.Text = AuthService.CurrentUser.IsTeamLead ? "ملاحظات قائد الفريق" : "كلمه لزميلك";
            txtFinalNote.Text = _evaluationInstance.FinalNote;
        }

        private void EmployeeEvaluationForm_FormClosing(object? sender, FormClosingEventArgs e)
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            EvaluationFormHelper.Load("تحميل تقييم الموظف من ملف Excel");
        }

        private void btnGenerateExcel_Click(object sender, EventArgs e)
        {
            Generate();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (HasChanges())
            {
                var result = MessageBox.Show(
                     "هل تريد حفظ التقييم قبل الرجوع؟",
                     "تنبيه قبل العوده!",
                     MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                    return;

                if (result == DialogResult.Yes)
                {
                    if (HasAnyInputWithDefaultValue())
                    {
                        var confirm = MessageBox.Show(
                            "هناك عناصر ما زالت على القيم الافتراضية. هل تريد المتابعة بالحفظ؟",
                            "تنبيه قبل الحفظ",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (confirm != DialogResult.Yes)
                            return;
                    }
                    else
                    {
                        var confirm = MessageBox.Show(
                            "هناك عناصر ما زالت على القيم الافتراضية. هل تريد المتابعة بالحفظ؟",
                            "تنبيه قبل الحفظ",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (confirm != DialogResult.Yes)
                            return;
                    }

                    ApplyInputsToModel();
                    _evaluationInstance.CalculateScore();
                    EvaluationService.Save(_evaluationInstance);
                    MessageBox.Show("تم الحفظ بنجاح.");
                }
            }

            var list = new EmployeeListForm();
            list.Show();
            _isNavigating = true;
            Hide();
        }
    }
}
