using Newtonsoft.Json;

namespace Evaluation_App.Services
{
    public static class AuthService
    {
        private const string RememberedLoginFileName = "remembered_login.json";

        private static readonly string RememberedLoginPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", RememberedLoginFileName);

        private static Employee? currentUser;
        public static Employee CurrentUser => currentUser!;

        public static bool KeepLoggedIn { get; private set; }

        public static void Login(Employee user, bool keepLoggedIn = false)
        {
            if (user == null)
                return;

            currentUser = user;
            KeepLoggedIn = keepLoggedIn;

<<<<<<< Updated upstream
            SaveRememberedLogin();
=======
            if (keepLoggedIn)
                SaveRememberedLogin(user.Code);
            else
                ClearRememberedLogin();
>>>>>>> Stashed changes
        }

        public static bool TryAutoLogin()
        {
<<<<<<< Updated upstream
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
=======
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
>>>>>>> Stashed changes
        }

        public static void Logout()
        {
            currentUser = null;
            KeepLoggedIn = false;
<<<<<<< Updated upstream

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
=======
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
>>>>>>> Stashed changes
        }
    }
}