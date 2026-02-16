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

            if (keepLoggedIn)
                SaveRememberedLogin(user.Code);
            else
                ClearRememberedLogin();
        }

        public static bool TryAutoLogin()
        {
            var code = LoadRememberedLoginCode();
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var user = EmployeeService.GetEmployeeByCode(code);
            if (user == null)
            {
                ClearRememberedLogin();
                return false;
            }

            currentUser = user;
            KeepLoggedIn = true;
            return true;
        }

        public static void Logout()
        {
            currentUser = null;
            KeepLoggedIn = false;
            ClearRememberedLogin();
        }

        private static void SaveRememberedLogin(string code)
        {
            try
            {
                var dir = Path.GetDirectoryName(RememberedLoginPath);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var payload = new RememberedLogin { Code = code };
                File.WriteAllText(
                    RememberedLoginPath,
                    JsonConvert.SerializeObject(payload, Formatting.Indented));
            }
            catch
            {
                // intentionally ignored to avoid blocking login flow
            }
        }

        private static string? LoadRememberedLoginCode()
        {
            try
            {
                if (!File.Exists(RememberedLoginPath))
                    return null;

                var json = File.ReadAllText(RememberedLoginPath);
                var payload = JsonConvert.DeserializeObject<RememberedLogin>(json);
                return payload?.Code;
            }
            catch
            {
                return null;
            }
        }

        private static void ClearRememberedLogin()
        {
            try
            {
                if (File.Exists(RememberedLoginPath))
                    File.Delete(RememberedLoginPath);
            }
            catch
            {
                // intentionally ignored to avoid crashing logout/login flow
            }
        }

        private class RememberedLogin
        {
            public string Code { get; set; } = string.Empty;
        }
    }
}
