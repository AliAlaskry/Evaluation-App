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
            this.lblCodeTitle = new System.Windows.Forms.Label();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.chkKeepLogged = new System.Windows.Forms.CheckBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // lblCodeTitle
            this.lblCodeTitle.AutoSize = true;
            this.lblCodeTitle.Location = new System.Drawing.Point(24, 24);
            this.lblCodeTitle.Name = "lblCodeTitle";
            this.lblCodeTitle.Size = new System.Drawing.Size(69, 16);
            this.lblCodeTitle.TabIndex = 0;
            this.lblCodeTitle.Text = "كود الموظف";

            // txtCode
            this.txtCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCode.Location = new System.Drawing.Point(24, 46);
            this.txtCode.Name = "txtCode";
            this.txtCode.PlaceholderText = "ادخل كود الموظف";
            this.txtCode.Size = new System.Drawing.Size(352, 22);
            this.txtCode.TabIndex = 1;

            // chkKeepLogged
            this.chkKeepLogged.AutoSize = true;
            this.chkKeepLogged.Location = new System.Drawing.Point(24, 80);
            this.chkKeepLogged.Name = "chkKeepLogged";
            this.chkKeepLogged.Size = new System.Drawing.Size(121, 20);
            this.chkKeepLogged.TabIndex = 2;
            this.chkKeepLogged.Text = "Keep me logged";
            this.chkKeepLogged.UseVisualStyleBackColor = true;

            // btnLogin
            this.btnLogin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogin.Location = new System.Drawing.Point(264, 112);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(112, 32);
            this.btnLogin.TabIndex = 3;
            this.btnLogin.Text = "تسجيل الدخول";
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);

            // LoginForm
            this.AcceptButton = this.btnLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 170);
            this.Controls.Add(this.lblCodeTitle);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.chkKeepLogged);
            this.Controls.Add(this.btnLogin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.MinimumSize = new System.Drawing.Size(360, 210);
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "تسجيل الدخول";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }

}
