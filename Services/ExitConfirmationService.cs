namespace Evaluation_App.Services
{
    public static class ExitConfirmationService
    {
        public static bool ConfirmExit()
        {
            var result = MessageBox.Show(
                "هل أنت متأكد أنك تريد إغلاق التطبيق؟",
                "تأكيد الخروج",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            return result == DialogResult.Yes;
        }
    }
}
