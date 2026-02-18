using Evaluation_App.Services;
using System.Net.Security;
using static Constants;

namespace Evaluation_App.Forms
{
    public partial class SystemEvaluationForm : Form
    {
        private bool _isNavigating;
        private readonly Dictionary<string, TrackBar> _inputControls = new();
        private readonly Dictionary<string, Label> _valueLabels = new();
        private SystemEvaluation _evaluation;

        private SystemEvaluationOptions _systemOptions => ConfigLoader.SystemEvaluationConfig.Options;
        private EmployeeEvaluationOptions _employeeOptions => ConfigLoader.EmployeeEvaluationConfig.Options;

        public SystemEvaluationForm()
        {
            InitializeComponent();
            FormClosing += SystemEvaluationForm_FormClosing;
            Text = $"تقييم النظام - {AuthService.CurrentUser.Name} ({AuthService.CurrentUser.Code})";

            _evaluation = EvaluationService.LoadEvaluation<SystemEvaluation>
                (EvaluationBase.BuildFilename(AuthService.CurrentUser))
                ?? new SystemEvaluation(AuthService.CurrentUser,  
                    ConfigLoader.SystemEvaluationConfig.FilteredSectionsForCurrentUser);

            chkTeamLeadAssistant.Visible = !AuthService.CurrentUser.IsTeamLead 
                && _employeeOptions.AskPreferTeamLeaderAssistant;
            chkTeamLeadAssistant.Checked = _evaluation.ReadyToBeAssistantTeamLeader;

            LoadSections();
            LoadPreviousAnswers();
            LoadIssues();

            txtSuggestions.Text = _evaluation.FinalNote;
        }

        private void SystemEvaluationForm_FormClosing(object? sender, FormClosingEventArgs e)
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

        private void LoadSections()
        {
            flowLayoutPanel1.Controls.Clear();
            _inputControls.Clear();
            _valueLabels.Clear();

            foreach (var section in _evaluation.Sections)
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
                        Height = 116,
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

                    int min = (int)Math.Round(question.MinValue);
                    int max = (int)Math.Round(question.MaxValue);
                    int def = Math.Clamp((int)Math.Round(question.DefaultValue), min, max);

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
                        Location = new Point(10, 42),
                        RightToLeft = RightToLeft.No
                    };

                    const int hintLabelY = 76;
                    int hintLabelWidth = (slider.Width / 2) - 4;

                    var minLabel = new Label
                    {
                        Text = question.MinValueMeaning,
                        AutoSize = false,
                        Width = hintLabelWidth,
                        Height = 30,
                        Location = new Point(slider.Left, hintLabelY),
                        TextAlign = ContentAlignment.TopLeft,
                        ForeColor = Color.DimGray
                    };

                    var maxLabel = new Label
                    {
                        Text = question.MaxValueMeaning,
                        AutoSize = false,
                        Width = hintLabelWidth,
                        Height = 30,
                        Location = new Point(slider.Right - hintLabelWidth, hintLabelY),
                        TextAlign = ContentAlignment.TopRight,
                        ForeColor = Color.DimGray
                    };

                    var valueLabel = new Label
                    {
                        Text = string.Empty,
                        AutoSize = false,
                        Width = 48,
                        Height = 20,
                        Location = new Point(slider.Left, slider.Top - 18),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                    };

                    slider.ValueChanged += (_, _) => UpdateValueLabelPosition(valueLabel, slider);

                    panel.Controls.Add(qLabel);
                    panel.Controls.Add(slider);
                    panel.Controls.Add(minLabel);
                    panel.Controls.Add(maxLabel);
                    panel.Controls.Add(valueLabel);
                    UpdateValueLabelPosition(valueLabel, slider);
                    flowLayoutPanel1.Controls.Add(panel);

