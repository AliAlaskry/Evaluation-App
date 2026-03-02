using System.Windows.Forms;

internal static class EvaluationFormHelper
{
    private static FlowLayoutPanel FlowLayout;
    private static Dictionary<string, TrackBar> InputControls;
    private static Dictionary<string, Label> ValueLabels;

    private static CheckBox ChkTeamLead;
    private static TextBox TxtFinalNote;

    public static EvaluationInstance EvaluationInstance;

    public static void Initialize(FlowLayoutPanel flowLayout,
        Dictionary<string, TrackBar> inputControls, Dictionary<string, Label> valueLabels,
        CheckBox chkTeamLead, TextBox txtFinalNote,
        EvaluationInstance evaluationInstance)
    {
        FlowLayout = flowLayout;
        InputControls = inputControls;
        ValueLabels = valueLabels;

        ChkTeamLead = chkTeamLead;
        TxtFinalNote = txtFinalNote;
        
        EvaluationInstance = evaluationInstance;
    }

    public static void LoadSections()
    {
        FlowLayout.Controls.Clear();
        InputControls.Clear();
        ValueLabels.Clear();

        foreach (var entity in EvaluationInstance.Entities)
            LoadEntity(entity);
    }
    private static void LoadEntity(EntityBase entity)
    {
        LoadRootEntity(entity);
        LoadValueEntity(entity);

        if (entity.RootConfig.HasValue)
            foreach (var child in entity.RootConfig.Value.Childs)
                LoadEntity(child);
    }
    private static void LoadRootEntity(EntityBase entity)
    {
        if (!entity.RootConfig.HasValue)
            return;

        var lblSection = new Label
        {
            Text = entity.RootConfig.Value.Title,
            AutoSize = false,
            Width = FlowLayout.Width - 25,
            Height = 28,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight
        };
        FlowLayout.Controls.Add(lblSection);
    }
    private static void LoadValueEntity(EntityBase entity)
    {
        if (!entity.ValueConfig.HasValue)
            return;

        var panel = new Panel
        {
            Width = FlowLayout.Width - 25,
            Height = 116,
            RightToLeft = RightToLeft.Yes
        };

        var lblQ = new Label
        {
            Text = entity.ValueConfig.Value.Body,
            AutoSize = false,
            Width = panel.Width,
            Height = 24,
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(0, 0)
        };

        int min = (int)Math.Round(entity.ValueConfig.Value.MinValue);
        int max = (int)Math.Round(entity.ValueConfig.Value.MaxValue);
        int def = (int)Math.Round(entity.ValueConfig.Value.DefaultValue);
        def = Math.Clamp(def, min, max);

        var slider = new TrackBar
        {
            Minimum = min,
            Maximum = max,
            Value = def,
            TickStyle = TickStyle.None,
            AutoSize = false,
            Width = panel.Width - 20,
            Height = 30,
            Location = new Point(10, 42),
            Name = entity.BaseConfig.ID,
            RightToLeft = RightToLeft.No
        };
        slider.MouseDown += BlockMouse;
        slider.MouseUp += BlockMouse;
        slider.MouseWheel += BlockMouse;

        const int hintLabelY = 76;
        int hintLabelWidth = (slider.Width / 2) - 4;

        var minLabel = new Label
        {
            Text = entity.ValueConfig.Value.MinValueMeaning,
            AutoSize = false,
            Width = hintLabelWidth,
            Height = 30,
            Location = new Point(slider.Left, hintLabelY),
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = Color.DimGray
        };

        var maxLabel = new Label
        {
            Text = entity.ValueConfig.Value.MaxValueMeaning,
            AutoSize = false,
            Width = hintLabelWidth,
            Height = 30,
            Location = new Point(slider.Right - hintLabelWidth, hintLabelY),
            TextAlign = ContentAlignment.TopRight,
            ForeColor = Color.DimGray
        };

        var valueLabel = new Label
        {
            Text = string.Empty,
            AutoSize = false,
            Width = 48,
            Height = 20,
            Location = new Point(slider.Left, slider.Top - 18),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };

        slider.ValueChanged += (_, _) => UpdateValueLabelPosition(valueLabel, slider);

        panel.Controls.Add(lblQ);
        panel.Controls.Add(slider);
        panel.Controls.Add(minLabel);
        panel.Controls.Add(maxLabel);
        panel.Controls.Add(valueLabel);
        UpdateValueLabelPosition(valueLabel, slider);
        FlowLayout.Controls.Add(panel);

        InputControls[entity.BaseConfig.ID] = slider;
        ValueLabels[entity.BaseConfig.ID] = valueLabel;
    }
    private static void BlockMouse(object? sender, MouseEventArgs e)
    {
        if (e is HandledMouseEventArgs he)
            he.Handled = true;
    }

    public static void UpdateValueLabelPosition(Label valueLabel, TrackBar slider)
    {
        valueLabel.Text = slider.Value.ToString();

        int valueRange = Math.Max(1, slider.Maximum - slider.Minimum);
        int trackWidth = Math.Max(1, slider.Width - 16);
        double ratio = (slider.Value - slider.Minimum) / (double)valueRange;
        int thumbX = slider.Left + 8 + (int)Math.Round(trackWidth * ratio);

        valueLabel.Left = Math.Clamp(thumbX - (valueLabel.Width / 2), slider.Left, slider.Right - valueLabel.Width);
        valueLabel.Top = slider.Top - 18;
    }

