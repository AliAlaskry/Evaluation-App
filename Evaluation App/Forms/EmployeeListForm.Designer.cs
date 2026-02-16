namespace Evaluation_App.Forms
{
    partial class EmployeeListForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox lstEmployees;
        private System.Windows.Forms.Button btnEvaluate;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnExportMembers;
        private System.Windows.Forms.Button btnExpertReport;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lstEmployees = new System.Windows.Forms.ListBox();
            this.btnEvaluate = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnExportMembers = new System.Windows.Forms.Button();
            this.btnExpertReport = new System.Windows.Forms.Button();

            // lstEmployees
            this.lstEmployees.Location = new System.Drawing.Point(20, 20);
            this.lstEmployees.Size = new System.Drawing.Size(500, 400);
            this.lstEmployees.Name = "lstEmployees";
            this.lstEmployees.ScrollAlwaysVisible = true;

            // btnEvaluate
            this.btnEvaluate.Location = new System.Drawing.Point(20, 440);
            this.btnEvaluate.Size = new System.Drawing.Size(120, 30);
            this.btnEvaluate.Name = "btnEvaluate";
            this.btnEvaluate.Text = "تقييم الموظف";
            this.btnEvaluate.Click += new System.EventHandler(this.btnEvaluate_Click);

            // btnBack
            this.btnBack.Location = new System.Drawing.Point(160, 440);
            this.btnBack.Size = new System.Drawing.Size(80, 30);
            this.btnBack.Name = "btnBack";
            this.btnBack.Text = "رجوع";
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);

            // btnExportMembers
            this.btnExportMembers.Location = new System.Drawing.Point(260, 440);
            this.btnExportMembers.Size = new System.Drawing.Size(120, 30);
            this.btnExportMembers.Name = "btnExportMembers";
            this.btnExportMembers.Text = "تصدير الموظفين";
            this.btnExportMembers.Enabled = false;
            this.btnExportMembers.Click += new System.EventHandler(this.btnExportMembers_Click);

            // btnExpertReport
            this.btnExpertReport.Location = new System.Drawing.Point(400, 440);
            this.btnExpertReport.Size = new System.Drawing.Size(120, 30);
            this.btnExpertReport.Name = "btnExpertReport";
            this.btnExpertReport.Text = "تقرير شامل";
            this.btnExpertReport.Enabled = false;
            this.btnExpertReport.Click += new System.EventHandler(this.btnExpertReport_Click);

            // EmployeeListForm
            this.ClientSize = new System.Drawing.Size(550, 500);
            this.Controls.Add(this.lstEmployees);
            this.Controls.Add(this.btnEvaluate);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnExportMembers);
            this.Controls.Add(this.btnExpertReport);
            this.Name = "EmployeeListForm";
            this.Text = "قائمة الموظفين";
        }
    }
}
