using DocumentFormat.OpenXml.Vml.Office;
using Newtonsoft.Json;

public struct ConfigEntity
{
    #region Public Fields
    [JsonProperty("Base")]
    public EntityBaseConfig BaseConfig;

    [JsonProperty("Root")]
    public RootEntityConfig? RootConfig;

    [JsonProperty("Value")]
    public ValueEntityConfig? ValueConfig;

    [JsonProperty("Addons")]
    public AddonsEntityConfig? AddonsConfig;
    #endregion

    #region Public Static Methods
    public static List<ConfigEntity>? FilterConfigEntities(List<ConfigEntity>? entities,
         EvaluationInstance? evaluationInstance = null)
    {
        if (entities == null)
            return null;

        var temp = entities.Where(c =>
        {
            return !c.AddonsConfig.HasValue 
                ||!c.AddonsConfig.Value.Include.HasValue 
                || c.AddonsConfig.Value.Include.Value;
        }).ToList();

        for(int i = 0; i < temp.Count; i++)
        {
            var entity = temp[i];
            if (entity.RootConfig.HasValue)
            {
                var root = entity.RootConfig.Value;
                var childs = FilterConfigEntities(root.Childs, evaluationInstance);
                root.SetChilds(childs);
                entity.SetRoot(root); 
            }
            temp[i] = entity;
        }


        if (evaluationInstance != null)
        {
            if (evaluationInstance.Evaluator != null)
                temp = temp
                    .Where(c =>
                    {
                        bool canInclude = true;

                        if (c.AddonsConfig.HasValue)
                            canInclude &= c.AddonsConfig.Value.CanAsk(evaluationInstance.Evaluator);

                        return canInclude;
                    }).ToList();

            if (evaluationInstance.BeingEvaluated != null)
                temp = temp
                    .Where(c =>
                    {
                        bool canInclude = true;

                        if (c.AddonsConfig.HasValue)
                            canInclude &= c.AddonsConfig.Value.CanAnswer(evaluationInstance.BeingEvaluated);

                        return canInclude;
                    }).ToList();
        }

        return temp;
    }
    #endregion

    #region Public Methods
    public void NormalizeAll()
    {
        ValueConfig?.Normalize();
        if (RootConfig.HasValue)
            foreach (var entity in RootConfig.Value.Childs)
                entity.NormalizeAll();
    }
    public void SetRoot(RootEntityConfig root)
    {
        RootConfig = root;
    }
    public ConfigEntity Clone()
    {
        return new ConfigEntity
        {
            BaseConfig = BaseConfig,
            RootConfig = RootConfig?.Clone(),
            ValueConfig = ValueConfig,
            AddonsConfig = AddonsConfig?.Clone()
        };
    }
    #endregion
}

public struct EntityBaseConfig
{
    #region private Fields
    [JsonProperty("Id")]
    private string id;
    [JsonProperty("Weight")]
    private double weight;
    #endregion

    #region Public Getonly Props
    [JsonIgnore]
    public readonly string ID => id;
    [JsonIgnore]
    public readonly double Weight => weight;
    #endregion
}

public struct RootEntityConfig
{
    #region private Fields
    [JsonProperty("Title")]
    private string title;

    [JsonProperty("ScoreMeaning")]
    private string scoreMeaning;

    [JsonProperty("Entites")]
    private List<ConfigEntity> childs;
    #endregion

    #region Public Getonly Props
    [JsonIgnore]
    public readonly string Title => title;

    [JsonIgnore]
    public readonly string ScoreMeaning => scoreMeaning;

    [JsonIgnore]
    public readonly bool HasChilds => childs != null && childs.Count > 0;

    [JsonIgnore]
    public readonly List<ConfigEntity> Childs => childs;
    #endregion

    #region Public Methods
    public void SetChilds(List<ConfigEntity> childs)
    {
        this.childs = childs;
    }
    public RootEntityConfig Clone()
    {
        var temp = new RootEntityConfig
        {
            title = title,
            scoreMeaning = scoreMeaning,
            childs = [.. childs.Select(c => c.Clone())]
        };
        return temp;
    }
    #endregion
}

public struct ValueEntityConfig
{
    #region private Fields
    [JsonProperty("Body")]
    private string body;

    [JsonProperty("MinValueMeaning")]
    private string minValueMeaning;
    [JsonProperty("MaxValueMeaning")]
    private string maxValueMeaning;