    public static void LoadValues(EvaluationInstance evaluationInstance)
    {
        foreach (var kvp in InputControls)
        {
            if (evaluationInstance.TryGetEntity(kvp.Key, out var entity) && entity.ValueConfig.HasValue)
            {
                kvp.Value.Value = Math.Clamp((int)Math.Round((double)entity.Value),
                    kvp.Value.Minimum, kvp.Value.Maximum);
                UpdateValueLabelPosition(ValueLabels[kvp.Key], kvp.Value);
            }
        }

        TxtFinalNote.Text = evaluationInstance.FinalNote;
        ChkTeamLead.Checked = evaluationInstance.RecommendAsTeamLead;
    }
    private static bool TryGetEntity(this EvaluationInstance instance, string key, out EntityBase entity)
    {
        entity = instance.SearchFor(e => e.BaseConfig.ID.Equals(key));
        return entity != null;
    }

    public static void ApplyInputsToModel()
    {
        foreach (var kvp in InputControls)
            if (EvaluationInstance.TryGetEntity(kvp.Key, out var entity) && entity.ValueConfig.HasValue)
                entity.Value = kvp.Value.Value;

        EvaluationInstance.FinalNote = TxtFinalNote.Text;
        EvaluationInstance.RecommendAsTeamLead = ChkTeamLead.Checked;
    }

    public static bool HasChanges()
    {
        foreach (var kvp in InputControls) 
            if (EvaluationInstance.TryGetEntity(kvp.Key, out var entity) && entity.ValueConfig.HasValue)
                if (entity.Value != kvp.Value.Value)
                return true;
        
        if (EvaluationInstance.RecommendAsTeamLead != ChkTeamLead.Checked)
            return true;

        if (!EvaluationInstance.FinalNote.Equals(TxtFinalNote.Text))
            return true;

        return false;
    }

    public static bool HasAnyInputWithDefaultValue()
    {
        foreach (var kvp in InputControls)
            if (EvaluationInstance.TryGetEntity(kvp.Key, out var entity) && entity.ValueConfig.HasValue)
                if (kvp.Value.Value == entity.ValueConfig.Value.DefaultValue)
                    return true;

        return false;
    }

    public static void Reset()
    {
        if (HasChanges())
        {
            var result = MessageBox.Show(
             "لم يتم حفظ المعلومات. هل تريد الحفظ والمتابعه؟",
             "تنبة قبل التصدير",
             MessageBoxButtons.YesNo,
             MessageBoxIcon.Warning);

            if (result == DialogResult.No)
                return;
        }

        foreach (var kvp in InputControls)
            if (EvaluationInstance.TryGetEntity(kvp.Key, out var entity) && entity.ValueConfig.HasValue)
            {
                var slider = kvp.Value;
                slider.Value = Math.Clamp((int)Math.Round(entity.ValueConfig.Value.DefaultValue),
                    slider.Minimum, slider.Maximum);
            }

        ChkTeamLead.Checked = false;
        TxtFinalNote.Text = string.Empty;

        MessageBox.Show("تمت إعادة الضبط.");
    }

    public static void Load(string title)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx",
            Title = title
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        var temp = EvaluationInstance.Clone();
        if (!ExcelExportService.TryLoadEvaluationInstanceFromExcel(dialog.FileName, temp))
        {
            MessageBox.Show("تعذر تحميل البيانات من ملف Excel المحدد.");
            return;
        }

        LoadValues(temp);

        MessageBox.Show("تم التحميل بنجاح.");
    }

    public static void Save()
    {
        if (!HasChanges())
        {
            MessageBox.Show("لا توجد تغييرات للحفظ.");
            return;
        }

        if (HasAnyInputWithDefaultValue())
        {
            var confirm = MessageBox.Show(
                "هناك عناصر ما زالت على القيم الافتراضية. هل تريد المتابعة بالحفظ؟",
                "تنبيه قبل الحفظ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;
        }
        else
        {
            var confirm = MessageBox.Show(
                "هناك عناصر ما زالت على القيم الافتراضية. هل تريد المتابعة بالحفظ؟",
                "تنبيه قبل الحفظ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;
        }

        ApplyInputsToModel();
        EvaluationInstance.CalculateScore();
        EvaluationService.Save(EvaluationInstance);
        MessageBox.Show("تم الحفظ بنجاح.");
    }

    public static void Generate()
    {
        if (HasChanges())
        {
            var result = MessageBox.Show(
             "لم يتم حفظ المعلومات. هل تريد الحفظ والمتابعه؟",
             "تنبة قبل التصدير",
             MessageBoxButtons.YesNo,
             MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;
        }

        ApplyInputsToModel();
        EvaluationInstance.CalculateScore();
        EvaluationService.Save(EvaluationInstance);

        if (ExcelExportService.TryExportEmployeeEvaluation(EvaluationInstance))
            MessageBox.Show("تم إنشاء تقرير Excel على سطح المكتب.");
    }
}