                    _inputControls[question.Id] = slider;
                    _valueLabels[question.Id] = valueLabel;
                }
            }
        }

        private void LoadIssues()
        {
            lstIssues.Items.Clear();

            if (_systemOptions.IssuesToResolve == null || !_systemOptions.IssuesToResolve.Any())
            {
                lblIssues.Visible = false;
                lstIssues.Visible = false;

                lblSuggestions.Location = new Point(12, 369);
                flowLayoutPanel1.Size = new Size(736, 401);
                return;
            }

            lblIssues.Visible = true;
            lstIssues.Visible = true;

            int i = 1;
            foreach (var issue in _systemOptions.IssuesToResolve)
            {
                lstIssues.Items.Add($"{i}. {issue}");
                i++;
            }
        }

        private void LoadPreviousAnswers()
        {
            foreach (var kvp in _inputControls)
            {
                if (_evaluation.Questions.TryGetValue(kvp.Key, out var value))
                {
                    kvp.Value.Value = Math.Clamp((int)Math.Round(value.Value), kvp.Value.Minimum, kvp.Value.Maximum);
                    UpdateValueLabelPosition(_valueLabels[kvp.Key], kvp.Value);
                }
            }
        }

        private static void UpdateValueLabelPosition(Label valueLabel, TrackBar slider)
        {
            valueLabel.Text = slider.Value.ToString();

            int valueRange = Math.Max(1, slider.Maximum - slider.Minimum);
            int trackWidth = Math.Max(1, slider.Width - 16);
            double ratio = (slider.Value - slider.Minimum) / (double)valueRange;
            int thumbX = slider.Left + 8 + (int)Math.Round(trackWidth * ratio);

            valueLabel.Left = Math.Clamp(thumbX - (valueLabel.Width / 2), slider.Left, slider.Right - valueLabel.Width);
            valueLabel.Top = slider.Top - 18;
        }

        private void ApplyInputsToModel()
        {
            foreach (var section in _evaluation.Sections)
            foreach (var question in section.Questions)
                if (_inputControls.TryGetValue(question.Id, out var slider))
                    question.Value = slider.Value;

            _evaluation.ReadyToBeAssistantTeamLeader = chkTeamLeadAssistant.Checked;
            _evaluation.FinalNote = txtSuggestions.Text;
            _evaluation.CalculateScore();
        }

        private bool HasChanges()
        {
            foreach (var section in _evaluation.Sections)
                foreach (var question in section.Questions)
                    if (_inputControls.TryGetValue(question.Id, out var slider))
                        if (question.Value != slider.Value)
                            return true;

            if (_evaluation.ReadyToBeAssistantTeamLeader != chkTeamLeadAssistant.Checked)
                return true;

            if(!_evaluation.FinalNote.Equals(txtSuggestions.Text))
                return true;

            return false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!HasChanges())
            {
                MessageBox.Show("لا توجد تغييرات للحفظ.");
                return;
            }

            var confirm = MessageBox.Show("هل تريد حفظ التغييرات؟", "تأكيد الحفظ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            if (!ConfirmSaveWithDefaultValuesWarning())
                return;

            ApplyInputsToModel();
            _evaluation.CalculateScore();
            EvaluationService.Save(_evaluation);
            MessageBox.Show("تم الحفظ بنجاح.");
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (HasChanges())
            {
                var result = MessageBox.Show(
                 "لم يتم حفظ المعلومات. هل تريد الحفظ والمتابعه؟",
                 "تنبة قبل التصدير",
                 MessageBoxButtons.YesNo,
                 MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    return;
            }

            foreach (var section in _evaluation.Sections)
                foreach (var question in section.Questions)
                    if (_inputControls.TryGetValue(question.Id, out var slider))
                        slider.Value = Math.Clamp((int)Math.Round(question.DefaultValue),
                            slider.Minimum, slider.Maximum);

            txtSuggestions.Text = string.Empty;
            chkTeamLeadAssistant.Checked = false;
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

            SystemEvaluation tempEval = _evaluation.Clone();
            
            if (!ExcelExportService.TryLoadSystemEvaluationFromExcel(dialog.FileName, _evaluation))
            {
                MessageBox.Show("تعذر تحميل البيانات من ملف Excel المحدد.");
                return;
            }

            LoadPreviousAnswers();
            txtSuggestions.Text = _evaluation.FinalNote;
            chkTeamLeadAssistant.Checked = _evaluation.ReadyToBeAssistantTeamLeader;

            _evaluation = tempEval;

            MessageBox.Show("تم التحميل بنجاح.");
        }

        private void btnGenerateExcel_Click(object sender, EventArgs e)
        {
            if (HasChanges())
            {
                var result = MessageBox.Show(
                 "لم يتم حفظ المعلومات. هل تريد الحفظ والمتابعه؟",
                 "تنبة قبل التصدير",
                 MessageBoxButtons.YesNo,
                 MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    return;
            }

            ApplyInputsToModel();
            _evaluation.CalculateScore();
            EvaluationService.Save(_evaluation);

            if (ExcelExportService.TryExportSystemEvaluation(_evaluation))
                MessageBox.Show("تم إنشاء تقرير Excel على سطح المكتب.");
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            var shouldNavigate = ConfirmSaveBeforeBack();
            if (!shouldNavigate)
                return;

            var surveyForm = new SurveyForm();
            surveyForm.Show();
            _isNavigating = true;
            Hide();
        }

        private bool ConfirmSaveBeforeBack()
        {
            if (!HasChanges())
                return true;

            var result = MessageBox.Show("هل تريد حفظ التقييم قبل الرجوع؟", "تأكيد", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
                return false;

            if (result == DialogResult.Yes)
            {
                if (!ConfirmSaveWithDefaultValuesWarning())
                    return false;

                ApplyInputsToModel();
                _evaluation.CalculateScore();
                EvaluationService.Save(_evaluation);
            }

            return true;
        }

        private bool ConfirmSaveWithDefaultValuesWarning()
        {
            if (!HasAnyInputWithDefaultValue())
                return true;

            var result = MessageBox.Show(
                 "هناك عناصر ما زالت على القيم الافتراضية. هل تريد المتابعة بالحفظ؟",
                 "تنبيه قبل الحفظ",
                 MessageBoxButtons.YesNo,
                 MessageBoxIcon.Warning);

            return result == DialogResult.Yes;
        }

        private bool HasAnyInputWithDefaultValue()
        {
            foreach (var section in _evaluation.Sections)
            foreach (var question in section.Questions)
                if (_inputControls.TryGetValue(question.Id, out var slider))
                {
                    int defaultValue = Math.Clamp((int)Math.Round(question.DefaultValue), 
                        slider.Minimum, slider.Maximum);
                    if (slider.Value == defaultValue)
                        return true;
                }

            return false;
        }
    }
}