    [JsonProperty("MinValue")]
    private double? minValue;
    [JsonProperty("MaxValue")]
    private double? maxValue;
    [JsonProperty("DefaultValue")]
    private double? defaultValue;
    #endregion

    #region Public Getonly Props
    [JsonIgnore]
    public readonly string Body => body;

    [JsonIgnore]
    public readonly string MinValueMeaning => minValueMeaning;
    [JsonIgnore]
    public readonly string MaxValueMeaning => maxValueMeaning;

    [JsonIgnore]
    public readonly double MinValue => minValue ?? 0;
    [JsonIgnore]
    public readonly double MaxValue => maxValue ?? 100;
    [JsonIgnore]
    public readonly double DefaultValue => defaultValue ?? MinValue;
    #endregion

    #region Public Methods
    public void Normalize()
    {
        if (maxValue < minValue)
            (minValue, maxValue) = (maxValue, minValue);

        defaultValue = Math.Clamp(DefaultValue, MinValue, MaxValue);
    }
    #endregion
}

public struct AddonsEntityConfig
{
    #region private Fields
    [JsonProperty("Include")]
    private bool? include;

    [JsonProperty("ByTeamLeaderOnly")]
    private bool? byTeamLeaderOnly;
    [JsonProperty("ForTeamLeaderOnly")]
    private bool? forTeamLeaderOnly;

    [JsonProperty("WhoAsk")]
    private List<string>? whoAsk;
    [JsonProperty("WhoNotAsk")]
    private List<string>? whoNotAsk;

    [JsonProperty("WhoAnswer")]
    private List<string>? whoAnswer;
    [JsonProperty("WhoNotAnswer")]
    private List<string>? whoNotAnswer;

    [JsonProperty("ScoreFormula")]
    private string? singleScoreFormula;
    [JsonProperty("CombinedScoreFormula")]
    private string? combinedScoreFormula;

    #endregion

    #region Public Getonly Props
    [JsonIgnore]
    public readonly bool? Include => include;

    [JsonIgnore]
    public readonly bool? ByTeamLeaderOnly => byTeamLeaderOnly;

    [JsonIgnore]
    public readonly bool? ForTeamLeaderOnly => forTeamLeaderOnly;

    [JsonIgnore]
    public readonly List<string>? WhoAsk => whoAsk;
    [JsonIgnore]
    public readonly List<string>? WhoNotAsk => whoNotAsk;

    [JsonIgnore]
    public readonly List<string>? WhoAnswer => whoAnswer;
    [JsonIgnore]
    public readonly List<string>? WhoNotAnswer => whoNotAnswer;

    [JsonIgnore]
    public readonly string? SingleScoreFormula => singleScoreFormula;
    [JsonIgnore]
    public readonly string? CombinedScoreFormula => combinedScoreFormula;
    #endregion

    #region Public Methods
    public bool CanAsk(Employee evaluator)
    {
        bool can = !byTeamLeaderOnly.HasValue || !byTeamLeaderOnly.Value || evaluator.IsTeamLead;

        if (WhoAsk != null && WhoAsk.Count > 0)
            can &= WhoAsk.Any(w => w.Equals(evaluator.Code));

        if (WhoNotAsk != null && WhoNotAsk.Count > 0)
            can &= !WhoNotAsk.Contains(evaluator.Code);

        return can;
    }
    public bool CanAnswer(Employee beingEvaluated)
    {
        bool can = !forTeamLeaderOnly.HasValue || !forTeamLeaderOnly.Value || beingEvaluated.IsTeamLead;
        if (WhoAnswer != null && WhoAnswer.Count > 0)
            can &= WhoAnswer.Any(w => w.Equals(beingEvaluated.Code));

        if (WhoNotAnswer != null && WhoNotAnswer.Count > 0)
            can &= !WhoNotAnswer.Contains(beingEvaluated.Code);

        return can;
    }
    public AddonsEntityConfig Clone()
    {
        var temp = this;

        temp.whoAsk = [.. whoAsk ?? []];
        temp.whoNotAsk = [.. whoNotAsk ?? []];

        temp.whoAnswer = [.. whoAnswer ?? []];
        temp.whoNotAnswer = [.. whoNotAnswer ?? []];

        return temp;
    }
    #endregion
}