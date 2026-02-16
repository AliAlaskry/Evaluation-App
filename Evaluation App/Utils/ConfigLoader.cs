using Evaluation_App.Services;
using Newtonsoft.Json;

public static class ConfigLoader
{
    private static readonly string BasePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

    private static readonly string EmployeeEvaluationPath =
        Path.Combine(BasePath, "evaluation_config.json");

    private static readonly string SystemConfigPath =
        Path.Combine(BasePath, "system_evaluation.json");

    /// <summary>
    /// تحميل الأقسام والأسئلة الخاصة بتقييم الموظفين
    /// </summary>
    public static List<Section> LoadEmployeeSections()
    {
        return LoadSectionsFromFile(EmployeeEvaluationPath);
    }

    /// <summary>
    /// تحميل الأقسام والأسئلة الخاصة بتقييم النظام
    /// </summary>
    public static List<Section> LoadSystemSections()
    {
        return LoadSectionsFromFile(SystemConfigPath);
    }

    // ================= PRIVATE HELPERS =================

    private static List<Section> LoadSectionsFromFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                MessageBox.Show(
                    $"ملف الإعدادات غير موجود:\n{path}",
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return new List<Section>();
            }

            var json = File.ReadAllText(path);
            var wrapper = JsonConvert.DeserializeObject<SectionWrapper>(json);

            if (wrapper?.Sections == null)
                return new List<Section>();

            return FilterManagerOnly(wrapper.Sections);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "حدث خطأ أثناء تحميل ملف التقييم:\n" + ex.Message,
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return new List<Section>();
        }
    }

    /// <summary>
    /// فلترة الأقسام والأسئلة الخاصة بالمدير
    /// </summary>
    private static List<Section> FilterManagerOnly(List<Section> sections)
    {
        var currentUser = AuthService.CurrentUser;

        // حماية إضافية
        if (currentUser == null)
            return sections;

        return sections
            .Where(section =>
            {
                // 1️⃣ حذف القسم لو ManagerOnly
                if (section.ManagerOnly && !currentUser.IsTeamLead)
                    return false;

                // 2️⃣ حذف الأسئلة فقط لو ManagerOnly
                section.Questions = section.Questions?
                    .Where(q => !q.ManagerOnly || currentUser.IsTeamLead)
                    .ToList() ?? new List<Question>();

                return true;
            })
            .ToList();
    }
}

/// <summary>
/// Wrapper لفك JSON
/// </summary>
public class SectionWrapper
{
    public List<Section> Sections { get; set; }
}
