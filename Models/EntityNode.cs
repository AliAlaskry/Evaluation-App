using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

public sealed class EntityNode : IEqualityComparer<EntityNode>, IEntityNode
{
    #region Constructors
    public EntityNode(ConfigEntity configEntity, ScoringOptions scoringOptions)
    {
        this.ConfigEntityId = configEntity.BaseConfig.ID;
        this.ConfigEntity = configEntity;
        this.scoringOptions = scoringOptions;

        if (ConfigEntity.RootConfig.HasValue)
        {
            childs ??= [];
            foreach (var entity in ConfigEntity.RootConfig.Value.Childs)
                childs.Add(new EntityNode(entity, scoringOptions));
            childsDict = childs.ToDictionary(o => o.ConfigEntityId, o => o);
        }
    }
    #endregion

    #region Private Fields
    [JsonProperty("Childs")]
    private List<EntityNode>? childs;
    private Dictionary<string, EntityNode>? childsDict;

    private double score;

    private ScoringOptions scoringOptions;
    #endregion

    #region Public Getonly Props
    [JsonIgnore]
    public ConfigEntity ConfigEntity { get; private set; }

    [JsonIgnore]
    public EntityBaseConfig BaseConfig => ConfigEntity.BaseConfig;

    [JsonIgnore]
    public RootEntityConfig? RootConfig => ConfigEntity.RootConfig;

    [JsonIgnore]
    public ValueEntityConfig? ValueConfig => ConfigEntity.ValueConfig;

    [JsonIgnore]
    public AddonsEntityConfig? AddonsConfig => ConfigEntity.AddonsConfig;

    [JsonIgnore]
    public bool Valuable => ValueConfig.HasValue;

    [JsonIgnore]
    public bool HasChilds => Childs != null && childs?.Count > 0;

    [JsonIgnore]
    public List<EntityNode> Childs => childs ?? [];

    [JsonIgnore]
    public Dictionary<string, EntityNode>? ChildsDict => childsDict;

    [JsonIgnore]
    public IReadOnlyList<IEntityNode> ReadonlyChilds => childs ?? [];

    [JsonIgnore]
    public double Score => score;
    #endregion

    #region Public Props
    [JsonProperty("Config Entity Id")]
    public string ConfigEntityId { get; private set; }

    [JsonProperty("Value")]
    public int Value { get; set; }

    #endregion

    #region Private Methods
    private string? ScoreFormula()
    {
        if (!AddonsConfig.HasValue)
            return null;

        return AddonsConfig.Value.SingleScoreFormula;
    }
    private string DefaultScoreFormula()
    {
        if (RootConfig.HasValue)
            return scoringOptions.SectionFormula;

        return scoringOptions.DefaultQuestionScoreFormula;
    }
    private Dictionary<string, FormulaEngine.Value> Variables()
    {
        var temp = new Dictionary<string, FormulaEngine.Value>()
        {
            ["Weight"] = new FormulaEngine.Value(BaseConfig.Weight),
        };

        if (HasChilds)
        {
            temp.Add("ChildScore", new ([.. childs.Select(c => c.score)]));
            temp.Add("ChildWeight", new ([.. childs.Select(c => c.BaseConfig.Weight)]));
            temp.Add("ChildCount", new (childs.Count));
        }

        if(Valuable)
        {
            temp.Add("Value", new(Value));
            temp.Add("Default", new(ValueConfig.Value.DefaultValue));
            temp.Add("MinValue", new(ValueConfig.Value.MinValue));
            temp.Add("MaxValue", new(ValueConfig.Value.MaxValue));
        }

        return temp;
    }
    #endregion

    #region Public Methods
    public void PostJsonParsing(ConfigEntity configEntity, ScoringOptions scoringOptions)
    {
        this.ConfigEntityId = configEntity.BaseConfig.ID;
        this.ConfigEntity = configEntity;
        this.scoringOptions = scoringOptions;

        if (HasChilds)
            foreach (var child in childs)
            {
                configEntity = ConfigEntity.RootConfig.Value.Childs
                    .Find(o => o.BaseConfig.ID.Equals(child.ConfigEntityId));
                child.PostJsonParsing(configEntity, scoringOptions);
            }
    }

    public void CalculateScore()
    {
        if (HasChilds)
            foreach (var child in childs)
                child.CalculateScore();

        string formula = ScoreFormula() ?? DefaultScoreFormula();
        double fallback = ValueConfig.HasValue ? Value : 0;

        score = FormulaEngine.EvaluateToScalar(formula, Variables(), fallback);
    }
    public void Reset()
    {
        if(HasChilds)
            foreach (var child in childs)
                child.Reset();

        if (Valuable)
            Value = (int)ValueConfig.Value.DefaultValue;

        CalculateScore();
    }
    public void RandomizeValues(Random? rnd = null)
    {
        rnd ??= new Random();

        if (Valuable)
        {
            int min = (int)ValueConfig.Value.MinValue;
            int max = (int)ValueConfig.Value.MaxValue;

            if (max < min)
                (min, max) = (max, min);

            Value = rnd.Next(min, max + 1);
        }

        if (HasChilds)
        {
            foreach (var child in childs)
                child.RandomizeValues(rnd);
        }

        CalculateScore();
    }
    public EntityNode Clone()
    {
        CalculateScore();

        var clonedChildren = childs?.Select(c => c.Clone()).ToList();
       
        var temp = new EntityNode(ConfigEntity, scoringOptions)
        {
            childs = clonedChildren,
            childsDict = clonedChildren?.ToDictionary(c => c.ConfigEntityId, c => c),
            Value = Value,
            score = score,
        };
          
        return temp;
    }
    #endregion

    #region IEqualityComparer Methods
    public bool Equals(EntityNode? x, EntityNode? y)
    {
        if (x == null || y == null) return false;
        return x.BaseConfig.ID.Equals(y.BaseConfig.ID);
    }
    public int GetHashCode([DisallowNull] EntityNode obj)
    {
        return obj.BaseConfig.ID?.GetHashCode() ?? 0;
    }
    #endregion
}
