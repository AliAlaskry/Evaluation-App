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
        _filePath = ResolveToProjectDataFile(filePath);
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

        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 420
        };

        splitContainer.Panel1.Controls.Add(_tree);
        splitContainer.Panel2.Controls.Add(_value);
        splitContainer.Panel2.Controls.Add(_lblType);
        splitContainer.Panel2.Controls.Add(_lblPath);

        Controls.Add(splitContainer);
        Controls.Add(topPanel);
        Controls.Add(bottomPanel);

        _tree.AfterSelect += (_, _) => PopulateEditorFromSelection();
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
                    var labelPrefix = prop.Value.Type switch
                    {
                        JTokenType.Object => "obj",
                        JTokenType.Array => "list",
                        _ => "field"
                    };
                    var valuePreview = prop.Value.Type is JTokenType.Object or JTokenType.Array
                        ? string.Empty
                        : $" = {prop.Value}";

                    var node = new TreeNode($"{labelPrefix}: {prop.Name}{valuePreview}") { Tag = new NodeData(childPath, prop.Value) };
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
                    var itemKind = item.Type switch
                    {
                        JTokenType.Object => "obj",
                        JTokenType.Array => "list",
                        _ => "field"
                    };
                    var valuePreview = item.Type is JTokenType.Object or JTokenType.Array ? string.Empty : $" = {item}";

                    var node = new TreeNode($"{itemKind}: [{i}]{valuePreview}") { Tag = new NodeData(childPath, item) };
                    node.Name = childPath;
                    parentNode.Nodes.Add(node);
                    AddTokenNodes(node, item, childPath);
                }
                break;
        }
    }

    private void PopulateEditorFromSelection()
    {
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

    private void UpdateSelected()
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
    }

    private void AddField()
    {
        var node = _tree.SelectedNode;
        if (node == null)
            return;

        var token = node.Name == "root" ? _root as JToken : _root.SelectToken(node.Name.Replace("root.", ""));
        if (token is JObject obj)
        {
            string key = ShowInputDialog("اسم الحقل", "إضافة");
            if (string.IsNullOrWhiteSpace(key))
                return;

            string val = ShowInputDialog("القيمة", "إضافة");
            obj[key] = ParseValue(val);
        }
        else if (token is JArray arr)
        {
            string val = ShowInputDialog("قيمة العنصر", "إضافة عنصر للقائمة");
            arr.Add(ParseValue(val));
        }
        else
        {
            MessageBox.Show("يمكن الإضافة فقط داخل كائن (obj) أو قائمة (list).", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
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
