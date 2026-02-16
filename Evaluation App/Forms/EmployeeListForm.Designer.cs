namespace Evaluation_App.Forms
{
    partial class EmployeeListForm
    {
        private System.ComponentModel.IContainer components = null;
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
            lstEmployees = new ListBox();
            btnBack = new Button();
            SuspendLayout();
            // 
            // lstEmployees
            // 
            lstEmployees.Location = new Point(20, 10);
            lstEmployees.Name = "lstEmployees";
            lstEmployees.Size = new Size(510, 424);
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
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "EmployeeListForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Employee List";
            ResumeLayout(false);
        }
    }
}
