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

            ConfigLoader.Initialize();
            EmployeeService.Initialize();

            if (AuthService.TryAutoLogin())
            {
                EmployeeService.SetOtherEmployees();
                Application.Run(new MainMenuForm());
                return;
            }

            Application.Run(new LoginForm());
        }

        private static void Test()
        {
            var evaluator = EmployeeService.GetEmployeeByCode("D4");
            var beingEvaluated = EmployeeService.GetEmployeeByCode("D3");

            var e1 = new EvaluationInstance(evaluator, beingEvaluated);
            e1.RandomizeValues();
            EvaluationService.Save(e1);

            EvaluationService.TryLoadEvaluation(evaluator, out var e2, beingEvaluated);
            e2.CalculateScore();

            var result = EvaluationInstanceQaTester.Compare(e1, e2);

            string message =
                "EvaluationInstance instances QA testing result." + Environment.NewLine +
                string.Join(Environment.NewLine, result.Differences);

            var resultMessage = result.AreIdentical ? "Identical" : "Not Identical";
            MessageBox.Show(
                message,
                $"Validation Result: {resultMessage}",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
