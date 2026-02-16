namespace Evaluation_App.Forms
{
    partial class SystemEvaluationForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private CheckBox chkTeamLeadAssistant;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label lblIssues;
        private ListBox lstIssues;
        private Label lblSuggestions;
        private TextBox txtSuggestions;
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
            chkTeamLeadAssistant = new CheckBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            lblIssues = new Label();
            lstIssues = new ListBox();
            lblSuggestions = new Label();
            txtSuggestions = new TextBox();
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
            lblTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(760, 48);
            lblTitle.TabIndex = 11;
            lblTitle.Text = "تقييم النظام";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // chkTeamLeadAssistant
            // 
            chkTeamLeadAssistant.AutoSize = true;
            chkTeamLeadAssistant.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            chkTeamLeadAssistant.Location = new Point(12, 55);
            chkTeamLeadAssistant.Name = "chkTeamLeadAssistant";
            chkTeamLeadAssistant.Size = new Size(172, 19);
            chkTeamLeadAssistant.TabIndex = 10;
            chkTeamLeadAssistant.Text = "أرشّح نفسي كمساعد قائد فريق";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(12, 80);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(736, 280);
            flowLayoutPanel1.TabIndex = 9;
            flowLayoutPanel1.WrapContents = false;
            // 
            // lblIssues
            // 
            lblIssues.AutoSize = true;
            lblIssues.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblIssues.Location = new Point(12, 367);
            lblIssues.Name = "lblIssues";
            lblIssues.Size = new Size(106, 15);
            lblIssues.TabIndex = 8;
            lblIssues.Text = "قضايا بحاجة لمعالجة";
            // 
            // lstIssues
            // 
            lstIssues.Location = new Point(12, 387);
            lstIssues.Name = "lstIssues";
            lstIssues.SelectionMode = SelectionMode.None;
            lstIssues.Size = new Size(736, 94);
            lstIssues.TabIndex = 7;
            // 
            // lblSuggestions
            // 
            lblSuggestions.AutoSize = true;
            lblSuggestions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSuggestions.Location = new Point(12, 490);
            lblSuggestions.Name = "lblSuggestions";
            lblSuggestions.Size = new Size(104, 15);
            lblSuggestions.TabIndex = 6;
            lblSuggestions.Text = "ملاحظات / اقتراحات";
            // 
            // txtSuggestions
            // 
            txtSuggestions.Location = new Point(12, 510);
            txtSuggestions.Multiline = true;
            txtSuggestions.Name = "txtSuggestions";
            txtSuggestions.ScrollBars = ScrollBars.Vertical;
            txtSuggestions.Size = new Size(736, 90);
            txtSuggestions.TabIndex = 5;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(12, 612);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 32);
            btnSave.TabIndex = 4;
            btnSave.Text = "حفظ";
            btnSave.Click += btnSave_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(120, 612);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(100, 32);
            btnReset.TabIndex = 3;
            btnReset.Text = "إعادة ضبط";
            btnReset.Click += btnReset_Click;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(228, 612);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(100, 32);
            btnLoad.TabIndex = 2;
            btnLoad.Text = "تحميل";
            btnLoad.Click += btnLoad_Click;
            // 
            // btnGenerateExcel
            // 
            btnGenerateExcel.Location = new Point(540, 612);
            btnGenerateExcel.Name = "btnGenerateExcel";
            btnGenerateExcel.Size = new Size(100, 32);
            btnGenerateExcel.TabIndex = 1;
            btnGenerateExcel.Text = "تصدير Excel";
            btnGenerateExcel.Click += btnGenerateExcel_Click;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(648, 612);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(100, 32);
            btnBack.TabIndex = 0;
            btnBack.Text = "رجوع";
            btnBack.Click += btnBack_Click;
            // 
            // SystemEvaluationForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 655);
            Controls.Add(btnBack);
            Controls.Add(btnGenerateExcel);
            Controls.Add(btnLoad);
            Controls.Add(btnReset);
            Controls.Add(btnSave);
            Controls.Add(txtSuggestions);
            Controls.Add(lblSuggestions);
            Controls.Add(lstIssues);
            Controls.Add(lblIssues);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(chkTeamLeadAssistant);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "SystemEvaluationForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "تقييم النظام";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
