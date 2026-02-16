using Evaluation_App.Services;

namespace Evaluation_App.Forms
{
    public partial class EmployeeEvaluationForm : Form
    {
        private readonly Employee _employee;
        private readonly Dictionary<string, NumericUpDown> _inputControls = new();
        private EvaluationResult _evaluationResult;
        private readonly EmployeeEvaluationOptions _employeeOptions;

        public EmployeeEvaluationForm(Employee employee)
        {
            InitializeComponent();
            _employee = employee;
            _employeeOptions = ConfigLoader.LoadEmployeeOptions();

            Text = $"Employee Evaluation - {AuthService.CurrentUser.Name} ({AuthService.CurrentUser.Code})";
            lblTitle.Text = $"{Text} | Rated: {employee.Name} ({employee.Code})";

            _evaluationResult = EvaluationService.LoadEvaluation(_employee.Code)
                ?? new EvaluationResult(_employee.Code, true, ConfigLoader.LoadEmployeeSections());

            LoadSections();
            LoadPreviousAnswers();

            chkTeamLead.Visible = _employeeOptions.AskPreferTeamLeaderAssistant && _employee.IsTeamLead;
            chkTeamLead.Checked = _evaluationResult.RecommendAsTeamLead;

            lblFinalNote.Text = AuthService.CurrentUser.IsTeamLead ? "Leader notes" : "Letter to your teammate";
            txtFinalNote.Text = _evaluationResult.FinalNote;
        }

        private void LoadSections()
        {
            flowLayoutPanel1.Controls.Clear();
            _inputControls.Clear();

            foreach (var section in _evaluationResult.Sections)
            {
                var lblSection = new Label
                {
                    Text = section.Name,
                    AutoSize = false,
                    Width = flowLayoutPanel1.Width - 25,
                    Height = 28,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
                flowLayoutPanel1.Controls.Add(lblSection);

                foreach (var question in section.Questions)
                {
                    var panel = new Panel
                    {
                        Width = flowLayoutPanel1.Width - 25,
                        Height = 45
                    };

                    var lblQ = new Label
                    {
                        Text = $"{question.Text}\n({question.Min}={question.MinMeaning} | {question.Max}={question.MaxMeaning})",
                        AutoSize = false,
                        Width = panel.Width - 80,
                        Height = 42,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Location = new Point(0, 0)
                    };

                    var nud = new NumericUpDown
                    {
                        Minimum = (decimal)question.Min,
                        Maximum = (decimal)question.Max,
                        Value = (decimal)question.Default,
                        Width = 72,
                        Location = new Point(panel.Width - 76, 0),
                        Name = question.Id
                    };

                    panel.Controls.Add(lblQ);
                    panel.Controls.Add(nud);
                    flowLayoutPanel1.Controls.Add(panel);
                    _inputControls[question.Id] = nud;
                }
            }
        }

        private void LoadPreviousAnswers()
        {
            foreach (var kvp in _inputControls)
            {
                if (_evaluationResult.Questions.TryGetValue(kvp.Key, out var value))
                    kvp.Value.Value = Math.Clamp((decimal)value.Score, kvp.Value.Minimum, kvp.Value.Maximum);
            }
        }

        private void ApplyInputsToModel()
        {
            foreach (var section in _evaluationResult.Sections)
            foreach (var question in section.Questions)
                if (_inputControls.TryGetValue(question.Id, out var nud))
                    question.Score = (double)nud.Value;

            _evaluationResult.FinalNote = txtFinalNote.Text;
            _evaluationResult.RecommendAsTeamLead = chkTeamLead.Visible && chkTeamLead.Checked;
            _evaluationResult.SetTotalScore();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ApplyInputsToModel();
            EvaluationService.Save(_evaluationResult);
            MessageBox.Show("Saved successfully.");
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            foreach (var section in _evaluationResult.Sections)
            foreach (var question in section.Questions)
                if (_inputControls.TryGetValue(question.Id, out var nud))
                    nud.Value = (decimal)question.Default;

            chkTeamLead.Checked = false;
            txtFinalNote.Text = string.Empty;

            EvaluationService.Reset(_employee.Code);
            _evaluationResult.Reset();
            MessageBox.Show("Saved successfully.");
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "Load employee evaluation from Excel"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            if (!ExcelExportService.TryLoadEvaluationFromExcel(dialog.FileName, _employee.Code, _evaluationResult))
            {
                MessageBox.Show("Could not load data from selected Excel file.");
                return;
            }

            LoadPreviousAnswers();
            txtFinalNote.Text = _evaluationResult.FinalNote;
            chkTeamLead.Checked = _evaluationResult.RecommendAsTeamLead;
            MessageBox.Show("Loaded successfully.");
        }

        private void btnGenerateExcel_Click(object sender, EventArgs e)
        {
            ApplyInputsToModel();
            EvaluationService.Save(_evaluationResult);
            ExcelExportService.ExportTeamMember(_employee);
            MessageBox.Show("Excel report generated on Desktop.");
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            var list = new EmployeeListForm();
            list.Show();
            Hide();
        }
    }
}
