namespace Evaluation_App.Forms
{
    partial class SystemEvaluationForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnSignOut;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Label lblSuggestions;
        private System.Windows.Forms.TextBox txtSuggestions;
        private System.Windows.Forms.CheckBox chkTeamLeadAssistant;

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
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnSignOut = new System.Windows.Forms.Button();
            this.lblUser = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblUser
            // 

            // chkTeamLeadAssistant
            this.chkTeamLeadAssistant = new System.Windows.Forms.CheckBox();
            this.chkTeamLeadAssistant.AutoSize = true;
            this.chkTeamLeadAssistant.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.chkTeamLeadAssistant.Location = new System.Drawing.Point(12, 35);
            this.chkTeamLeadAssistant.Name = "chkTeamLeadAssistant";
            this.chkTeamLeadAssistant.Size = new System.Drawing.Size(260, 19);
            this.chkTeamLeadAssistant.Text = "هل تحب تكون مساعد مدير الفريق القادم؟";
            this.chkTeamLeadAssistant.UseVisualStyleBackColor = true;

            this.lblUser.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblUser.Location = new System.Drawing.Point(12, 9);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(560, 25);
            this.lblUser.TabIndex = 0;
            this.lblUser.Text = "المستخدم: ...";
            this.lblUser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 60);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(560, 380);
            this.flowLayoutPanel1.TabIndex = 1;
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.WrapContents = false;

            // lblSuggestions
            this.lblSuggestions = new System.Windows.Forms.Label();
            this.lblSuggestions.AutoSize = true;
            this.lblSuggestions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSuggestions.Location = new System.Drawing.Point(12, 445);
            this.lblSuggestions.Name = "lblSuggestions";
            this.lblSuggestions.Size = new System.Drawing.Size(150, 15);
            this.lblSuggestions.Text = "المقترحات والملاحظات (فى شكل نقاط)";

            // txtSuggestions
            this.txtSuggestions = new System.Windows.Forms.TextBox();
            this.txtSuggestions.Location = new System.Drawing.Point(12, 465);
            this.txtSuggestions.Name = "txtSuggestions";
            this.txtSuggestions.Size = new System.Drawing.Size(560, 80);
            this.txtSuggestions.Multiline = true;
            this.txtSuggestions.ScrollBars = ScrollBars.Vertical;
            this.txtSuggestions.Font = new System.Drawing.Font("Segoe UI", 9F);

            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(12, 555);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "حفظ";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(120, 555);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(100, 30);
            this.btnReset.TabIndex = 3;
            this.btnReset.Text = "إعادة التعيين";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnSignOut
            // 
            this.btnSignOut.Location = new System.Drawing.Point(360, 555);
            this.btnSignOut.Name = "btnSignOut";
            this.btnSignOut.Size = new System.Drawing.Size(100, 30);
            this.btnSignOut.TabIndex = 4;
            this.btnSignOut.Text = "تسجيل الخروج";
            this.btnSignOut.UseVisualStyleBackColor = true;
            this.btnSignOut.Click += new System.EventHandler(this.btnSignOut_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(472, 555);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(100, 30);
            this.btnPrint.TabIndex = 5;
            this.btnPrint.Text = "طباعة Excel";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // SystemEvaluationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 610);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.btnSignOut);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.lblUser);
            this.Controls.Add(this.lblSuggestions);
            this.Controls.Add(this.txtSuggestions);
            this.Controls.Add(this.chkTeamLeadAssistant);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "SystemEvaluationForm";
            this.Text = "تقييم النظام";
            this.ResumeLayout(false);
        }
    }
}
