using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

internal sealed class EntityBase : IEqualityComparer<EntityBase>
{
    #region Constructors
    private EntityBase() { }
    #endregion

    #region private Fields
    private List<EntityBase>? adjoinings;
    private ScoringOptions scoringOptions;
    private double score;

    private EvaluationInstance? evaluationInstance;
    #endregion

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

    #region Public Getonly Props
    [JsonIgnore]
    public bool HasAdjoinings => adjoinings != null && adjoinings.Count > 0;
    [JsonIgnore]
    public List<EntityBase>? Adjoinings => adjoinings;

    [JsonIgnore]
    public int Value { get; set; }
    [JsonIgnore]
    public double Score => score;
    #endregion

    #region Private Methods
    private string? ScoreFormula()
    {
        if (!AddonsConfig.HasValue)
            return null;

        return adjoinings == null ?
            AddonsConfig.Value.SingleScoreFormula : AddonsConfig.Value.CombinedScoreFormula;
    }
    private string DefaultScoreFormula()
    {
        if (RootConfig.HasValue)
            return scoringOptions.SectionFormula;

        if (HasAdjoinings)
            return scoringOptions.DefaultCombinedQuestionScoreFormula;

        return scoringOptions.DefaultQuestionScoreFormula;
    }
    private List<double> AdjoiningsScores()
    {
        if (adjoinings == null)
            return [];

        return [.. adjoinings.Select(a => a.score)];
    }
    private Dictionary<string, FormulaEngine.Value> Variables()
    {
        var temp = new Dictionary<string, FormulaEngine.Value>()
        {
            ["Weight"] = new FormulaEngine.Value(BaseConfig.Weight),
        };

        if (RootConfig.HasValue)
        {
            temp.Add("ChildScore", new ([.. RootConfig.Value.Childs.Select(c => c.score)]));
            temp.Add("ChildWeight", new ([.. RootConfig.Value.Childs.Select(c => c.BaseConfig.Weight)]));
            temp.Add("ChildCount", new (RootConfig.Value.Childs.Count));
        }

        if(ValueConfig.HasValue)
        {
            temp.Add("Value", new(Value));
            temp.Add("Default", new(ValueConfig.Value.DefaultValue));
            temp.Add("MinValue", new(ValueConfig.Value.MinValue));
            temp.Add("MaxValue", new(ValueConfig.Value.MaxValue));
        }

        if(adjoinings != null && adjoinings.Count > 0)
        {
            string formula = AddonsConfig?.SingleScoreFormula ?? scoringOptions.DefaultQuestionScoreFormula;
            double currentScore = FormulaEngine.EvaluateToScalar(formula, temp, Value);
            temp.Add("Scores", new([.. AdjoiningsScores(), currentScore]));
        }

        return temp;
    }
    #endregion

    #region Public Methods
    public void Normalize()
    {
        if (!ValueConfig.HasValue)
            return;

        ValueConfig.Value.Normalize();
        Value = (int)Math.Clamp(Value, ValueConfig.Value.MinValue, ValueConfig.Value.MaxValue);
    }
    public void SetEvaluationInstance(EvaluationInstance evaluationInstance)
    {
        this.evaluationInstance = evaluationInstance;
        this.scoringOptions = evaluationInstance.ScoringOptions;

        if (RootConfig.HasValue)
        {
            var root = RootConfig.Value;
            root.SetChilds(FilterEntities(RootConfig.Value.Childs, evaluationInstance));
            RootConfig = root;

            foreach(var child in RootConfig.Value.Childs)
                child.SetEvaluationInstance(evaluationInstance);
        }

        this.adjoinings = FilterEntities(adjoinings, evaluationInstance);
    }
    public void AddAdjoining(EntityBase adjoining)
    {
        this.adjoinings ??= [];
        this.adjoinings.Add(adjoining);
        this.adjoinings = FilterEntities(adjoinings, evaluationInstance);
    }
    public void CalculateScore()
    {
        if (RootConfig.HasValue)
        {
            foreach (var child in RootConfig.Value.Childs)
                child.CalculateScore();
        }

        string formula = ScoreFormula() ?? DefaultScoreFormula();
        double fallback = ValueConfig.HasValue ? Value : 0;

        score = FormulaEngine.EvaluateToScalar(formula, Variables(), fallback);
    }
    public void Reset()
    {
        if(RootConfig.HasValue)
            foreach (var child in RootConfig.Value.Childs)
                child.Reset();

        if (ValueConfig.HasValue)
            Value = (int)ValueConfig.Value.DefaultValue;

        CalculateScore();
    }
    public EntityBase Clone()
    {
        CalculateScore();

        var temp = new EntityBase()
        {
            BaseConfig = BaseConfig,

            ValueConfig = ValueConfig,

            adjoinings = adjoinings?.Select(a => a.Clone()).ToList(),
            scoringOptions = scoringOptions,
            score = score,

            Value = Value,

            evaluationInstance = evaluationInstance
        };

        if (RootConfig.HasValue)
            temp.RootConfig = RootConfig.Value.Clone();

        if (AddonsConfig != null)
            temp.AddonsConfig = AddonsConfig.Value.Clone();
          
        return temp;
    }
    #endregion

