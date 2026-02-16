using Evaluation_App.Services;
using static Constants;

namespace Evaluation_App.Forms
{
    public partial class SystemEvaluationForm : Form
    {
        private bool _isNavigating;
        private readonly Dictionary<string, TrackBar> _inputControls = new();
        private readonly Dictionary<string, Label> _valueLabels = new();
        private EvaluationResult _evaluationResult;
        private EmployeeEvaluationOptions _employeeOptions;

        public SystemEvaluationForm()
        {
            InitializeComponent();
            FormClosing += SystemEvaluationForm_FormClosing;
            Text = $"تقييم النظام - {AuthService.CurrentUser.Name} ({AuthService.CurrentUser.Code})";
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

        private void SystemEvaluationForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_isNavigating && e.CloseReason == CloseReason.UserClosing)
                Application.Exit();
        }

        private void LoadSections()
        {
            flowLayoutPanel1.Controls.Clear();
            _inputControls.Clear();
            _valueLabels.Clear();

            foreach (var section in _evaluationResult.Sections)
            {
                var sectionLabel = new Label
                {
                    Text = section.Name,
                    AutoSize = false,
                    Width = flowLayoutPanel1.Width - 25,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleRight,
                    Margin = new Padding(3, 10, 3, 3)
                };
                flowLayoutPanel1.Controls.Add(sectionLabel);

                foreach (var question in section.Questions)
                {
                    var panel = new Panel
                    {
                        Width = flowLayoutPanel1.Width - 25,
                        Height = 95,
                        Margin = new Padding(3),
                        RightToLeft = RightToLeft.Yes
                    };

                    var qLabel = new Label
                    {
                        Text = question.Text,
                        AutoSize = false,
                        Width = panel.Width,
                        Height = 24,
                        Location = new Point(0, 0),
                        TextAlign = ContentAlignment.MiddleRight
                    };

                    int min = (int)Math.Round(question.Min);
                    int max = (int)Math.Round(question.Max);
                    int def = Math.Clamp((int)Math.Round(question.Default), min, max);

                    var slider = new TrackBar
                    {
                        Minimum = min,
                        Maximum = max,
                        Value = def,
                        TickStyle = TickStyle.None,
                        AutoSize = false,
                        Width = panel.Width - 20,
                        Height = 30,
                        Name = question.Id,
                        Location = new Point(10, 28),
                        RightToLeft = RightToLeft.No
                    };

                    var minLabel = new Label
                    {
                        Text = $"{question.Min}: {question.MinMeaning}",
                        AutoSize = false,
                        Width = (panel.Width / 2) - 10,
                        Height = 32,
                        Location = new Point(10, 60),
                        TextAlign = ContentAlignment.TopLeft,
                        ForeColor = Color.DimGray
                    };

                    var maxLabel = new Label
                    {
                        Text = $"{question.Max}: {question.MaxMeaning}",
                        AutoSize = false,
                        Width = (panel.Width / 2) - 10,
                        Height = 32,
                        Location = new Point(panel.Width / 2, 60),
                        TextAlign = ContentAlignment.TopRight,
                        ForeColor = Color.DimGray
                    };

                    var valueLabel = new Label
                    {
                        Text = slider.Value.ToString(),
                        AutoSize = false,
                        Width = 60,
                        Height = 20,
                        Location = new Point(panel.Width - 70, 30),
                        TextAlign = ContentAlignment.MiddleRight,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                    };

                    slider.ValueChanged += (_, _) => valueLabel.Text = slider.Value.ToString();

                    panel.Controls.Add(qLabel);
                    panel.Controls.Add(slider);
                    panel.Controls.Add(minLabel);
                    panel.Controls.Add(maxLabel);
                    panel.Controls.Add(valueLabel);
                    flowLayoutPanel1.Controls.Add(panel);

                    _inputControls[question.Id] = slider;
                    _valueLabels[question.Id] = valueLabel;
                }
            }
        }

        private void LoadIssues()
        {
            lstIssues.Items.Clear();

            if (_employeeOptions.IssuesToResolve == null || !_employeeOptions.IssuesToResolve.Any())
            {
                lblIssues.Visible = false;
                lstIssues.Visible = false;
                return;
            }

            lblIssues.Visible = true;
            lstIssues.Visible = true;

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
                    kvp.Value.Value = Math.Clamp((int)Math.Round(value.Score), kvp.Value.Minimum, kvp.Value.Maximum);
                    _valueLabels[kvp.Key].Text = kvp.Value.Value.ToString();
                }
            }
        }

        private void ApplyInputsToModel()
        {
            foreach (var section in _evaluationResult.Sections)
            foreach (var question in section.Questions)
                if (_inputControls.TryGetValue(question.Id, out var slider))
                    question.Score = slider.Value;

            _evaluationResult.RecommendAsTeamLead = chkTeamLeadAssistant.Visible && chkTeamLeadAssistant.Checked;
            _evaluationResult.FinalNote = txtSuggestions.Text;
            _evaluationResult.SetTotalScore();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ApplyInputsToModel();
            EvaluationService.Save(_evaluationResult);
            MessageBox.Show("تم الحفظ بنجاح.");
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            foreach (var section in _evaluationResult.Sections)
            foreach (var question in section.Questions)
                if (_inputControls.TryGetValue(question.Id, out var slider))
                    slider.Value = Math.Clamp((int)Math.Round(question.Default), slider.Minimum, slider.Maximum);

            txtSuggestions.Text = string.Empty;
            chkTeamLeadAssistant.Checked = false;
            EvaluationService.Reset(SYSTEM_EVALUATION_CODE);
            _evaluationResult.Reset();
            MessageBox.Show("تمت إعادة الضبط.");
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "تحميل تقييم النظام من ملف Excel"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            if (!ExcelExportService.TryLoadEvaluationFromExcel(dialog.FileName, SYSTEM_EVALUATION_CODE, _evaluationResult))
            {
                MessageBox.Show("تعذر تحميل البيانات من ملف Excel المحدد.");
                return;
            }

            LoadPreviousAnswers();
            txtSuggestions.Text = _evaluationResult.FinalNote;
            chkTeamLeadAssistant.Checked = _evaluationResult.RecommendAsTeamLead;
            MessageBox.Show("تم التحميل بنجاح.");
        }

        private void btnGenerateExcel_Click(object sender, EventArgs e)
        {
            ApplyInputsToModel();
            EvaluationService.Save(_evaluationResult);
            ExcelExportService.ExportSystemEvaluation();
            MessageBox.Show("تم إنشاء تقرير Excel على سطح المكتب.");
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            var surveyForm = new SurveyForm();
            surveyForm.Show();
            _isNavigating = true;
            Hide();
        }
    }
}
