using Newtonsoft.Json;

namespace Evaluation_App.Services
{
    public static class AuthService
    {
        private const string RememberedLoginFileName = "remembered_login.json";

        private static readonly string RememberedLoginPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", RememberedLoginFileName);

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
