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

        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Anchor = AnchorStyles.None,
            Padding = new Padding(10)
        };
        panel.Controls.AddRange(new Control[] { _btnSystem, _btnEmployee, _btnBack });

        var host = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1
        };
        host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        host.Controls.Add(panel, 0, 0);
        Controls.Add(host);

        string projectPath = Directory.GetParent(Application.StartupPath).Parent.Parent.Parent.FullName;

        _btnSystem.Click += (_, _) => Navigate(new JsonConfigEditorForm("System Config", Path.Combine(projectPath, "Data", "system_evaluation_config.json")));
        _btnEmployee.Click += (_, _) => Navigate(new JsonConfigEditorForm("Employee Config", Path.Combine(projectPath, "Data", "employee_evaluation_config.json")));
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
