namespace Evaluation_App.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblCodeTitle;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.CheckBox chkKeepLogged;
        private System.Windows.Forms.Button btnLogin;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblCodeTitle = new Label();
            txtCode = new TextBox();
            chkKeepLogged = new CheckBox();
            btnLogin = new Button();
            SuspendLayout();
            // 
            // lblCodeTitle
            // 
            lblCodeTitle.AutoSize = true;
            lblCodeTitle.Location = new Point(50, 17);
            lblCodeTitle.Name = "lblCodeTitle";
            lblCodeTitle.Size = new Size(88, 15);
            lblCodeTitle.TabIndex = 0;
            lblCodeTitle.Text = "Employee code";
            // 
            // txtCode
            // 
            txtCode.Location = new Point(50, 48);
            txtCode.Name = "txtCode";
            txtCode.PlaceholderText = "Enter employee code ...";
            txtCode.RightToLeft = RightToLeft.No;
            txtCode.Size = new Size(200, 23);
            txtCode.TabIndex = 1;
            // 
            // chkKeepLogged
            // 
            chkKeepLogged.AutoSize = true;
            chkKeepLogged.Location = new Point(50, 88);
            chkKeepLogged.Name = "chkKeepLogged";
            chkKeepLogged.Size = new Size(105, 19);
            chkKeepLogged.TabIndex = 2;
            chkKeepLogged.Text = "Keep logged in";
            chkKeepLogged.UseVisualStyleBackColor = true;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(94, 113);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(112, 32);
            btnLogin.TabIndex = 3;
            btnLogin.Text = "Login";
            btnLogin.Click += btnLogin_Click;
            // 
            // LoginForm
            // 
            ClientSize = new Size(344, 171);
            Controls.Add(lblCodeTitle);
            Controls.Add(txtCode);
            Controls.Add(chkKeepLogged);
            Controls.Add(btnLogin);
            MinimumSize = new Size(360, 210);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login Page";
            ResumeLayout(false);
            PerformLayout();
        }
    }

}
