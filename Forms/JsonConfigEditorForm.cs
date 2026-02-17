using Evaluation_App.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Evaluation_App.Forms;

public class JsonConfigEditorForm : Form
{
    private readonly string _filePath;
    private readonly TreeView _tree = new() { Dock = DockStyle.Fill, HideSelection = false };
    private readonly TextBox _value = new() { Dock = DockStyle.Top, Height = 28 };
    private readonly Button _btnUpdate = new() { Text = "تحديث الحقل" };
    private readonly Button _btnAdd = new() { Text = "إضافة حقل" };
    private readonly Button _btnDelete = new() { Text = "حذف الحقل" };
    private readonly Button _btnBack = new() { Text = "رجوع" };
    private readonly Button _btnSave = new() { Text = "حفظ" };
    private readonly Button _btnReloadFile = new() { Text = "تحميل من الملف" };
    private readonly Button _btnLoadJson = new() { Text = "تحميل JSON من ملف" };
    private JObject _root = new();
    private bool _isDirty;
    private bool _isNavigating;

    public JsonConfigEditorForm(string title, string filePath)
    {
        _filePath = filePath;
        Text = title;
        Width = 1000;
        Height = 650;
        StartPosition = FormStartPosition.CenterScreen;
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;

        var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
        topPanel.Controls.AddRange(new Control[] { _btnUpdate, _btnAdd, _btnDelete });

        var bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 45 };
        bottomPanel.Controls.AddRange(new Control[] { _btnBack, _btnSave, _btnReloadFile, _btnLoadJson });

        Controls.Add(_tree);
        Controls.Add(_value);
        Controls.Add(topPanel);
        Controls.Add(bottomPanel);

        _tree.AfterSelect += (_, _) => _value.Text = _tree.SelectedNode?.Tag?.ToString() ?? string.Empty;
        _btnUpdate.Click += (_, _) => UpdateSelected();
        _btnAdd.Click += (_, _) => AddField();
        _btnDelete.Click += (_, _) => DeleteField();
        _btnSave.Click += (_, _) => SaveChanges();
        _btnReloadFile.Click += (_, _) => ReloadWithConfirm();
        _btnLoadJson.Click += (_, _) => LoadFromExternalJson();
        _btnBack.Click += (_, _) => BackWithConfirm();

        FormClosing += JsonConfigEditorForm_FormClosing;

