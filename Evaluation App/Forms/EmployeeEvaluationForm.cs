using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Evaluation_App.Forms
{
    public partial class EmployeeEvaluationForm : Form
    {
        private Employee _employee;
        private Dictionary<string, NumericUpDown> _inputControls;
        private EvaluationResult _evaluationResult;

        public EmployeeEvaluationForm(Employee employee)
        {
            InitializeComponent();
            _employee = employee;
            lblEmployee.Text = $"{employee.Name} ({employee.Code})";

            _inputControls = new();

            _evaluationResult = EvaluationService.LoadEvaluation(_employee.Code);
            _evaluationResult ??= new(_employee.Code, true, ConfigLoader.LoadEmployeeSections());

            LoadSections();
            LoadPreviousAnswers();

            chkTeamLead.Checked = _evaluationResult.RecommendAsTeamLead;
            txtFinalNote.Text = _evaluationResult.FinalNote;
        }

        private void LoadSections()
        {
            flowLayoutPanel1.Controls.Clear();
            _inputControls.Clear();

            foreach (var section in _evaluationResult.Sections)
            {
                Label lblSection = new Label
                {
                    Text = section.Name,
                    AutoSize = true,
                    Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
                };
                flowLayoutPanel1.Controls.Add(lblSection);

                foreach (var question in section.Questions)
                {
                    Panel panel = new Panel
                    {
                        Width = flowLayoutPanel1.Width - 25,
                        Height = 30
                    };

                    Label lblQ = new Label
                    {
                        Text = question.Text,
                        AutoSize = false,
                        Width = panel.Width - 70,
                        TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                        Location = new System.Drawing.Point(0, 0)
                    };

                    NumericUpDown nud = new NumericUpDown
                    {
                        Minimum = 0,
                        Maximum = 100,
                        Width = 60,
                        Location = new System.Drawing.Point(panel.Width - 65, 0),
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
                if (_evaluationResult.Questions.TryGetValue(kvp.Key, out Question value))
                    kvp.Value.Value = (decimal)value.Score;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            foreach (var section in _evaluationResult.Sections)
                foreach (var question in section.Questions)
                    if (_inputControls.TryGetValue(question.Id, out var nud))
                        question.Score = (int)nud.Value;

            _evaluationResult.FinalNote = txtFinalNote.Text;
            _evaluationResult.RecommendAsTeamLead = chkTeamLead.Checked;
            _evaluationResult.SetTotalScore();

            EvaluationService.Save(_evaluationResult);

            if(MessageBox.Show("تم حفظ تقييم الموظف بنجاح!") == DialogResult.OK)
            {
                this.Hide();

                var empListForm = new EmployeeListForm();
                empListForm.Show();
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("هل تريد إعادة تعيين التقييم؟", "تأكيد", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (var nud in _inputControls.Values)
                    nud.Value = 0;

                chkTeamLead.Checked = false;
                txtFinalNote.Text = "";

                EvaluationService.Reset(_employee.Code);
                _evaluationResult.Reset();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            foreach (var section in _evaluationResult.Sections)
                foreach (var question in section.Questions)
                    if (_inputControls.TryGetValue(question.Id, out var nud))
                        question.Score = (int)nud.Value;

            _evaluationResult.FinalNote = txtFinalNote.Text;
            _evaluationResult.RecommendAsTeamLead = chkTeamLead.Checked;
            _evaluationResult.SetTotalScore();

            EvaluationService.Save(_evaluationResult);

            ExcelExportService.ExportTeamMember(_employee);
            MessageBox.Show("تم إنشاء تقرير Excel لجميع التقييمات على سطح المكتب.");
        }
    }
}
