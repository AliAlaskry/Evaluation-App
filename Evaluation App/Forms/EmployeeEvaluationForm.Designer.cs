namespace Evaluation_App.Forms
{
    partial class EmployeeEvaluationForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private CheckBox chkTeamLead;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label lblFinalNote;
        private TextBox txtFinalNote;
        private Button btnSave;
        private Button btnReset;
        private Button btnLoad;
        private Button btnGenerateExcel;
        private Button btnBack;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            chkTeamLead = new CheckBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            lblFinalNote = new Label();
            txtFinalNote = new TextBox();
            btnSave = new Button();
            btnReset = new Button();
            btnLoad = new Button();
            btnGenerateExcel = new Button();
            btnBack = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Size = new Size(760, 55);
            lblTitle.Text = "تقييم الموظف";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // chkTeamLead
            // 
            chkTeamLead.AutoSize = true;
            chkTeamLead.Location = new Point(12, 58);
            chkTeamLead.Size = new Size(277, 19);
            chkTeamLead.Text = "أوصي به/بها كمساعد قائد فريق";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(12, 84);
            flowLayoutPanel1.Size = new Size(736, 360);
            flowLayoutPanel1.WrapContents = false;
            // 
            // lblFinalNote
            // 
            lblFinalNote.AutoSize = true;
            lblFinalNote.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFinalNote.Location = new Point(12, 452);
            lblFinalNote.Text = "ملاحظات";
            // 
            // txtFinalNote
            // 
            txtFinalNote.Location = new Point(12, 472);
            txtFinalNote.Multiline = true;
            txtFinalNote.ScrollBars = ScrollBars.Vertical;
            txtFinalNote.Size = new Size(736, 95);
            // 
            // btnSave
            // 
            btnSave.Location = new Point(12, 580);
            btnSave.Size = new Size(100, 32);
            btnSave.Text = "حفظ";
            btnSave.Click += btnSave_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(120, 580);
            btnReset.Size = new Size(100, 32);
            btnReset.Text = "إعادة ضبط";
            btnReset.Click += btnReset_Click;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(228, 580);
            btnLoad.Size = new Size(100, 32);
            btnLoad.Text = "تحميل";
            btnLoad.Click += btnLoad_Click;
            // 
            // btnGenerateExcel
            // 
            btnGenerateExcel.Location = new Point(540, 580);
            btnGenerateExcel.Size = new Size(100, 32);
            btnGenerateExcel.Text = "توليد Excel";
            btnGenerateExcel.Click += btnGenerateExcel_Click;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(648, 580);
            btnBack.Size = new Size(100, 32);
            btnBack.Text = "رجوع";
            btnBack.Click += btnBack_Click;
            // 
            // EmployeeEvaluationForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 625);
            Controls.Add(btnBack);
            Controls.Add(btnGenerateExcel);
            Controls.Add(btnLoad);
            Controls.Add(btnReset);
            Controls.Add(btnSave);
            Controls.Add(txtFinalNote);
            Controls.Add(lblFinalNote);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(chkTeamLead);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Name = "EmployeeEvaluationForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "تقييم الموظف";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
