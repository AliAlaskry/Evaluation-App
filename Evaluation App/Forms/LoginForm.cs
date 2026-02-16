using Evaluation_App.Services;

namespace Evaluation_App.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            var user = EmployeeService.GetEmployeeByCode(code);

            if (user == null)
            {
                MessageBox.Show("كود الموظف غير صحيح أو غير مشارك في التقييم");
                return;
            }

            if (user.IsTeamLead)
            {
                // طلب كلمة المرور للمدير
                string password = Prompt.ShowDialog("أدخل كلمة المرور للمدير", "تسجيل دخول المدير");
                if (password != "@#159357@#") // ضع هنا كلمة المرور الحقيقية للمدير
                {
                    MessageBox.Show("كلمة المرور غير صحيحة");
                    return;
                }
            }

            // تسجيل الدخول
            AuthService.Login(user, chkKeepLogged.Checked);

            // فتح القائمة الرئيسية بعد تسجيل الدخول
            var mainMenuForm = new MainMenuForm();
            mainMenuForm.Show();
            this.Hide();
        }

    }
}
