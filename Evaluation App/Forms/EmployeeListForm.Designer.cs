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
            exportExcel_btn = new Button();
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
            btnBack.Text = "رجوع";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_Click;
            // 
            // button1
            // 
            exportExcel_btn.Location = new Point(20, 446);
            exportExcel_btn.Name = "button1";
            exportExcel_btn.Size = new Size(100, 32);
            exportExcel_btn.TabIndex = 3;
            exportExcel_btn.Text = "تصدير Excel";
            exportExcel_btn.UseVisualStyleBackColor = true;
            exportExcel_btn.Click += exportExcel_Click;
            // 
            // EmployeeListForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(550, 490);
            Controls.Add(btnBack);
            Controls.Add(exportExcel_btn);
            Controls.Add(lstEmployees);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "EmployeeListForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "قائمة الموظفين";
            ResumeLayout(false);
        }
        private Button exportExcel_btn;
    }
}
