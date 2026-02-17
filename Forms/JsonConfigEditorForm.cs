using Evaluation_App.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Evaluation_App.Forms;

public class JsonConfigEditorForm : Form
{
    private readonly string _filePath;
    private readonly TreeView _tree = new() { Dock = DockStyle.Fill, HideSelection = false };
    private readonly TextBox _value = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly Label _lblPath = new() { Dock = DockStyle.Top, Height = 26, TextAlign = ContentAlignment.MiddleLeft };
    private readonly Label _lblType = new() { Dock = DockStyle.Top, Height = 26, TextAlign = ContentAlignment.MiddleLeft };
    private readonly Button _btnRefresh = new() { Text = "تحديث" };
    private readonly Button _btnEdit = new() { Text = "تعديل" };
    private readonly Button _btnAdd = new() { Text = "إضافة" };
    private readonly Button _btnDelete = new() { Text = "حذف الحقل" };
    private readonly Button _btnBack = new() { Text = "رجوع" };
    private readonly Button _btnSave = new() { Text = "حفظ" };
    private readonly Button _btnLoadJson = new() { Text = "تحميل JSON من ملف" };
    private readonly Button _btnApply = new() { Text = "Apply", Dock = DockStyle.Bottom, Height = 34 };
    private readonly SplitContainer _splitContainer = new()
    {
        Dock = DockStyle.Fill,
        Orientation = Orientation.Vertical,
        SplitterDistance = 420
    };
    private JObject _root = new();
    private bool _isDirty;
    private bool _isNavigating;
    private bool _isEditing;

    public JsonConfigEditorForm(string title, string filePath)
    {
        _filePath = ResolveToProjectDataFile(filePath);
        Text = title;
        Width = 1000;
        Height = 650;
        StartPosition = FormStartPosition.CenterScreen;
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;

        var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
        topPanel.Controls.AddRange(new Control[] { _btnRefresh, _btnEdit, _btnAdd, _btnDelete });

        var bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 45 };
        bottomPanel.Controls.AddRange(new Control[] { _btnBack, _btnSave, _btnLoadJson });

        _splitContainer.Panel1.Controls.Add(_tree);
        _splitContainer.Panel2.Controls.Add(_value);
        _splitContainer.Panel2.Controls.Add(_btnApply);
        _splitContainer.Panel2.Controls.Add(_lblType);
        _splitContainer.Panel2.Controls.Add(_lblPath);

        Controls.Add(_splitContainer);
        Controls.Add(topPanel);
        Controls.Add(bottomPanel);

        _tree.AfterSelect += (_, _) => PopulateEditorFromSelection();
        _btnRefresh.Click += (_, _) => ReloadWithConfirm();
        _btnEdit.Click += (_, _) => StartEditSelected();
        _btnAdd.Click += (_, _) => AddField();
        _btnDelete.Click += (_, _) => DeleteField();
        _btnSave.Click += (_, _) => SaveChanges();
        _btnLoadJson.Click += (_, _) => LoadFromExternalJson();
        _btnBack.Click += (_, _) => BackWithConfirm();
        _btnApply.Click += (_, _) => ConfirmAndApplyCurrentEdit();
        _tree.NodeMouseClick += Tree_NodeMouseClick;
        _tree.MouseDown += Tree_MouseDown;

        FormClosing += JsonConfigEditorForm_FormClosing;

