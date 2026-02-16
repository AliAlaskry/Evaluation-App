using Evaluation_App.Services;
using static Constants;

namespace Evaluation_App.Forms
{
    public partial class SystemEvaluationForm : Form
    {
        private Dictionary<string, NumericUpDown> _inputControls;
        private EvaluationResult _evaluationResult;

        public SystemEvaluationForm()
        {
            InitializeComponent();
            lblUser.Text = $"المستخدم: {AuthService.CurrentUser.Name} ({AuthService.CurrentUser.Code})";
            lblUser.TextAlign = ContentAlignment.MiddleCenter;

            _inputControls = new();

            _evaluationResult = EvaluationService.LoadEvaluation(SYSTEM_EVALUATION_CODE);
            _evaluationResult ??= new(SYSTEM_EVALUATION_CODE, false, ConfigLoader.LoadSystemSections());

            LoadSections();
            LoadPreviousAnswers();

            txtSuggestions.Text = _evaluationResult.FinalNote;
            chkTeamLeadAssistant.Checked = _evaluationResult.RecommendAsTeamLead;
        }

        private void LoadSections()
        {
            flowLayoutPanel1.Controls.Clear();
            _inputControls.Clear();

            foreach (var section in _evaluationResult.Sections)
            {
                Label sectionLabel = new Label
                {
                    Text = section.Name,
                    AutoSize = false,
                    Width = flowLayoutPanel1.Width - 20,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(3, 10, 3, 3)
                };
                flowLayoutPanel1.Controls.Add(sectionLabel);

                foreach (var question in section.Questions)
                {
                    Panel panel = new Panel
                    {
                        Width = flowLayoutPanel1.Width - 20,
                        Height = 30,
                        Margin = new Padding(3)
                    };

                    NumericUpDown nud = new NumericUpDown
                    {
                        Minimum = 0,
                        Maximum = 100,
                        Width = 60,
                        Name = question.Id.ToString(),
                        Location = new Point(0, 0)
                    };

                    Label qLabel = new Label
                    {
                        Text = question.Text,
                        AutoSize = false,
                        Width = panel.Width - nud.Width - 10,
                        Height = 30,
                        Location = new Point(panel.Width - (panel.Width - nud.Width - 10), 0),
                        TextAlign = ContentAlignment.MiddleRight
                    };

                    panel.Controls.Add(nud);
                    panel.Controls.Add(qLabel);
                    flowLayoutPanel1.Controls.Add(panel);

                    _inputControls[question.Id] = nud;
                }
            }
        }

        private void LoadPreviousAnswers()
        {
            foreach (var kvp in _inputControls)
                if (_evaluationResult.Questions.TryGetValue(kvp.Key, out Question value))
                    kvp.Value.Value = (decimal)value.Score;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            foreach (var section in _evaluationResult.Sections)
                foreach (var question in section.Questions)
                    if (_inputControls.TryGetValue(question.Id, out var nud))
                        question.Score = (int)nud.Value;

            _evaluationResult.RecommendAsTeamLead = chkTeamLeadAssistant.Checked;
            _evaluationResult.FinalNote = txtSuggestions.Text;
            _evaluationResult.SetTotalScore();

            EvaluationService.Save(_evaluationResult);
           
            if(MessageBox.Show("تم حفظ تقييم النظام بنجاح!") == DialogResult.OK)
            {
                // افتح EmployeeListForm مباشرة بعد الحفظ
                var empListForm = new EmployeeListForm();
                empListForm.Show();
                this.Hide();
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("هل تريد إعادة تعيين التقييم؟", "تأكيد", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // إعادة جميع الإجابات إلى صفر
                foreach (Control c in flowLayoutPanel1.Controls)
                {
                    if (c is Panel panel)
                    {
                        foreach (Control child in panel.Controls)
                        {
                            if (child is NumericUpDown nud)
                                nud.Value = 0;
                        }
                    }
                }

                txtSuggestions.Text = "";
                chkTeamLeadAssistant.Checked = false;

                // مسح البيانات المخزنة في الخدمة أيضاً
                EvaluationService.Reset(Constants.SYSTEM_EVALUATION_CODE);
                _evaluationResult.Reset();
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            foreach (var section in _evaluationResult.Sections)
                foreach (var question in section.Questions)
                    if (_inputControls.TryGetValue(question.Id, out var nud))
                        question.Score = (int)nud.Value;

            _evaluationResult.RecommendAsTeamLead = chkTeamLeadAssistant.Checked; 
            _evaluationResult.FinalNote = txtSuggestions.Text;
            _evaluationResult.SetTotalScore();

            EvaluationService.Save(_evaluationResult);

            ExcelExportService.ExportSystemEvaluation();
            MessageBox.Show("تم إنشاء تقرير Excel لجميع التقييمات على سطح المكتب.");
        }

        private void btnSignOut_Click(object sender, EventArgs e)
        {
            this.Hide();
            AuthService.Logout();
            var login = new LoginForm();
            login.Show();
        }
    }
}
