namespace Evaluation_App.Forms
{
    partial class MainMenuForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnModifyConfig;
        private System.Windows.Forms.Button btnSurvey;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Button btnExit;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnModifyConfig = new Button();
            btnSurvey = new Button();
            btnLogout = new Button();
            btnExit = new Button();
            lblTitle = new Label();
            SuspendLayout();
            // 
            // btnModifyConfig
            // 
            btnModifyConfig.Font = new Font("Segoe UI", 10F);
            btnModifyConfig.Location = new Point(100, 90);
            btnModifyConfig.Name = "btnModifyConfig";
            btnModifyConfig.Size = new Size(220, 36);
            btnModifyConfig.TabIndex = 1;
            btnModifyConfig.Text = "تعديل ملفات الإعدادات";
            btnModifyConfig.UseVisualStyleBackColor = true;
            btnModifyConfig.Click += BtnModifyConfig_Click;
            // 
            // btnSurvey
            // 
            btnSurvey.Font = new Font("Segoe UI", 10F);
            btnSurvey.Location = new Point(100, 140);
            btnSurvey.Name = "btnSurvey";
            btnSurvey.Size = new Size(220, 36);
            btnSurvey.TabIndex = 2;
            btnSurvey.Text = "الإستبيان";
            btnSurvey.UseVisualStyleBackColor = true;
            btnSurvey.Click += BtnSurvey_Click;
            // 
            // btnLogout
            // 
            btnLogout.Font = new Font("Segoe UI", 10F);
            btnLogout.Location = new Point(100, 195);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(220, 36);
            btnLogout.TabIndex = 4;
            btnLogout.Text = "تسجيل الخروج";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += BtnLogout_Click;
            // 
            // btnExit
            // 
            btnExit.Font = new Font("Segoe UI", 10F);
            btnExit.Location = new Point(100, 249);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(220, 36);
            btnExit.TabIndex = 5;
            btnExit.Text = "خروج";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += BtnExit_Click;
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(420, 70);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "القائمة الرئيسية";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MainMenuForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(420, 340);
            Controls.Add(btnExit);
            Controls.Add(btnLogout);
            Controls.Add(btnSurvey);
            Controls.Add(btnModifyConfig);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "MainMenuForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "القائمة الرئيسية";
            ResumeLayout(false);
        }
    }
}