        SetEditorVisible(false);
        LoadFromFile();
    }

    private void Tree_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
    {
        _tree.SelectedNode = e.Node;
        if (_isEditing)
            SetEditorVisible(true);
    }

    private void Tree_MouseDown(object? sender, MouseEventArgs e)
    {
        if (_tree.HitTest(e.Location).Node != null)
            return;

        _tree.SelectedNode = null;
        _lblPath.Text = "Path:";
        _lblType.Text = "Type:";
        _value.Text = string.Empty;
        SetEditorVisible(false);
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
        var rootNode = new TreeNode("obj: root") { Name = "root", Tag = new NodeData("root", _root) };
        _tree.Nodes.Add(rootNode);
        AddTokenNodes(rootNode, _root, "root");
        rootNode.Expand();
        _tree.SelectedNode = rootNode;
    }

    private void AddTokenNodes(TreeNode parentNode, JToken token, string path)
    {
        switch (token)
        {
            case JObject obj:
                foreach (var prop in obj.Properties())
                {
                    var childPath = path == "root" ? prop.Name : $"{path}.{prop.Name}";
                    var node = new TreeNode(BuildNodeText(prop.Name, prop.Value)) { Tag = new NodeData(childPath, prop.Value) };
                    node.Name = childPath;
                    parentNode.Nodes.Add(node);
                    AddTokenNodes(node, prop.Value, childPath);
                }
                break;
            case JArray arr:
                for (int i = 0; i < arr.Count; i++)
                {
                    var childPath = $"{path}[{i}]";
                    var item = arr[i];
                    var node = new TreeNode(BuildNodeText($"[{i}]", item)) { Tag = new NodeData(childPath, item) };
                    node.Name = childPath;
                    parentNode.Nodes.Add(node);
                    AddTokenNodes(node, item, childPath);
                }
                break;
        }
    }

    private void PopulateEditorFromSelection()
    {
        if (!_isEditing)
            return;

        if (_tree.SelectedNode?.Tag is not NodeData nodeData)
        {
            _lblPath.Text = "Path:";
            _lblType.Text = "Type:";
            _value.Text = string.Empty;
            return;
        }

        _lblPath.Text = $"Path: {nodeData.Path}";
        _lblType.Text = $"Type: {nodeData.Token.Type}";
        _value.Text = nodeData.Token.Type is JTokenType.Object or JTokenType.Array
            ? nodeData.Token.ToString(Formatting.Indented)
            : nodeData.Token.ToString();
    }

    private void StartEditSelected()
    {
        if (_tree.SelectedNode == null)
            return;

        _isEditing = true;
        SetEditorVisible(true);
        PopulateEditorFromSelection();
        _value.Focus();
    }

    private void ConfirmAndApplyCurrentEdit()
    {
        if (!_isEditing)
            return;

        if (MessageBox.Show("Apply this edit?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        ApplyCurrentEdit();
    }

    private void ApplyCurrentEdit()
    {
        var node = _tree.SelectedNode;
        if (node == null || string.IsNullOrWhiteSpace(node.Name) || node.Name == "root")
            return;

        var path = node.Name.Replace("root.", "");
        var token = _root.SelectToken(path);
        if (token == null)
            return;

        var newValue = ParseValueForToken(_value.Text, token.Type);
        if (token.Parent is JProperty p)
        {
            p.Value = newValue;
        }
        else if (token.Parent is JArray arr)
        {
            int start = path.LastIndexOf('[');
            int end = path.LastIndexOf(']');
            if (start >= 0 && end > start && int.TryParse(path.Substring(start + 1, end - start - 1), out int index) && index >= 0 && index < arr.Count)
                arr[index] = newValue;
        }

        _isDirty = true;
        RebuildTree();
        if (_tree.Nodes.Count > 0)
        {
            SelectNodeByName(path == "root" ? "root" : $"root.{path}");
        }

        PopulateEditorFromSelection();
    }

    private void AddField()
    {
        var node = _tree.SelectedNode;
        if (node == null)
            return;

        var token = node.Name == "root" ? _root as JToken : _root.SelectToken(node.Name.Replace("root.", ""));
        if (token is not JObject and not JArray)
        {
            MessageBox.Show("Cannot add under a field entry.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var addType = ShowDropdownDialog("اختر نوع الإضافة", "Add Type", new[] { "json", "obj", "array", "field" });
        if (string.IsNullOrWhiteSpace(addType))
            return;

        var newToken = addType switch
        {
            "json" => PromptForJsonToken(),
            "obj" => new JObject(),
            "array" => new JArray(),
            "field" => PromptForTypedFieldToken(),
            _ => null
        };

        if (newToken == null)
            return;

        if (token is JObject obj)
        {
            string key = ShowInputDialog("اسم المفتاح", "إضافة");
            if (string.IsNullOrWhiteSpace(key))
                return;

            obj[key] = newToken;
        }
        else if (token is JArray arr)
        {
            arr.Add(newToken);
        }

        _isDirty = true;
        RebuildTree();
    }

    private void DeleteField()
    {
        var node = _tree.SelectedNode;
        if (node == null || node.Name == "root")
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
            MessageBox.Show("there is no changes", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        if (_isDirty && MessageBox.Show("هناك تغييرات غير محفوظة. هل تريد المتابعة؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        LoadFromFile();
    }

    private void LoadFromExternalJson()
    {
        if (_isDirty && MessageBox.Show("هناك تغييرات غير محفوظة. هل تريد المتابعة؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
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
        if (_isDirty)
        {
            var result = MessageBox.Show("هناك تغييرات غير محفوظة. نعم = حفظ، لا = بدون حفظ، إلغاء = البقاء", "تأكيد", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
                return;

            if (result == DialogResult.Yes)
            {
                SaveChanges();
                if (_isDirty)
                    return;
            }
        }

        var form = new ConfigFilesMenuForm();
        form.Show();
        _isNavigating = true;
        Close();
    }

    private void SetEditorVisible(bool isVisible)
    {
        _splitContainer.Panel2Collapsed = !isVisible;
    }

    private void SelectNodeByName(string name)
    {
        foreach (TreeNode rootNode in _tree.Nodes)
        {
            var found = FindNodeRecursive(rootNode, name);
            if (found != null)
            {
                _tree.SelectedNode = found;
                found.EnsureVisible();
                return;
            }
        }
    }

    private static TreeNode? FindNodeRecursive(TreeNode node, string name)
    {
        if (node.Name == name)
            return node;

        foreach (TreeNode child in node.Nodes)
        {
            var found = FindNodeRecursive(child, name);
            if (found != null)
                return found;
        }

        return null;
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

    private static JToken? PromptForNewToken(string text)
    {
        var raw = ShowInputDialog(text, "إضافة");
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        try
        {
            return JToken.Parse(raw);
        }
        catch
        {
            return ParseValue(raw);
        }
    }

    private static JToken? PromptForJsonToken()
    {
        var raw = ShowInputDialog("أدخل JSON", "Add JSON");
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        try
        {
            return JToken.Parse(raw);
        }
        catch
        {
            MessageBox.Show("Invalid JSON value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    private static JToken? PromptForTypedFieldToken()
    {
        var fieldType = ShowDropdownDialog("اختر نوع الحقل", "Field Type", new[] { "int", "float", "boolean", "string" });
        if (string.IsNullOrWhiteSpace(fieldType))
            return null;

        var rawValue = ShowInputDialog("ادخل القيمة", "Field Value");
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        return fieldType switch
        {
            "int" when int.TryParse(rawValue, out var i) => new JValue(i),
            "float" when double.TryParse(rawValue, out var d) => new JValue(d),
            "boolean" when bool.TryParse(rawValue, out var b) => new JValue(b),
            "string" => new JValue(rawValue),
            _ => InvalidFieldTypeValue()
        };
    }

    private static JToken? InvalidFieldTypeValue()
    {
        MessageBox.Show("Invalid value for selected field type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return null;
    }

    private static string ShowDropdownDialog(string text, string caption, IReadOnlyList<string> options)
    {
        var prompt = new Form
        {
            Width = 430,
            Height = 185,
            Text = caption,
            StartPosition = FormStartPosition.CenterParent,
            RightToLeft = RightToLeft.Yes,
            RightToLeftLayout = true,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var textLabel = new Label { Left = 20, Top = 20, Text = text, Width = 380 };
        var comboBox = new ComboBox
        {
            Left = 20,
            Top = 52,
            Width = 380,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        comboBox.Items.AddRange(options.Cast<object>().ToArray());
        if (comboBox.Items.Count > 0)
            comboBox.SelectedIndex = 0;

        var confirmation = new Button { Text = "OK", Left = 320, Width = 80, Top = 95, DialogResult = DialogResult.OK };
        prompt.Controls.AddRange(new Control[] { textLabel, comboBox, confirmation });
        prompt.AcceptButton = confirmation;

        return prompt.ShowDialog() == DialogResult.OK ? comboBox.SelectedItem?.ToString() ?? string.Empty : string.Empty;
    }

    private static string BuildNodeText(string name, JToken token)
    {
        var type = MapDisplayType(token);
        var value = token.Type is JTokenType.Object or JTokenType.Array ? token.ToString(Formatting.None) : token.ToString();
        return $"{name} = {value}, Type = {type}";
    }

    private static string MapDisplayType(JToken token)
    {
        return token.Type switch
        {
            JTokenType.Object => "obj",
            JTokenType.Array => "list",
            JTokenType.Integer => "int",
            JTokenType.Float => "float",
            JTokenType.Boolean => "boolean",
            _ => "string"
        };
    }

    private static JToken ParseValueForToken(string value, JTokenType originalType)
    {
        if (originalType is JTokenType.Object or JTokenType.Array)
        {
            try
            {
                return JToken.Parse(value);
            }
            catch
            {
                return originalType == JTokenType.Object ? new JObject() : new JArray();
            }
        }

        return ParseValue(value);
    }

    private static string ResolveToProjectDataFile(string filePath)
    {
        var candidatePaths = new List<string>
        {
            filePath,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", Path.GetFileName(filePath)),
            Path.Combine(Directory.GetCurrentDirectory(), "Data", Path.GetFileName(filePath))
        };

        var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (directory != null)
        {
            candidatePaths.Add(Path.Combine(directory.FullName, "Data", Path.GetFileName(filePath)));
            if (directory.GetFiles("*.csproj").Any())
                break;

            directory = directory.Parent;
        }

        return candidatePaths.FirstOrDefault(File.Exists) ?? candidatePaths.Last();
    }

    private sealed record NodeData(string Path, JToken Token);
}
