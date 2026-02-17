using Evaluation_App.Services;

namespace Evaluation_App.Forms;

public class ConfigFilesMenuForm : Form
{
    private readonly Button _btnSystem = new() { Text = "ملف تقييم النظام", Width = 250, Height = 40 };
    private readonly Button _btnEmployee = new() { Text = "ملف تقييم الموظف", Width = 250, Height = 40 };
    private readonly Button _btnBack = new() { Text = "رجوع", Width = 250, Height = 40 };
    private bool _isNavigating;

    public ConfigFilesMenuForm()
    {
        Text = "تعديل ملفات الإعدادات";
        Width = 430;
        Height = 300;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;

        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(80, 40, 80, 20) };
        panel.Controls.AddRange(new Control[] { _btnSystem, _btnEmployee, _btnBack });
        Controls.Add(panel);

        _btnSystem.Click += (_, _) => Navigate(new JsonConfigEditorForm("System Config", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "system_evaluation_config.json")));
        _btnEmployee.Click += (_, _) => Navigate(new JsonConfigEditorForm("Employee Config", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "employee_evaluation_config.json")));
        _btnBack.Click += (_, _) =>
        {
            var form = new MainMenuForm();
            form.Show();
            _isNavigating = true;
            Close();
        };

        FormClosing += (_, e) =>
        {
            if (!_isNavigating && e.CloseReason == CloseReason.UserClosing)
            {
                if (!ExitConfirmationService.ConfirmExit())
                {
                    e.Cancel = true;
                    return;
                }

                Application.Exit();
            }
        };
    }

    private void Navigate(Form form)
    {
        form.Show();
        _isNavigating = true;
        Hide();
    }
}