    #region Public Static Methods
    public static List<EntityBase>? FilterEntities(List<EntityBase>? entities,
         EvaluationInstance? evaluationInstance = null)
    {
        if (entities == null)
            return null;

        var temp = entities.Where(c => !c.AddonsConfig.HasValue || 
        !c.AddonsConfig.Value.Include.HasValue || c.AddonsConfig.Value.Include.Value);

        if (evaluationInstance != null)
        {
            if (evaluationInstance.FirstEvaluator != null)
                temp = temp
                    .Where(c =>
                    {
                        bool canInclude = true;

                        if (c.AddonsConfig.HasValue)
                            canInclude &= c.AddonsConfig.Value.CanAsk(evaluationInstance.FirstEvaluator);

                        return canInclude;
                    });

            if (evaluationInstance.BeingEvaluated != null)
                temp = temp
                    .Where(c =>
                    {
                        bool canInclude = true;

                        if (c.AddonsConfig.HasValue)
                            canInclude &= c.AddonsConfig.Value.CanAnswer(evaluationInstance.BeingEvaluated);

                        return canInclude;
                    });
        }

        return [.. temp];
    }
    #endregion

    #region IEqualityComparer Methods
    public bool Equals(EntityBase? x, EntityBase? y)
    {
        if (x == null || y == null) return false;
        return x.BaseConfig.ID.Equals(y.BaseConfig.ID);
    }
    public int GetHashCode([DisallowNull] EntityBase obj)
    {
        return obj.BaseConfig.ID?.GetHashCode() ?? 0;
    }
    #endregion
}

internal struct EntityBaseConfig
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

internal struct RootEntityConfig
{
    #region private Fields
    [JsonProperty("Title")]
    private string title;

    [JsonProperty("ScoreMeaning")]
    private string scoreMeaning;

    [JsonProperty("Entites")]
    private List<EntityBase> childs;
    #endregion

    #region Public Getonly Props
    [JsonIgnore]
    public readonly string Title => title;

    [JsonIgnore]
    public readonly string ScoreMeaning => scoreMeaning;

    [JsonIgnore]
    public readonly bool HasChilds => childs != null && childs.Count > 0;
    [JsonIgnore]
    public readonly List<EntityBase> Childs => childs;

    [JsonIgnore]
    public readonly Dictionary<string, EntityBase> ChildsDict =>
        this.childs.ToDictionary(c => c.BaseConfig.ID, c => c);
    #endregion

    #region Public Methods
    public void SetChilds(IEnumerable<EntityBase> childs)
    {
        this.childs = [.. childs];
    }
    public RootEntityConfig Clone()
    {
        var temp = new RootEntityConfig
        {
            title = title,
            scoreMeaning = scoreMeaning,
            childs = childs.Select(c => c.Clone()).ToList()
        };
        return temp;
    }
    #endregion
}

internal struct ValueEntityConfig
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

internal struct AddonsEntityConfig
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
        return new AddonsEntityConfig
        {
            include = include,

            byTeamLeaderOnly = byTeamLeaderOnly,
            forTeamLeaderOnly = forTeamLeaderOnly,

            whoAsk = [.. WhoAsk ?? []],
            whoNotAsk = [.. whoNotAsk ?? []],

            whoAnswer = [.. whoAnswer ?? []],
            whoNotAnswer = [.. whoNotAnswer ?? []],

            singleScoreFormula = singleScoreFormula,
            combinedScoreFormula = combinedScoreFormula,
        };
    }
    #endregion
}