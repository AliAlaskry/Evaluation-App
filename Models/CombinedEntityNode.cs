using Newtonsoft.Json;

public class CombinedEntityNode : IEntityNode
{
    #region Constructor
    public CombinedEntityNode(IEntityNode source, ScoringOptions scoringOptions)
    {
        ConfigEntityId = source.ConfigEntityId;
        ConfigEntity = source.ConfigEntity;
        this.scoringOptions = scoringOptions;
    }
    #endregion

    #region Private Fields
    double score;

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
    public double Weight => BaseConfig.Weight;

    public List<double> SourceScores { get; set; } = new();

    [JsonIgnore]
    public bool HasChilds => Childs?.Count > 0;

    [JsonProperty("Childs")]
    public List<CombinedEntityNode>? Childs { get; set; }

    [JsonIgnore]
    public IReadOnlyList<IEntityNode> ReadonlyChilds => Childs ?? [];

    [JsonIgnore]
    public Dictionary<string, CombinedEntityNode>? ChildsDict { get; set; }

    [JsonIgnore]
    public double Score => score;

    [JsonIgnore]
    public int Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    #endregion

    #region Public Props 
    [JsonProperty("Config Entity Id")]
    public string ConfigEntityId { get; private set; }
    #endregion

    #region Public Methods
    public void CalculateCombinedScore()
    {
        if (Childs != null)
        {
            foreach (var child in Childs)
                child.CalculateCombinedScore();
        }

        string formula = ScoreFormula() ?? DefaultScoreFormula();
        score = FormulaEngine.EvaluateToScalar(formula, Variables(), 0);
    }
    #endregion

    #region Private Methods
    private string? ScoreFormula()
    {
        if (!AddonsConfig.HasValue)
            return null;

        return AddonsConfig.Value.CombinedScoreFormula;
    }

    private string DefaultScoreFormula()
    {
        bool isSection = RootConfig.HasValue;

        return isSection
            ? scoringOptions.SectionFormula
            : scoringOptions.DefaultCombinedQuestionScoreFormula;
    }

    private Dictionary<string, FormulaEngine.Value> Variables()
    {
        var vars = new Dictionary<string, FormulaEngine.Value>
        {
            ["Weight"] = new FormulaEngine.Value(BaseConfig.Weight),
            ["Scores"] = new FormulaEngine.Value(SourceScores)
        };

        if (Childs != null && Childs.Count > 0)
        {
            vars["ChildScore"] = new FormulaEngine.Value(Childs.Select(c => c.Score).ToList());
            vars["ChildWeight"] = new FormulaEngine.Value(Childs.Select(c => c.Weight).ToList());
        }

        return vars;
    }
    #endregion
}