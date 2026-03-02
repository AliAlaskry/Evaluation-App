using Evaluation_App.Forms;

namespace Evaluation_App
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (AuthService.TryAutoLogin())
            {
                Application.Run(new MainMenuForm());
                return;
            }

            Application.Run(new LoginForm());
        }
    }
}
