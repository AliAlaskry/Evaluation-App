namespace Evaluation_App.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.Button btnLogin;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtCode = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();

            // txtCode
            this.txtCode.Location = new System.Drawing.Point(50, 30);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(200, 22);

            // btnLogin
            this.btnLogin.Location = new System.Drawing.Point(50, 70);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(100, 30);
            this.btnLogin.Text = "تسجيل الدخول";
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);

            // LoginForm
            this.ClientSize = new System.Drawing.Size(300, 150);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.btnLogin);
            this.Name = "LoginForm";
            this.Text = "تسجيل الدخول";
        }
    }

}