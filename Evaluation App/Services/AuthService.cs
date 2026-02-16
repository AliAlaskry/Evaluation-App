namespace Evaluation_App.Services
{
    public static class AuthService
    {
        private static Employee? currentUser;
        public static Employee CurrentUser => currentUser;

        public static void Login(Employee user)
        {
            if (user == null)
                return;

            currentUser = user;
        }

        public static void Logout()
        {
            currentUser = null;
        }
    }
}