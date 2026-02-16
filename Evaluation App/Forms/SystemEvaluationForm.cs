using Evaluation_App.Services;
using static Constants;

namespace Evaluation_App.Forms
{
    public partial class SystemEvaluationForm : Form
    {
        private readonly Dictionary<string, NumericUpDown> _inputControls = new();
        private EvaluationResult _evaluationResult;
        private EmployeeEvaluationOptions _employeeOptions;

        public SystemEvaluationForm()
        {
            InitializeComponent();
            Text = $"System Evaluation - {AuthService.CurrentUser.Name} ({AuthService.CurrentUser.Code})";
            lblTitle.Text = Text;

            _employeeOptions = ConfigLoader.LoadEmployeeOptions();
            _evaluationResult = EvaluationService.LoadEvaluation(SYSTEM_EVALUATION_CODE)
                ?? new EvaluationResult(SYSTEM_EVALUATION_CODE, false, ConfigLoader.LoadSystemSections());

            chkTeamLeadAssistant.Visible = _employeeOptions.AskPreferTeamLeaderAssistant;
            LoadSections();
            LoadPreviousAnswers();
            LoadIssues();

            txtSuggestions.Text = _evaluationResult.FinalNote;
            chkTeamLeadAssistant.Checked = _evaluationResult.RecommendAsTeamLead;
        }

        private void LoadSections()
        {
            flowLayoutPanel1.Controls.Clear();
            _inputControls.Clear();

            foreach (var section in _evaluationResult.Sections)
            {
                var sectionLabel = new Label
                {
                    Text = section.Name,
                    AutoSize = false,
                    Width = flowLayoutPanel1.Width - 25,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(3, 10, 3, 3)
                };
                flowLayoutPanel1.Controls.Add(sectionLabel);

                foreach (var question in section.Questions)
                {
                    var panel = new Panel
                    {
                        Width = flowLayoutPanel1.Width - 25,
                        Height = 45,
                        Margin = new Padding(3)
                    };

                    var nud = new NumericUpDown
                    {
                        Minimum = (decimal)question.Min,
                        Maximum = (decimal)question.Max,
                        Value = (decimal)question.Default,
                        DecimalPlaces = 0,
                        Width = 70,
                        Name = question.Id,
                        Location = new Point(0, 0)
                    };

                    var qLabel = new Label
                    {
                        Text = $"{question.Text}\n({question.Min}={question.MinMeaning} | {question.Max}={question.MaxMeaning})",
                        AutoSize = false,
                        Width = panel.Width - nud.Width - 15,
                        Height = 42,
                        Location = new Point(nud.Width + 10, 0),
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    panel.Controls.Add(nud);
                    panel.Controls.Add(qLabel);
                    flowLayoutPanel1.Controls.Add(panel);

                    _inputControls[question.Id] = nud;
                }
            }
        }

        private void LoadIssues()
        {
            lstIssues.Items.Clear();
            int i = 1;
            foreach (var issue in _employeeOptions.IssuesToResolve)
            {
                lstIssues.Items.Add($"{i}. {issue}");
                i++;
            }
        }

        private void LoadPreviousAnswers()
        {
            foreach (var kvp in _inputControls)
            {
                if (_evaluationResult.Questions.TryGetValue(kvp.Key, out var value))
                {
                    kvp.Value.Value = Math.Clamp((decimal)value.Score, kvp.Value.Minimum, kvp.Value.Maximum);
                }
            }
        }

        private void ApplyInputsToModel()
        {
            foreach (var section in _evaluationResult.Sections)
            foreach (var question in section.Questions)
                if (_inputControls.TryGetValue(question.Id, out var nud))
                    question.Score = (double)nud.Value;

            _evaluationResult.RecommendAsTeamLead = chkTeamLeadAssistant.Visible && chkTeamLeadAssistant.Checked;
            _evaluationResult.FinalNote = txtSuggestions.Text;
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

            txtSuggestions.Text = string.Empty;
            chkTeamLeadAssistant.Checked = false;
            EvaluationService.Reset(SYSTEM_EVALUATION_CODE);
            _evaluationResult.Reset();
            MessageBox.Show("Saved successfully.");
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "Load system evaluation from Excel"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            if (!ExcelExportService.TryLoadEvaluationFromExcel(dialog.FileName, SYSTEM_EVALUATION_CODE, _evaluationResult))
            {
                MessageBox.Show("Could not load data from selected Excel file.");
                return;
            }

            LoadPreviousAnswers();
            txtSuggestions.Text = _evaluationResult.FinalNote;
            chkTeamLeadAssistant.Checked = _evaluationResult.RecommendAsTeamLead;
            MessageBox.Show("Loaded successfully.");
        }

        private void btnGenerateExcel_Click(object sender, EventArgs e)
        {
            ApplyInputsToModel();
            EvaluationService.Save(_evaluationResult);
            ExcelExportService.ExportSystemEvaluation();
            MessageBox.Show("Excel report generated on Desktop.");
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            var surveyForm = new SurveyForm();
            surveyForm.Show();
            Hide();
        }
    }
}
