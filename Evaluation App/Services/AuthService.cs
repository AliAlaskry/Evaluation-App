namespace Evaluation_App.Services
{
    public static class AuthService
    {
        private static Employee? currentUser;
        public static Employee CurrentUser => currentUser;

        public static bool KeepLoggedIn { get; private set; }

        public static void Login(Employee user, bool keepLoggedIn = false)
        {
            if (user == null)
                return;

            currentUser = user;
            KeepLoggedIn = keepLoggedIn;
        }

        public static void Logout()
        {
            currentUser = null;
            KeepLoggedIn = false;
        }
    }
}
