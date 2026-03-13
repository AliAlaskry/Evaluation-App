using static EvaluationFormHelper;

namespace Evaluation_App.Forms
{
    public partial class SystemEvaluationForm : Form
    {
        private bool _isNavigating;
        private readonly Dictionary<string, TrackBar> _inputControls = new();
        private readonly Dictionary<string, Label> _valueLabels = new();
        private EvaluationInstance _evaluationInstance;

        private SystemEvaluationOptions _systemOptions => ConfigLoader.SystemEvaluationOptions;

        public SystemEvaluationForm()
        {
            InitializeComponent();
            FormClosing += SystemEvaluationForm_FormClosing;
            Text = $"تقييم النظام - {AuthService.CurrentUser.Name} ({AuthService.CurrentUser.Code})";

            if (!EvaluationService.TryLoadEvaluation(AuthService.CurrentUser, out _evaluationInstance))
                _evaluationInstance = new EvaluationInstance(AuthService.CurrentUser);

            chkTeamLeadAssistant.Visible =_evaluationInstance.AssistantSectionEnabled();
            chkTeamLeadAssistant.Checked = _evaluationInstance.ReadyToBeAssistantTeamLeader;

            lstIssues.MeasureItem += lstIssues_MeasureItem;
            lstIssues.DrawItem += lstIssues_DrawItem;

            Initialize(flowLayoutPanel1, _inputControls, _valueLabels, chkTeamLeadAssistant,
                txtSuggestions, _evaluationInstance);

            LoadSections();
            LoadValues(_evaluationInstance);
            LoadIssues();

            txtSuggestions.Text = _evaluationInstance.FinalNote;
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

        private void LoadIssues()
        {
            lstIssues.Items.Clear();
            lstIssues.Enabled = false;

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

            lstIssues.Items.Clear();
            foreach (var issue in _systemOptions.IssuesToResolve)
                lstIssues.Items.Add(issue);
        }

        private int PrefixWidth(Graphics g)
        {
            int maxN = Math.Max(1, lstIssues.Items.Count);
            string prefix = $"{maxN}. ";
            return TextRenderer.MeasureText(g, prefix, lstIssues.Font).Width;
        }

        private void lstIssues_MeasureItem(object? sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0) return;

            using var g = lstIssues.CreateGraphics();
            int prefixW = PrefixWidth(g);

            int textWidth = Math.Max(1, lstIssues.ClientSize.Width - prefixW - 6);
            string text = lstIssues.Items[e.Index]?.ToString() ?? "";

            var size = TextRenderer.MeasureText(
                g,
                text,
                lstIssues.Font,
                new Size(textWidth, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.RightToLeft
            );

            e.ItemHeight = Math.Max(lstIssues.Font.Height + 4, size.Height + 4);
        }

        private void lstIssues_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            string text = lstIssues.Items[e.Index]?.ToString() ?? "";
            string prefix = $"{e.Index + 1}. ";

            int fixedPrefixW = PrefixWidth(e.Graphics);

            // في RTL: الرقم على اليمين
            var prefixRect = new Rectangle(
                e.Bounds.Right - fixedPrefixW - 2,
                e.Bounds.Y + 2,
                fixedPrefixW,
                e.Bounds.Height - 4
            );

            // النص على يسار الرقم ويُلف (wrap)
            var textRect = new Rectangle(
                e.Bounds.X + 2,
                e.Bounds.Y + 2,
                e.Bounds.Width - fixedPrefixW - 6,
                e.Bounds.Height - 4
            );

            var prefixFlags = TextFormatFlags.Right | TextFormatFlags.Top | TextFormatFlags.RightToLeft;
            var textFlags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl
                            | TextFormatFlags.RightToLeft | TextFormatFlags.Right;

            TextRenderer.DrawText(e.Graphics, prefix, lstIssues.Font, prefixRect, e.ForeColor, prefixFlags);
            TextRenderer.DrawText(e.Graphics, text, lstIssues.Font, textRect, e.ForeColor, textFlags);

            e.DrawFocusRectangle();
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
            EvaluationFormHelper.Load("تحميل تقييم النظام من ملف Excel");
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

            var surveyForm = new SurveyForm();
            surveyForm.Show();
            _isNavigating = true;
            Hide();
        }
    }
}
