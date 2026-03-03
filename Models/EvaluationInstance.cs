using Newtonsoft.Json;

internal class EvaluationInstance
{
    #region Constructors
    [JsonConstructor]
    public EvaluationInstance(Employee evaluator)
    {
        this.Evaluators = [evaluator];
        this.BeingEvaluated = null;
        this.entities = EntityBase.FilterEntities(ConfigLoader.SystemEvaluationEntites, this) ?? [];
        this.scoringOptions = ConfigLoader.SystemEvaluationOptions.Scoring;
      
        RefereshEntitiesDict();

        foreach (var entity in entities)
            entity.SetEvaluationInstance(this);

        CalculateScore();
    }
    public EvaluationInstance(Employee evaluator, Employee beingEvaluated)
    {
        this.Evaluators = [evaluator];
        this.BeingEvaluated = beingEvaluated;
        this.entities = EntityBase.FilterEntities(ConfigLoader.EmployeeEvaluationEntites, this) ?? [];
        this.scoringOptions = ConfigLoader.EmployeeEvaluationOptions.Scoring;
       
        RefereshEntitiesDict();

        foreach (var entity in entities)
            entity.SetEvaluationInstance(this);

        CalculateScore();
    }
    public EvaluationInstance(List<EvaluationInstance> evals, string filename)
    {
        this.Evaluators = [.. evals.SelectMany(o => o.Evaluators)];
        this.BeingEvaluated = evals[0].BeingEvaluated;
        this.EntitiesDict = new();
        this.scoringOptions = evals[0].scoringOptions;
        this.filename = filename;

        foreach (var eval in evals)
        {
            eval.CalculateScore();
            AppendEntities(EntitiesDict, eval.Entities);
        }

        this.entities = EntitiesDict.Values.ToList();
        CalculateScore();
    }
    #endregion

    #region Private Fields
    [JsonProperty("Entities")]
    private List<EntityBase> entities;

    private ScoringOptions scoringOptions;

    private string? filename;
    #endregion

    #region Private Getonly Props
    private List<double> EntityScores => [.. Entities.Select(e => e.Score)];
    private List<double> EntityWeights => [.. Entities.Select(e => e.BaseConfig.Weight)];
    #endregion

    #region Public Get Private Set Props
    [JsonProperty("Evaluator")]
    public List<Employee> Evaluators { get; private set; }
    [JsonProperty("BeingEvaluated")]
    public Employee? BeingEvaluated { get; private set; }

    [JsonIgnore]
    public Dictionary<string, EntityBase> EntitiesDict { get; private set; } = new();
    #endregion

    #region Public Getonly Props
    [JsonIgnore]
    public Employee FirstEvaluator => Evaluators[0];

    [JsonIgnore]
    public ScoringOptions ScoringOptions => scoringOptions;

    [JsonIgnore]
    public List<EntityBase> Entities => entities;

    [JsonIgnore]
    public bool IsSystemEvaluationInstance => BeingEvaluated == null;
    #endregion

    #region Public Props
    public string FinalNote { get; set; } = string.Empty;
    public bool RecommendAsTeamLead { get; set; }
    public bool ReadyToBeAssistantTeamLeader { get; set; }

    [JsonIgnore]
    public double TotalScore { get; set; }
    [JsonIgnore]
    public string FileNameWithoutExension
    {
        get
        {
            if (Evaluators.Count > 1)
                return filename ?? "";
            return this.GetFileName();
        }
    }
    [JsonIgnore]
    public string FileNameWithExension => FileNameWithoutExension.AppendExcelExtension();
    #endregion

    #region Public Methods
    public List<EntityBase> GetAllRootEntities()
    {
        return GetAllRootEntities(entities);
    }
    public EntityBase? SearchFor(Func<EntityBase, bool> predicate)
    {
        return SearchFor(Entities, predicate);
    }
    public bool AssistantSectionEnabled()
    {
        bool assistantEnabled = ConfigLoader.EmployeeEvaluationOptions.AskPreferTeamLeaderAssistant;
        if (BeingEvaluated == null) assistantEnabled &= Evaluators.Count == 1
                && !FirstEvaluator.IsTeamLead;
        else assistantEnabled &= !BeingEvaluated.IsTeamLead;
        return assistantEnabled;
    }
    public void RefereshEntitiesDict()
    {
        EntitiesDict = this.Entities.ToDictionary(c => c.BaseConfig.ID, c => c);
    }
    public void CalculateScore()
    {
        foreach (var entity in entities)
            entity.CalculateScore();

        TotalScore = FormulaEngine.EvaluateToScalar(scoringOptions.TotalFormula, Variables(),
            TotalScore);
    }
    public void Reset()
    {
        foreach (var entity in entities)
            entity.Reset();
    }
    public EvaluationInstance Clone()
    {
        EvaluationInstance temp;
        if (Evaluators.Count > 1)
            temp = new([this], filename ?? "");
        else if (IsSystemEvaluationInstance)
            temp = new(FirstEvaluator);
        else
            temp = new(FirstEvaluator, BeingEvaluated);

        temp.entities = [..entities.Select(e => e.Clone())];
        return temp;
    }
    #endregion

    #region Private Methods
    private void AppendEntities(Dictionary<string, EntityBase> dict, List<EntityBase> newList)
    {
        foreach (var entity in newList)
        {
            var temp = entity.Clone();
            if (dict.TryGetValue(temp.BaseConfig.ID, out var old))
            {
                old.AddAdjoining(temp);

                if (old.RootConfig.HasValue && temp.RootConfig.HasValue)
                {
                    var newRoot = temp.RootConfig.Value.Clone();
                    AppendEntities(old.RootConfig.Value.ChildsDict, newRoot.Childs);
                }
            }
            else
            {
                temp.SetEvaluationInstance(this);
                dict.Add(temp.BaseConfig.ID, temp);
            }
        }
    }
    private Dictionary<string, FormulaEngine.Value> Variables()
    {
        return new Dictionary<string, FormulaEngine.Value>
        {
            {"EntityScore", new FormulaEngine.Value(EntityScores) },
            {"EntityWeight", new FormulaEngine.Value(EntityWeights) },
        };
    }
    private List<EntityBase> GetAllRootEntities(List<EntityBase> entities)
    {
        var list = new List<EntityBase>();
        foreach (var entity in entities)
        {
            if(entity.RootConfig.HasValue)
            {
                list.Add(entity.Clone());
                list.AddRange(GetAllRootEntities(entity.RootConfig.Value.Childs));
            }
        }
        return list;
    }
    private EntityBase? SearchFor(List<EntityBase> entities, Func<EntityBase, bool> predicate)
    {
        foreach (var entity in entities)
        {
            if (predicate(entity))
                return entity;

            if (entity.RootConfig.HasValue)
            {
                var found = SearchFor(entity.RootConfig.Value.Childs, predicate);
                if (found != null)
                    return found;
            }
        }

        return null;
    }
    #endregion
}
