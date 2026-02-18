using Evaluation_App.Services;

namespace Evaluation_App.Forms
{
    public partial class EmployeeEvaluationForm : Form
    {
        private bool _isNavigating;
        private readonly Dictionary<string, TrackBar> _inputControls = new();
        private readonly Dictionary<string, Label> _valueLabels = new();
        private EmployeeEvaluation _evaluation;

        private EmployeeEvaluationOptions _employeeOptions => ConfigLoader.EmployeeEvaluationConfig.Options;

        public EmployeeEvaluationForm(Employee employee)
        {
            InitializeComponent();
            FormClosing += EmployeeEvaluationForm_FormClosing;

            Text = $"تقييم الموظف - {AuthService.CurrentUser.Name} [{AuthService.CurrentUser.Code}]";
            lblTitle.Text = $"تقييم: {employee.Name} [{employee.Code}]";

            _evaluation = EvaluationService.LoadEvaluation<EmployeeEvaluation>
                (EvaluationBase.BuildFilename(AuthService.CurrentUser, employee))
                ?? new EmployeeEvaluation(AuthService.CurrentUser,
                    ConfigLoader.EmployeeEvaluationConfig.FilteredSectionsForCurrentUser, employee);

            LoadSections();
            LoadPreviousAnswers();

            chkTeamLead.Visible = !employee.IsTeamLead
                && _employeeOptions.AskPreferTeamLeaderAssistant;
            chkTeamLead.Checked = _evaluation.RecommendAsTeamLead;

            lblFinalNote.Text = AuthService.CurrentUser.IsTeamLead ? "ملاحظات قائد الفريق" : "كلمه لزميلك";
            txtFinalNote.Text = _evaluation.FinalNote;
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

        private void LoadSections()
        {
            flowLayoutPanel1.Controls.Clear();
            _inputControls.Clear();
            _valueLabels.Clear();

            foreach (var section in _evaluation.Sections)
            {
                var lblSection = new Label
                {
                    Text = section.Name,
                    AutoSize = false,
                    Width = flowLayoutPanel1.Width - 25,
                    Height = 28,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleRight
                };
                flowLayoutPanel1.Controls.Add(lblSection);

                foreach (var question in section.Questions)
                {
                    var panel = new Panel
                    {
                        Width = flowLayoutPanel1.Width - 25,
                        Height = 116,
                        RightToLeft = RightToLeft.Yes
                    };

                    var lblQ = new Label
                    {
                        Text = question.Text,
                        AutoSize = false,
                        Width = panel.Width,
                        Height = 24,
                        TextAlign = ContentAlignment.MiddleRight,
                        Location = new Point(0, 0)
                    };

                    int min = (int)Math.Round(question.MinValue);
                    int max = (int)Math.Round(question.MaxValue);
                    int def = (int)Math.Round(question.DefaultValue);
                    def = Math.Clamp(def, min, max);

                    var slider = new TrackBar
                    {
                        Minimum = min,
                        Maximum = max,
                        Value = def,
                        TickStyle = TickStyle.None,
                        AutoSize = false,
                        Width = panel.Width - 20,
                        Height = 30,
                        Location = new Point(10, 42),
                        Name = question.Id,
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

                    panel.Controls.Add(lblQ);
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

            _evaluation.FinalNote = txtFinalNote.Text;
            _evaluation.RecommendAsTeamLead = chkTeamLead.Checked;
        }

        private bool HasChanges()
        {
            foreach (var section in _evaluation.Sections)
                foreach (var question in section.Questions)
                    if (_inputControls.TryGetValue(question.Id, out var slider))
                        if (question.Value != slider.Value)
                            return true;

            if (_evaluation.RecommendAsTeamLead != chkTeamLead.Checked)
                return true;

            if (!_evaluation.FinalNote.Equals(txtFinalNote.Text))
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
                        slider.Value = Math.Clamp((int)Math.Round(question.DefaultValue), slider.Minimum, slider.Maximum);

            chkTeamLead.Checked = false;
            txtFinalNote.Text = string.Empty;
            MessageBox.Show("تمت إعادة الضبط.");
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "تحميل تقييم الموظف من ملف Excel"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            EmployeeEvaluation tempEval = _evaluation.Clone();

            if (!ExcelExportService.TryLoadEmployeeEvaluationFromExcel(dialog.FileName, _evaluation))
            {
                MessageBox.Show("تعذر تحميل البيانات من ملف Excel المحدد.");
                return;
            }

            LoadPreviousAnswers();
            txtFinalNote.Text = _evaluation.FinalNote;
            chkTeamLead.Checked = _evaluation.RecommendAsTeamLead;

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

            if (ExcelExportService.TryExportEmployeeEvaluation(_evaluation))
                MessageBox.Show("تم إنشاء تقرير Excel على سطح المكتب.");
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            var shouldNavigate = ConfirmSaveBeforeBack();
            if (!shouldNavigate)
                return;

            var list = new EmployeeListForm();
            list.Show();
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
                    int defaultValue = Math.Clamp((int)Math.Round(question.DefaultValue), slider.Minimum, slider.Maximum);
                    if (slider.Value == defaultValue)
                        return true;
                }

            return false;
        }
    }
}
