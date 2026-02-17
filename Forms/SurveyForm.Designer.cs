namespace Evaluation_App.Forms
{
    partial class SurveyForm
    {
        private System.ComponentModel.IContainer components = null;
        private Button btnRateSystem;
        private Button btnRateTeammates;
        private Button btnGenerateExcel;
        private Button btnMergeExcel;
        private Button btnMergeAllExcel;
        private Button btnBack;

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
            btnRateSystem = new Button();
            btnRateTeammates = new Button();
            btnGenerateExcel = new Button();
            btnMergeExcel = new Button();
            btnMergeAllExcel = new Button();
            btnBack = new Button();
            lblTitle = new Label();
            SuspendLayout();
            //
            // btnRateSystem
            //
            btnRateSystem.Font = new Font("Segoe UI", 10F);
            btnRateSystem.Location = new Point(100, 90);
            btnRateSystem.Name = "btnRateSystem";
            btnRateSystem.Size = new Size(220, 38);
            btnRateSystem.TabIndex = 1;
            btnRateSystem.Text = "تقييم النظام";
            btnRateSystem.UseVisualStyleBackColor = true;
            btnRateSystem.Click += BtnRateSystem_Click;
            //
            // btnRateTeammates
            //
            btnRateTeammates.Font = new Font("Segoe UI", 10F);
            btnRateTeammates.Location = new Point(100, 142);
            btnRateTeammates.Name = "btnRateTeammates";
            btnRateTeammates.Size = new Size(220, 38);
            btnRateTeammates.TabIndex = 2;
            btnRateTeammates.Text = "تقييم الزملاء";
            btnRateTeammates.UseVisualStyleBackColor = true;
            btnRateTeammates.Click += BtnRateTeammates_Click;
            //
            // btnGenerateExcel
            //
            btnGenerateExcel.Font = new Font("Segoe UI", 10F);
            btnGenerateExcel.Location = new Point(100, 194);
            btnGenerateExcel.Name = "btnGenerateExcel";
            btnGenerateExcel.Size = new Size(220, 38);
            btnGenerateExcel.TabIndex = 3;
            btnGenerateExcel.Text = "تصدير excel شامل";
            btnGenerateExcel.UseVisualStyleBackColor = true;
            btnGenerateExcel.Click += BtnGenerateExcel_Click;
            //
            // btnMergeExcel
            //
            btnMergeExcel.Font = new Font("Segoe UI", 10F);
            btnMergeExcel.Location = new Point(100, 246);
            btnMergeExcel.Name = "btnMergeExcel";
            btnMergeExcel.Size = new Size(220, 38);
            btnMergeExcel.TabIndex = 4;
            btnMergeExcel.Text = "تجميع تقرير شامل لعضو";
            btnMergeExcel.UseVisualStyleBackColor = true;
            btnMergeExcel.Click += BtnMergeExcel_Click;
            //
            // btnMergeAllExcel
            //
            btnMergeAllExcel.Font = new Font("Segoe UI", 10F);
            btnMergeAllExcel.Location = new Point(100, 298);
            btnMergeAllExcel.Name = "btnMergeAllExcel";
            btnMergeAllExcel.Size = new Size(220, 38);
            btnMergeAllExcel.TabIndex = 5;
            btnMergeAllExcel.Text = "تجميع تقرير شامل للكل";
            btnMergeAllExcel.UseVisualStyleBackColor = true;
            btnMergeAllExcel.Click += BtnMergeAllExcel_Click;
            //
            // btnBack
            //
            btnBack.Font = new Font("Segoe UI", 10F);
            btnBack.Location = new Point(100, 350);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(220, 38);
            btnBack.TabIndex = 6;
            btnBack.Text = "رجوع";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += BtnBack_Click;
            //
            // lblTitle
            //
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(420, 75);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "إستبيان الـ Sprint";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            //
            // SurveyForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(420, 420);
            Controls.Add(btnBack);
            Controls.Add(btnMergeAllExcel);
            Controls.Add(btnMergeExcel);
            Controls.Add(btnGenerateExcel);
            Controls.Add(btnRateTeammates);
            Controls.Add(btnRateSystem);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "SurveyForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "إستبيان الـ Sprint";
            ResumeLayout(false);
        }
        private Label lblTitle;
    }
}
