namespace Evaluation_App.Forms
{
    partial class EmployeeListForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private ListBox lstEmployees;
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
            lstEmployees = new ListBox();
            btnBack = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(550, 60);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Employee List";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lstEmployees
            // 
            lstEmployees.Location = new Point(20, 70);
            lstEmployees.Name = "lstEmployees";
            lstEmployees.Size = new Size(510, 364);
            lstEmployees.TabIndex = 1;
            lstEmployees.DoubleClick += lstEmployees_DoubleClick;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(430, 445);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(100, 32);
            btnBack.TabIndex = 2;
            btnBack.Text = "Back";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_Click;
            // 
            // EmployeeListForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(550, 490);
            Controls.Add(btnBack);
            Controls.Add(lstEmployees);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "EmployeeListForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Employee List";
            ResumeLayout(false);
        }
    }
}
