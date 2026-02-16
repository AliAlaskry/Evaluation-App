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
            lblTitle.Size = new Size(760, 48);
            lblTitle.Text = "تقييم النظام";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // chkTeamLeadAssistant
            // 
            chkTeamLeadAssistant.AutoSize = true;
            chkTeamLeadAssistant.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            chkTeamLeadAssistant.Location = new Point(12, 55);
            chkTeamLeadAssistant.Size = new Size(365, 19);
            chkTeamLeadAssistant.Text = "أرشّح نفسي كمساعد قائد فريق";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(12, 80);
            flowLayoutPanel1.Size = new Size(736, 280);
            flowLayoutPanel1.WrapContents = false;
            // 
            // lblIssues
            // 
            lblIssues.AutoSize = true;
            lblIssues.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblIssues.Location = new Point(12, 367);
            lblIssues.Text = "قضايا بحاجة لمعالجة";
            // 
            // lstIssues
            // 
            lstIssues.Location = new Point(12, 387);
            lstIssues.Size = new Size(736, 94);
            lstIssues.SelectionMode = SelectionMode.None;
            // 
            // lblSuggestions
            // 
            lblSuggestions.AutoSize = true;
            lblSuggestions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSuggestions.Location = new Point(12, 490);
            lblSuggestions.Text = "ملاحظات / اقتراحات";
            // 
            // txtSuggestions
            // 
            txtSuggestions.Location = new Point(12, 510);
            txtSuggestions.Multiline = true;
            txtSuggestions.ScrollBars = ScrollBars.Vertical;
            txtSuggestions.Size = new Size(736, 90);
            // 
            // btnSave
            // 
            btnSave.Location = new Point(12, 612);
            btnSave.Size = new Size(100, 32);
            btnSave.Text = "حفظ";
            btnSave.Click += btnSave_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(120, 612);
            btnReset.Size = new Size(100, 32);
            btnReset.Text = "إعادة ضبط";
            btnReset.Click += btnReset_Click;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(228, 612);
            btnLoad.Size = new Size(100, 32);
            btnLoad.Text = "تحميل";
            btnLoad.Click += btnLoad_Click;
            // 
            // btnGenerateExcel
            // 
            btnGenerateExcel.Location = new Point(540, 612);
            btnGenerateExcel.Size = new Size(100, 32);
            btnGenerateExcel.Text = "توليد Excel";
            btnGenerateExcel.Click += btnGenerateExcel_Click;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(648, 612);
            btnBack.Size = new Size(100, 32);
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
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Name = "SystemEvaluationForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "تقييم النظام";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
