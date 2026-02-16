namespace Evaluation_App.Forms
{
    partial class EmployeeEvaluationForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Label lblEmployee;
        private System.Windows.Forms.CheckBox chkTeamLead;
        private System.Windows.Forms.TextBox txtFinalNote;
        private System.Windows.Forms.Label lblFinalNote;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.lblEmployee = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // lblEmployee
            this.lblEmployee.AutoSize = true;
            this.lblEmployee.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblEmployee.Location = new System.Drawing.Point(12, 9);
            this.lblEmployee.Name = "lblEmployee";
            this.lblEmployee.Size = new System.Drawing.Size(120, 19);
            this.lblEmployee.Text = "الموظف: ...";

            // chkTeamLead
            this.chkTeamLead = new System.Windows.Forms.CheckBox();
            this.chkTeamLead.AutoSize = true;
            this.chkTeamLead.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.chkTeamLead.Location = new System.Drawing.Point(12, 35);
            this.chkTeamLead.Name = "chkTeamLead";
            this.chkTeamLead.Size = new System.Drawing.Size(160, 19);
            this.chkTeamLead.Text = "هل ترشحه قائد فريق";

            // flowLayoutPanel1
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 65);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(560, 370);
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.WrapContents = false;

            // lblFinalNote
            this.lblFinalNote = new System.Windows.Forms.Label();
            this.lblFinalNote.AutoSize = true;
            this.lblFinalNote.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFinalNote.Location = new System.Drawing.Point(12, 440);
            this.lblFinalNote.Text = "ملاحظات المدير / رسالة لزميلك:";

            // txtFinalNote
            this.txtFinalNote = new System.Windows.Forms.TextBox();
            this.txtFinalNote.Location = new System.Drawing.Point(12, 460);
            this.txtFinalNote.Size = new System.Drawing.Size(560, 80);
            this.txtFinalNote.Multiline = true;
            this.txtFinalNote.ScrollBars = ScrollBars.Vertical;
            this.txtFinalNote.Font = new System.Drawing.Font("Segoe UI", 9F);

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(12, 550);
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.Text = "حفظ";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // btnReset
            this.btnReset.Location = new System.Drawing.Point(120, 550);
            this.btnReset.Size = new System.Drawing.Size(100, 30);
            this.btnReset.Text = "إعادة التعيين";
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            // btnBack
            this.btnBack.Location = new System.Drawing.Point(360, 550);
            this.btnBack.Size = new System.Drawing.Size(100, 30);
            this.btnBack.Text = "رجوع";
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);

            // btnPrint
            this.btnPrint.Location = new System.Drawing.Point(472, 550);
            this.btnPrint.Size = new System.Drawing.Size(100, 30);
            this.btnPrint.Text = "طباعة Excel";
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);

            // EmployeeEvaluationForm
            this.ClientSize = new System.Drawing.Size(584, 600);
            this.Controls.Add(this.lblEmployee);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.chkTeamLead);
            this.Controls.Add(this.lblFinalNote);
            this.Controls.Add(this.txtFinalNote);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "EmployeeEvaluationForm";
            this.Text = "تقييم الموظف";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
