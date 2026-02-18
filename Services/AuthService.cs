using Newtonsoft.Json;

namespace Evaluation_App.Services
{
    public static class AuthService
    {
        private const string RememberedLoginFileName = "remembered_login.json";

        private static readonly string RememberedLoginPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Session", RememberedLoginFileName);

        private static Employee? currentUser;
        public static Employee CurrentUser => currentUser!;

        public static bool KeepLoggedIn { get; private set; }

        public static void Login(Employee user, bool keepLoggedIn = false)
        {
            if (user == null)
                return;

            currentUser = user;
            KeepLoggedIn = keepLoggedIn;

            SaveRememberedLogin();
            EvaluationService.ResetAll();
        }

        public static bool TryAutoLogin()
        {
            if (!File.Exists(RememberedLoginPath))
                return false;

            try
            {
                var json = File.ReadAllText(RememberedLoginPath);
                var rememberedCode = JsonConvert.DeserializeObject<string>(json);
                if (string.IsNullOrWhiteSpace(rememberedCode))
                    return false;

                var employee = EmployeeService.GetEmployeeByCode(rememberedCode);
                if (employee == null)
                    return false;

                currentUser = employee;
                KeepLoggedIn = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Logout()
        {
            currentUser = null;
            KeepLoggedIn = false;

            if (File.Exists(RememberedLoginPath))
                File.Delete(RememberedLoginPath);
        }

        private static void SaveRememberedLogin()
        {
            string dataDirectory = Path.GetDirectoryName(RememberedLoginPath)!;
            if (!Directory.Exists(dataDirectory))
                Directory.CreateDirectory(dataDirectory);

            if (!KeepLoggedIn)
            {
                if (File.Exists(RememberedLoginPath))
                    File.Delete(RememberedLoginPath);
                return;
            }

            var json = JsonConvert.SerializeObject(currentUser?.Code ?? string.Empty);
            File.WriteAllText(RememberedLoginPath, json);
        }
    }
}