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
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(760, 55);
            lblTitle.TabIndex = 9;
            lblTitle.Text = "تقييم الموظف";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // chkTeamLead
            // 
            chkTeamLead.AutoSize = true;
            chkTeamLead.Location = new Point(12, 58);
            chkTeamLead.Name = "chkTeamLead";
            chkTeamLead.Size = new Size(177, 19);
            chkTeamLead.TabIndex = 8;
            chkTeamLead.Text = "أوصي به/بها كمساعد قائد فريق";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(12, 84);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(736, 360);
            flowLayoutPanel1.TabIndex = 7;
            flowLayoutPanel1.WrapContents = false;
            // 
            // lblFinalNote
            // 
            lblFinalNote.AutoSize = true;
            lblFinalNote.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFinalNote.Location = new Point(12, 452);
            lblFinalNote.Name = "lblFinalNote";
            lblFinalNote.Size = new Size(52, 15);
            lblFinalNote.TabIndex = 6;
            lblFinalNote.Text = "ملاحظات";
            // 
            // txtFinalNote
            // 
            txtFinalNote.Location = new Point(12, 472);
            txtFinalNote.Multiline = true;
            txtFinalNote.Name = "txtFinalNote";
            txtFinalNote.ScrollBars = ScrollBars.Vertical;
            txtFinalNote.Size = new Size(736, 95);
            txtFinalNote.TabIndex = 5;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(12, 580);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 32);
            btnSave.TabIndex = 4;
            btnSave.Text = "حفظ";
            btnSave.Click += btnSave_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(120, 580);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(100, 32);
            btnReset.TabIndex = 3;
            btnReset.Text = "إعادة ضبط";
            btnReset.Click += btnReset_Click;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(228, 580);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(100, 32);
            btnLoad.TabIndex = 2;
            btnLoad.Text = "تحميل";
            btnLoad.Click += btnLoad_Click;
            // 
            // btnGenerateExcel
            // 
            btnGenerateExcel.Location = new Point(540, 580);
            btnGenerateExcel.Name = "btnGenerateExcel";
            btnGenerateExcel.Size = new Size(100, 32);
            btnGenerateExcel.TabIndex = 1;
            btnGenerateExcel.Text = "تصدير Excel";
            btnGenerateExcel.Click += btnGenerateExcel_Click;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(648, 580);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(100, 32);
            btnBack.TabIndex = 0;
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
            Name = "EmployeeEvaluationForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "تقييم الموظف";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
