using Newtonsoft.Json;

public class EvaluationInstance : IEvaluation
{
    #region Constructors
    [JsonConstructor]
    private EvaluationInstance() { }
    public EvaluationInstance(Employee evaluator, Employee? beingEvaluated = null)
    {
        Evaluator = evaluator;
        evaluatorCode = Evaluator.Code;

        BeingEvaluated = beingEvaluated;
        beingEvaluatedCode = beingEvaluated?.Code;

        entities = [];
        foreach (var entity in FilteredConfigEntities)
            this.entities.Add(new EntityNode(entity, ScoringOptions));
        entitiesDict = entities.ToDictionary(o => o.ConfigEntityId, o => o);
    }
    #endregion

    #region Private Fields
    [JsonProperty("Evaluator Code")]
    private string evaluatorCode;

    [JsonProperty("Being Evaluated Code")]
    private string? beingEvaluatedCode;

    [JsonProperty("Entities")]
    private List<EntityNode> entities;

    private Dictionary<string, EntityNode> entitiesDict;
    #endregion

    #region Private Getonly Props
    private List<double> EntityScores => [.. Entities.Select(e => e.Score)];
    private List<double> EntityWeights => [.. Entities.Select(e => e.BaseConfig.Weight)];

    private ScoringOptions ScoringOptions => IsSystemEvaluation ?
        ConfigLoader.SystemEvaluationOptions.Scoring :
        ConfigLoader.EmployeeEvaluationOptions.Scoring;
    #endregion

    #region Public Get Private Set Props
    [JsonIgnore]
    public List<Employee> Evaluators => throw new NullReferenceException();

    [JsonIgnore]
    public Employee Evaluator { get; private set; }

    [JsonIgnore]
    public Employee? BeingEvaluated { get; private set; }
    #endregion

    #region Public Getonly Props
    [JsonIgnore]
    public bool IsCombined => false;

    [JsonIgnore]
    public List<EntityNode> Entities => entities;

    [JsonIgnore]
    public IReadOnlyDictionary<string, EntityNode> EntitiesDict => entitiesDict;

    [JsonIgnore]
    public IReadOnlyList<IEntityNode> ReadonlyEntities => entities;

    [JsonIgnore]
    public bool IsSystemEvaluation => BeingEvaluated == null;

    [JsonIgnore]
    public List<ConfigEntity> ConfigEntities => IsSystemEvaluation ?
        ConfigLoader.SystemEvaluationEntites : ConfigLoader.EmployeeEvaluationEntites;

    [JsonIgnore]
    public List<ConfigEntity> FilteredConfigEntities =>
        ConfigEntity.FilterConfigEntities(ConfigEntities, this);
    #endregion

    #region Public Props
    public string FinalNote { get; set; } = string.Empty;
    public bool RecommendAsTeamLead { get; set; }
    public bool ReadyToBeAssistantTeamLeader { get; set; }

    [JsonIgnore]
    public double TotalScore { get; set; }

    [JsonIgnore]
    public string FileName => this.GetFileName();
    #endregion

    #region Public Methods
    public void PostJsonParsing()
    {
        Evaluator = EmployeeService.GetEmployeeByCode(evaluatorCode);
        BeingEvaluated = string.IsNullOrEmpty(beingEvaluatedCode) ? null :
            EmployeeService.GetEmployeeByCode(beingEvaluatedCode);

        foreach (var entity in entities)
        {
            var configEntity = FilteredConfigEntities
                .Find(o => o.BaseConfig.ID.Equals(entity.ConfigEntityId));
            entity.PostJsonParsing(configEntity, ScoringOptions);
        }

        entitiesDict = entities.ToDictionary(o => o.ConfigEntityId, o => o);
    }

    public IEntityNode? SearchFor(Predicate<IEntityNode> predicate)
    {
        return SearchFor(Entities, predicate);
    }

    public bool AssistantSectionEnabled()
    {
        bool assistantEnabled = !Evaluator.IsTeamLead &
            ConfigLoader.EmployeeEvaluationOptions.AskPreferTeamLeaderAssistant;
        if (BeingEvaluated != null) assistantEnabled &= !BeingEvaluated.IsTeamLead;
        return assistantEnabled;
    }
    public void CalculateScore()
    {
        foreach (var entity in entities)
            entity.CalculateScore();

        TotalScore = FormulaEngine.EvaluateToScalar(ScoringOptions.TotalFormula, Variables(),
            TotalScore);
    }
    public void Reset()
    {
        foreach (var entity in entities)
            entity.Reset();
    }

    public void RandomizeValues(Random? rnd = null)
    {
        rnd ??= new Random();

        FinalNote = RandomShortString(rnd);
        RecommendAsTeamLead = rnd.Next(0, 2) == 1;
        ReadyToBeAssistantTeamLeader = rnd.Next(0, 2) == 1;

        foreach (var entity in entities)
            entity.RandomizeValues(rnd);

        CalculateScore();
    }
    private static string RandomShortString(Random rnd)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        int len = rnd.Next(5, 15); // short text
        return new string(Enumerable.Range(0, len)
            .Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
    }

    public EvaluationInstance Clone()
    {
        EvaluationInstance temp = new(Evaluator, BeingEvaluated)
        {
            FinalNote = FinalNote,
            RecommendAsTeamLead = RecommendAsTeamLead,
            ReadyToBeAssistantTeamLeader = ReadyToBeAssistantTeamLeader,
            TotalScore = TotalScore
        };
        temp.entities = [.. entities.Select(e => e.Clone())];
        temp.entitiesDict = temp.entities.ToDictionary(e => e.ConfigEntityId, e => e); 
        return temp;
    }
    #endregion

    #region Private Methods
    private Dictionary<string, FormulaEngine.Value> Variables()
    {
        return new Dictionary<string, FormulaEngine.Value>
        {
            {"EntityScore", new FormulaEngine.Value(EntityScores) },
            {"EntityWeight", new FormulaEngine.Value(EntityWeights) },
        };
    }
    private static EntityNode? SearchFor(IReadOnlyList<EntityNode> entities, Predicate<EntityNode> predicate)
    {
        foreach (var entity in entities)
        {
            if (predicate(entity))
                return entity;

            if (entity.RootConfig.HasValue)
            {
                var found = SearchFor(entity.Childs ?? [], predicate);
                if (found != null)
                    return found;
            }
        }

        return null;
    }

    public override bool Equals(object? obj)
    {
        return obj is EvaluationInstance instance &&
               FileName == instance.FileName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FileName);
    }
    #endregion
}