        LoadFromFile();
    }

    private void JsonConfigEditorForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_isNavigating)
            return;

        if (!ConfirmDiscardIfDirty())
        {
            e.Cancel = true;
            return;
        }

        if (!ExitConfirmationService.ConfirmExit())
        {
            e.Cancel = true;
            return;
        }

        Application.Exit();
    }

    private void LoadFromFile()
    {
        if (!File.Exists(_filePath))
        {
            _root = new JObject();
            RebuildTree();
            return;
        }

        _root = JObject.Parse(File.ReadAllText(_filePath));
        _isDirty = false;
        RebuildTree();
    }

    private void RebuildTree()
    {
        _tree.Nodes.Clear();
        var rootNode = new TreeNode("root") { Tag = _root.ToString(Formatting.None) };
        _tree.Nodes.Add(rootNode);
        AddTokenNodes(rootNode, _root, "root");
        rootNode.Expand();
    }

    private void AddTokenNodes(TreeNode parentNode, JToken token, string path)
    {
        switch (token)
        {
            case JObject obj:
                foreach (var prop in obj.Properties())
                {
                    var childPath = path == "root" ? prop.Name : $"{path}.{prop.Name}";
                    var node = new TreeNode(prop.Name) { Tag = prop.Value.Type is JTokenType.Object or JTokenType.Array ? prop.Value.ToString(Formatting.None) : prop.Value.ToString() };
                    node.Name = childPath;
                    parentNode.Nodes.Add(node);
                    AddTokenNodes(node, prop.Value, childPath);
                }
                break;
            case JArray arr:
                for (int i = 0; i < arr.Count; i++)
                {
                    var childPath = $"{path}[{i}]";
                    var node = new TreeNode($"[{i}]") { Tag = arr[i].Type is JTokenType.Object or JTokenType.Array ? arr[i].ToString(Formatting.None) : arr[i].ToString() };
                    node.Name = childPath;
                    parentNode.Nodes.Add(node);
                    AddTokenNodes(node, arr[i], childPath);
                }
                break;
        }
    }

    private void UpdateSelected()
    {
        var node = _tree.SelectedNode;
        if (node == null || string.IsNullOrWhiteSpace(node.Name) || node.Text == "root")
            return;

        var path = node.Name.Replace("root.", "");
        var token = _root.SelectToken(path);
        if (token == null)
            return;

        if (token.Parent is JProperty p)
        {
            p.Value = ParseValue(_value.Text);
        }
        else if (token.Parent is JArray arr)
        {
            int start = path.LastIndexOf('[');
            int end = path.LastIndexOf(']');
            if (start >= 0 && end > start && int.TryParse(path.Substring(start + 1, end - start - 1), out int index) && index >= 0 && index < arr.Count)
                arr[index] = ParseValue(_value.Text);
        }

        _isDirty = true;
        RebuildTree();
    }

    private void AddField()
    {
        var node = _tree.SelectedNode;
        if (node == null)
            return;

        string key = ShowInputDialog("اسم الحقل", "إضافة");
        if (string.IsNullOrWhiteSpace(key))
            return;

        string val = ShowInputDialog("القيمة", "إضافة");
        var token = node.Text == "root" ? _root as JToken : _root.SelectToken(node.Name.Replace("root.", ""));
        if (token is JObject obj)
            obj[key] = ParseValue(val);
        else if (token is JArray arr)
            arr.Add(ParseValue(val));

        _isDirty = true;
        RebuildTree();
    }

    private void DeleteField()
    {
        var node = _tree.SelectedNode;
        if (node == null || node.Text == "root")
            return;

        if (MessageBox.Show("تأكيد حذف الحقل؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        var token = _root.SelectToken(node.Name.Replace("root.", ""));
        if (token?.Parent is JProperty prop)
            prop.Remove();
        else
            token?.Remove();

        _isDirty = true;
        RebuildTree();
    }

    private void SaveChanges()
    {
        if (!_isDirty)
        {
            MessageBox.Show("لا توجد تغييرات للحفظ.");
            return;
        }

        if (MessageBox.Show("هل تريد حفظ التغييرات؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        File.WriteAllText(_filePath, _root.ToString(Formatting.Indented));
        _isDirty = false;
        MessageBox.Show("تم حفظ الملف.");
    }

    private void ReloadWithConfirm()
    {
        if (!ConfirmDiscardIfDirty())
            return;

        if (MessageBox.Show("سيتم تحميل البيانات الحالية من الملف. متابعة؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        LoadFromFile();
    }

    private void LoadFromExternalJson()
    {
        if (!ConfirmDiscardIfDirty())
            return;

        using var dialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        if (MessageBox.Show("سيتم استبدال البيانات المعروضة بملف JSON المحدد. متابعة؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        _root = JObject.Parse(File.ReadAllText(dialog.FileName));
        _isDirty = true;
        RebuildTree();
    }

    private void BackWithConfirm()
    {
        if (!ConfirmDiscardIfDirty())
            return;

        var form = new ConfigFilesMenuForm();
        form.Show();
        _isNavigating = true;
        Close();
    }

    private bool ConfirmDiscardIfDirty()
    {
        if (!_isDirty)
            return true;

        var result = MessageBox.Show("هناك تغييرات غير محفوظة. هل تريد المتابعة بدون حفظ؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        return result == DialogResult.Yes;
    }


    private static string ShowInputDialog(string text, string caption)
    {
        var prompt = new Form
        {
            Width = 500,
            Height = 170,
            Text = caption,
            StartPosition = FormStartPosition.CenterParent,
            RightToLeft = RightToLeft.Yes,
            RightToLeftLayout = true,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var textLabel = new Label { Left = 20, Top = 20, Text = text, Width = 440 };
        var textBox = new TextBox { Left = 20, Top = 55, Width = 440 };
        var confirmation = new Button { Text = "OK", Left = 380, Width = 80, Top = 95, DialogResult = DialogResult.OK };
        prompt.Controls.AddRange(new Control[] { textLabel, textBox, confirmation });
        prompt.AcceptButton = confirmation;

        return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
    }
    private static JToken ParseValue(string value)
    {
        if (bool.TryParse(value, out bool b)) return new JValue(b);
        if (double.TryParse(value, out double d)) return new JValue(d);
        return new JValue(value);
    }
}
